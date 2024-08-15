using System;
using KWS;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum WeatherState
{
    Clear, // Clear Skies Sunny weather with calm seas and clear visibility.
    Misty, // Early morning mist blankets the ocean, creating a tranquil and ethereal atmosphere.
    Currents, // Strong underwater currents that affect player movement and navigation.
    Stormy, // Heavy rain, strong winds, Lighting strikes, and turbulent waves make navigation difficult and dangerous.
    Heatwave, // High temperatures and scorching sun, affecting player stamina and hydration levels.
    ColdWave, // Low tempuratures and freezing water,
    Hailstorm, // Damage players above water slowly.
    Cyclone, // Intense cyclonic winds and massive waves.
    BloodMoon, // increased creature spawn and red water and rain.
}
[Serializable]
public struct WeatherStats{
    public WeatherState Name;
    public int ChanceOf;
}
public class WeatherManager : NetworkBehaviour
{
    public static WeatherManager Singleton;

    public NetworkVariable<WeatherState> CurrentWeatherState = new(WeatherState.Clear);
    public WeatherStats[] WeatherConditions;

    public void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer) GameManager.Singleton.CurrentLocation.OnValueChanged += SetWeatherState;
    }

    private WeatherStats GetRandomWeatherCondition()
    {
        float value = UnityEngine.Random.value * 100;

        for (int i = 0; i < WeatherConditions.Length; i++)
        {
            if (value >= WeatherConditions[i].ChanceOf)
            {
                return WeatherConditions[i];
            }
        }
        return WeatherConditions[0]; 
    }

    private void SetWeatherState(LocationData old, LocationData newValue)
    {
        if (newValue.ID == 0) return;

        CurrentWeatherState.Value = GetRandomWeatherCondition().Name;

        switch (CurrentWeatherState.Value)
        {
            case WeatherState.Clear:
                // Handle Clear weather state
                break;
            case WeatherState.Misty:
                // Handle Misty weather state
                break;
            case WeatherState.Currents:
                // Handle Currents weather state
                break;
            case WeatherState.Stormy:
                // Handle Stormy weather state
                break;
            case WeatherState.Heatwave:
                // Handle Heatwave weather state
                break;
            case WeatherState.ColdWave:
                // Handle ColdWave weather state
                break;
            case WeatherState.Hailstorm:
                // Handle Hailstorm weather state
                break;
            case WeatherState.Cyclone:
                // Handle Cyclone weather state
                break;
            case WeatherState.BloodMoon:
                // Handle BloodMoon weather state
                break;
            default:
                break;
        }
    }
}
