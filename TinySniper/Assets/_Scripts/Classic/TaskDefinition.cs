using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TaskDefinition", menuName = "Tasks/TaskDefinition")]
public class TaskDefinition : ScriptableObject
{
    public string taskPrefix;
    public string taskSuffix;
    public int minAmount;
    public int maxAmount;
}
