using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Steamworks;

[Serializable]
public struct Task
{
    public string Name;
    public bool Completed;
    public int Amount;
    public int CurrentAmount;
}

public class Tasks : NetworkBehaviour
{
    public static Tasks Instance;

    public PlayerType playerType;
    public GameObject TabSheet;
    public int taskAmount;
    public Image[] taskIndicators;
    public TMP_Text[] textTaskIndicators;
    public List<Task> currentTasks = new();

    public List<TaskDefinition> availableTaskDefinitions;

    private void Start()
    {
        if (!IsOwner) 
        {
            Instance = null;
            return;
        }

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) 
        {
            enabled = false;
            return;
        }

        SetTasks();
    }

    private void SetTasks()
    {
        // Clear current tasks
        currentTasks.Clear();

        if (playerType == PlayerType.Sniper)
        {
            // Sniper has only one task: Kill all players
            Task sniperTask = new()
            {
                Name = "Kill all Players",
                Completed = false,
            };
            currentTasks.Add(sniperTask);
        }
        else
        {
            List<Task> availableTasks = new();

            int alivePlayerCount = ClassicManager.Instance.CountAlivePlayers(); // Get the number of alive players

            // Populate tasks based on player type and available task definitions
            foreach (var taskDefinition in availableTaskDefinitions)
            {
                if (IsTaskApplicable(taskDefinition, alivePlayerCount))
                {
                    AddTask(taskDefinition, availableTasks);
                }
            }

            // Randomly select tasks, up to the number specified by taskAmount
            List<Task> selectedTasks = RandomlySelectTasks(availableTasks, taskAmount);

            currentTasks.AddRange(selectedTasks);
        }

        UpdateTaskIndicators();
        UpdateTaskTextIndicators();
    }

    private bool IsTaskApplicable(TaskDefinition taskDefinition, int alivePlayerCount)
    {
        if (taskDefinition.taskPrefix == "Kill" && taskDefinition.taskSuffix == "Player")
        {
            return alivePlayerCount > 2;
        }
        if (taskDefinition.taskPrefix == "Frame" && taskDefinition.taskSuffix == "Player")
        {
            return alivePlayerCount > 2;
        }

        return playerType switch
        {
            PlayerType.Assassin => taskDefinition.taskPrefix == "Kill" || taskDefinition.taskPrefix == "Frame",
            PlayerType.Thief => taskDefinition.taskPrefix == "Steal" || taskDefinition.taskPrefix == "Frame" && taskDefinition.taskSuffix == "Civilian",
            PlayerType.Sniper => false,
            _ => false,
        };
    }

    private void AddTask(TaskDefinition taskDefinition, List<Task> taskList)
    {
        int amount = UnityEngine.Random.Range(taskDefinition.minAmount, taskDefinition.maxAmount + 1); // min to max items
        Task task = new()
        {
            Amount = amount,
            Name = $"{taskDefinition.taskPrefix} {amount} {taskDefinition.taskSuffix}s",
            Completed = false,
            CurrentAmount = 0
        };
        taskList.Add(task);
    }

    private List<Task> RandomlySelectTasks(List<Task> availableTasks, int numberOfTasks)
    {
        List<Task> selectedTasks = new();

        if (availableTasks.Count <= numberOfTasks)
        {
            selectedTasks.AddRange(availableTasks);
        }
        else
        {
            System.Random random = new();
            HashSet<int> selectedIndices = new();

            while (selectedIndices.Count < numberOfTasks)
            {
                int index = random.Next(availableTasks.Count);
                selectedIndices.Add(index);
            }

            foreach (int index in selectedIndices)
            {
                selectedTasks.Add(availableTasks[index]);
            }
        }

        return selectedTasks;
    }

    private void UpdateTaskIndicators()
    {
        for (int i = 0; i < taskIndicators.Length; i++)
        {
            if (i < currentTasks.Count)
            {
                taskIndicators[i].gameObject.SetActive(true);
            }
            else
            {
                taskIndicators[i].gameObject.SetActive(false);
            }
        }
    }

    public void UpdateTaskTextIndicators()
    {
        for (int i = 0; i < textTaskIndicators.Length; i++)
        {
            if (i < currentTasks.Count)
            {
                textTaskIndicators[i].gameObject.SetActive(true);
                textTaskIndicators[i].text = currentTasks[i].Name;

                // Set the text color based on whether the task is completed
                textTaskIndicators[i].color = currentTasks[i].Completed ? Color.green : Color.red;
            }
            else
            {
                textTaskIndicators[i].gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TabSheet.SetActive(!TabSheet.activeSelf);
        }
    }

    public void CompleteTask(string taskName)
    {
        if (!IsOwner) return; 

        for (int i = 0; i < currentTasks.Count; i++)
        {
            if (currentTasks[i].Name == taskName && !currentTasks[i].Completed)
            {
                // Mark the task as completed
                Task updatedTask = currentTasks[i];
                updatedTask.Completed = true;
                currentTasks[i] = updatedTask;

                Debug.Log($"Task '{updatedTask.Name}' completed.");

                // Update the text indicator to turn green
                textTaskIndicators[i].color = Color.green;
                taskIndicators[i].color = Color.green;

                if (taskName.StartsWith("Steal"))
                {
                    SpawnAndPlayParticlesRpc(false, transform.position);
                }
                else if (taskName.StartsWith("Kill"))
                {
                    SpawnAndPlayParticlesRpc(true, transform.position);
                }

                // Check if all tasks are complete
                if (AreAllTasksComplete())
                {
                    print("ALL TASKS DONE");
                    ClassicManager.Instance.SetTaskCompletionRpc(NetworkManager.Singleton.LocalClientId, SteamClient.Name);
                }

                return;
            }
        }

        // If the task is not found in the current list, simply do nothing and move on
        Debug.Log($"Task '{taskName}' not found for local player. Ignoring.");
    }

    public bool AreAllTasksComplete()
    {
        foreach (var task in currentTasks)
        {
            if (!task.Completed)
            {
                return false;
            }
        }

        return true;
    }

    [Rpc(SendTo.Everyone)]
    public void SpawnAndPlayParticlesRpc(bool value, Vector3 position) // true is kill false is steal
    {
        if (value)
        {
            Instantiate(ClassicManager.Instance.killParticles.gameObject, position, Quaternion.identity);
        }
        else
        {
            Instantiate(ClassicManager.Instance.stealParticles.gameObject, position, Quaternion.identity);
        }
    }
}
