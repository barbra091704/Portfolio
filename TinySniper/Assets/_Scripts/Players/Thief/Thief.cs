using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Thief : NetworkBehaviour
{
    public int stealDelay; // The delay between steals in seconds
    public bool canSteal;
    public SpriteRenderer gloves;
    public TMP_Text cooldown;
    private float timeUntilCanSteal;

    private IEnumerator Start()
    {
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
        if (!canSteal)
        {
            UpdateCooldownText();
        }
    }

    private void CheckInput()
    {
        if (Input.GetMouseButtonDown(0) && canSteal && ClassicManager.Instance.IsPlayerDead(OwnerClientId))
        {
            SetGlovesVisibilityRpc(true);
            StartCoroutine(StealCoroutine());
        }

        if (Input.GetKeyDown(KeyCode.E) && canSteal && ClassicManager.Instance.IsPlayerDead(OwnerClientId))
        {
            StartCoroutine(FrameCoroutine());
        }
    }

    private IEnumerator StealCoroutine()
    {
        canSteal = false;
        timeUntilCanSteal = stealDelay; // Set the cooldown timer

        Steal();

        yield return new WaitForSeconds(0.5f);

        SetGlovesVisibilityRpc(false);

        // Wait for the full steal delay before allowing another steal
        while (timeUntilCanSteal > 0)
        {
            yield return new WaitForSeconds(1f); // Update every second
            timeUntilCanSteal -= 1f;
            UpdateCooldownText(); // Update the text display
        }

        canSteal = true;
        cooldown.text = "Ready"; // Display "Ready" or similar when cooldown is over
    }

    private IEnumerator FrameCoroutine()
    {
        canSteal = false;
        timeUntilCanSteal = stealDelay; // Set the cooldown timer for framing

        Frame();

        yield return new WaitForSeconds(0.5f);

        // Wait for the full delay before allowing another framing
        while (timeUntilCanSteal > 0)
        {
            yield return new WaitForSeconds(1f); // Update every second
            timeUntilCanSteal -= 1f;
            UpdateCooldownText(); // Update the text display
        }

        canSteal = true;
        cooldown.text = "Ready"; // Display "Ready" or similar when cooldown is over
    }

    private void UpdateCooldownText()
    {
        // Update the cooldown text to show the remaining time
        cooldown.text = $"{Mathf.CeilToInt(timeUntilCanSteal)}s";
    }

    private void Steal()
    {
        Collider2D[] overlap = Physics2D.OverlapAreaAll(Interaction.Instance.interactionPoint.bounds.min, Interaction.Instance.interactionPoint.bounds.max);

        if (overlap.Length > 0)
        {
            foreach (var item in overlap)
            {
                if (item.transform.CompareTag("Stealable"))
                {
                    RemoveStolenItemRpc(item.transform.GetComponent<NetworkObject>());

                    // Increment the steal task
                    IncrementTask("Steal", "Items");
                    return;
                }
                else if (item.transform.CompareTag("Valuable"))
                {
                    RemoveStolenItemRpc(item.transform.GetComponent<NetworkObject>());

                    // Increment the valuable steal task
                    IncrementTask("Steal", "Valuables");
                    return;
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
                    Tasks.Instance.SpawnAndPlayParticlesRpc(false, item.transform.position);
                    IncrementTask("Frame", "Players");
                    return;
                }
                else if (item.transform.CompareTag("Civilian"))
                {
                    Tasks.Instance.SpawnAndPlayParticlesRpc(false, item.transform.position);
                    IncrementTask("Frame", "Civilians");
                    return;
                }
            }
        }
    }

    private void IncrementTask(string taskPrefix, string itemType)
    {
        Regex regex = new($@"^{taskPrefix} (\d+) {itemType}s?$");

        for (int i = 0; i < Tasks.Instance.currentTasks.Count; i++)
        {
            var task = Tasks.Instance.currentTasks[i];
            Match match = regex.Match(task.Name);

            if (match.Success)
            {
                int requiredAmount = int.Parse(match.Groups[1].Value);

                if (task.Name.StartsWith(taskPrefix) && !task.Completed)
                {
                    task.CurrentAmount++;
                    Tasks.Instance.currentTasks[i] = task;

                    if (task.CurrentAmount >= requiredAmount)
                    {
                        Tasks.Instance.CompleteTask(task.Name);
                    }

                    return;
                }
            }
        }

        Debug.Log("Task not found for local player. Ignoring.");
    }


    [Rpc(SendTo.Server)]
    public void RemoveStolenItemRpc(NetworkObjectReference reference)
    {
        if (reference.TryGet(out NetworkObject networkObject))
        {
            networkObject.Despawn(true);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SetGlovesVisibilityRpc(bool value)
    {
        gloves.enabled = value;
    }
}
