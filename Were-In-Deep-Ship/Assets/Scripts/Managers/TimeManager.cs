using UnityEngine;
using Unity.Netcode;
using System;

public class TimeManager : NetworkBehaviour
{
    public static TimeManager Singleton { get; private set; }

    public Transform sunTransform;
    public NetworkVariable<bool> IsTimeRunning = new(false);
    public NetworkVariable<float> GameTime = new(0);
    public NetworkVariable<float> TotalGameTimeElapsed = new(0);
    public NetworkVariable<int> DaysPassed = new(0);
    public float maxGameTimeInMinutes = 12;
    [Range(0.1f, 10f)]
    public float GameTimeScale = 1;
    private float gameTimescale;
    public Vector3 startRotation;
    public Vector3 endRotation;
    private float maxGameTimeInSeconds;

    public event Action<int> DayEventTriggered;
    public event Action<float> TickEventTriggered;

    void Awake()
    {
        maxGameTimeInSeconds = maxGameTimeInMinutes * 60;

        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }

    public void ResetGameTime()
    {
        if (IsServer)
        {
            IsTimeRunning.Value = false;

            GameTime.Value = 0;

            CancelInvoke(nameof(IncrementGameTime));

            ResetSunRotation();

            //SetDays();
        }
    }

    private void SetDays()
    {
        if (DaysPassed.Value >= QuestManager.Singleton.CurrentQuest.DaysToComplete)
        {
            DaysPassed.Value = 0;
        }
        else 
            DaysPassed.Value++;

        DayEventTriggered?.Invoke(DaysPassed.Value);
    }

    private void Update()
    {
        if (IsTimeRunning.Value)
            RotateSun();
    }

    public void StartGameTime()
    {
        if (IsServer)
        {
            ResetSunRotation();
            IsTimeRunning.Value = true;
            gameTimescale = 1f / GameTimeScale;
            InvokeRepeating(nameof(IncrementGameTime), 0, gameTimescale);
        }
    }

    void IncrementGameTime()
    {
        GameTime.Value++;

        TotalGameTimeElapsed.Value++;

        GameTime.Value = Mathf.Clamp(GameTime.Value, 0, maxGameTimeInSeconds);

        TickEventTriggered?.Invoke(GameTime.Value);

    }

    private void RotateSun()
    {
        // Calculate the lerping progress based on GameTime
        float t = GameTime.Value / maxGameTimeInSeconds;
        float scaledT = t * GameTimeScale; // Apply GameTimeScale directly to lerping progress

        // Perform lerping based on the calculated progress
        Vector3 currentRotation = Vector3.Lerp(startRotation, endRotation, scaledT);
        sunTransform.rotation = Quaternion.Euler(currentRotation);

        // Check if the sun has reached the end rotation
        if (GameTime.Value >= maxGameTimeInSeconds)
        {
            sunTransform.rotation = Quaternion.Euler(endRotation); // Ensure the final rotation is reached exactly
            Debug.Log("Sun Rotation Completed");
        }
    }



    private void ResetSunRotation()
    {
        sunTransform.rotation = Quaternion.Euler(startRotation); // Reset sun's rotation
    }
}
