using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Text.RegularExpressions;

public class Assassin : NetworkBehaviour
{
    public float swingDelay; // The delay between swings in seconds
    public bool canSwing = true;
    public SpriteRenderer knife;
    private Animator knifeAnimator;
    public TMP_Text cooldown;
    private float timeUntilCanSwing; 

    private IEnumerator Start()
    {
        knifeAnimator = knife.GetComponent<Animator>();

        if (!IsOwner) 
        {
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;
            yield break;
        }

        ClassicManager.Instance.mapSpriteRenderer.sortingOrder = 0;
    }

    public void Update()
    {
        if (!IsOwner) return;

        CheckInput();

        // Update the cooldown text if needed
        if (!canSwing)
        {
            UpdateCooldownText();
        }
    }

    private void CheckInput()
    {
        if (Input.GetMouseButtonDown(0) && canSwing && ClassicManager.Instance.IsPlayerDead(OwnerClientId))
        {
            SetKnifeVisibilityRpc(true);
            StartCoroutine(SwingCoroutine());
        }

        if (Input.GetKeyDown(KeyCode.E) && canSwing && ClassicManager.Instance.IsPlayerDead(OwnerClientId))
        {
            StartCoroutine(FrameCoroutine());
        }
    }

    private IEnumerator SwingCoroutine()
    {
        canSwing = false;
        timeUntilCanSwing = swingDelay; // Set the cooldown timer

        knifeAnimator.Play("Swing");

        Swing();

        yield return new WaitForSeconds(0.5f);

        SetKnifeVisibilityRpc(false);

        // Wait for the full swing delay before allowing another swing
        while (timeUntilCanSwing > 0)
        {
            yield return new WaitForSeconds(1f); // Update every second
            timeUntilCanSwing -= 1f;
            UpdateCooldownText(); // Update the text display
        }

        canSwing = true;
        cooldown.text = "Ready"; // Display "Ready" or similar when cooldown is over
    }

    private IEnumerator FrameCoroutine()
    {
        canSwing = false;
        timeUntilCanSwing = swingDelay; // Set the cooldown timer for framing

        Frame();

        yield return new WaitForSeconds(0.5f);

        // Wait for the full delay before allowing another framing
        while (timeUntilCanSwing > 0)
        {
            yield return new WaitForSeconds(1f); // Update every second
            timeUntilCanSwing -= 1f;
            UpdateCooldownText(); // Update the text display
        }

        canSwing = true;
        cooldown.text = "Ready"; // Display "Ready" or similar when cooldown is over
    }

    private void UpdateCooldownText()
    {
        // Update the cooldown text to show the remaining time
        cooldown.text = $"{Mathf.CeilToInt(timeUntilCanSwing)}s";
    }

    private void Swing()
    {
        Collider2D[] overlap = Physics2D.OverlapAreaAll(Interaction.Instance.interactionPoint.bounds.min, Interaction.Instance.interactionPoint.bounds.max);

        if (overlap.Length > 0)
        {
            foreach (var item in overlap)
            {
                if (item.transform.TryGetComponent(out IDamagable damagable))
                {
                    damagable.DamageRpc(100);

                    // Check and increment tasks based on item tags
                    if (item.transform.CompareTag("Player"))
                    {
                        Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} killed: Player");
                        SoundManager.Instance.StartScreamingRpc();
                        return;
                    }
                    else if (item.transform.CompareTag("Civilian"))
                    {
                        IncrementKillTask("Kill", "Civilian");
                        Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} killed: Civilian");
                        SoundManager.Instance.StartScreamingRpc();
                        return;
                    }
                }
            }
        }
    }

    private void Frame()
    {
        Collider2D[] overlap = Physics2D.OverlapAreaAll(Interaction.Instance.interactionPoint.bounds.min, Interaction.Instance.interactionPoint.bounds.max);

        if (overlap.Length > 0)
        {
            foreach (var item in overlap)
            {
                if (item.transform.CompareTag("Player"))
                {
                    Tasks.Instance.SpawnAndPlayParticlesRpc(true, item.transform.position);
                    IncrementKillTask("Frame", "Player");
                    Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} framed: Player");
                    return;
                }
                else if (item.transform.CompareTag("Civilian"))
                {
                    Tasks.Instance.SpawnAndPlayParticlesRpc(true, item.transform.position);
                    IncrementKillTask("Frame", "Civilian");
                    Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} framed: Civilian");
                    return;
                }
            }
        }
    }

    private void IncrementKillTask(string taskPrefix, string targetType)
    {
        // Regular expression to match tasks with dynamic numbers
        Regex regex = new($@"^{taskPrefix} (\d+) {targetType}s?$");

        // Check if the task exists in the current tasks list
        for (int i = 0; i < Tasks.Instance.currentTasks.Count; i++)
        {
            var task = Tasks.Instance.currentTasks[i];
            Match match = regex.Match(task.Name);

            if (match.Success)
            {
                // Extract the number from the task name
                int requiredAmount = int.Parse(match.Groups[1].Value);

                if (task.Name.StartsWith(taskPrefix) && !task.Completed)
                {
                    // Increment the task amount
                    task.CurrentAmount++;
                    Debug.Log($"Incremented task '{task.Name}' to {task.CurrentAmount}");

                    // Update the task in the list
                    Tasks.Instance.currentTasks[i] = task;

                    // Check if the task is complete
                    if (task.CurrentAmount >= requiredAmount)
                    {
                        // Complete the task
                        Tasks.Instance.CompleteTask(task.Name);
                    }

                    return;
                }
            }
        }

        // If the task is not found in the current list, log a message
        Debug.Log("Task not found for local player. Ignoring.");
    }

    [Rpc(SendTo.Everyone)]
    public void SetKnifeVisibilityRpc(bool value)
    {
        knife.enabled = value;
    }
}
