using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using KWS;

[Serializable]
public struct OxygenDepletionRate
{
    public int depthThreshold;
    public float depletionRate;
}
public class WaterManager : NetworkBehaviour
{
    public static WaterManager Singleton;
    [SerializeField] private bool UseNetworkBuoyancy;
    [SerializeField] private bool UseNetworkTime;
    public OxygenDepletionRate[] OxygenDepletionRates;
    private WaterSystem waterSystem;
    public void Awake()
    {
        Singleton = Singleton != null && Singleton != this ? null : this;
        waterSystem = GetComponent<WaterSystem>();
    }
    public override void OnNetworkSpawn()
    { 
        WaterSystem.UseNetworkBuoyancy = UseNetworkBuoyancy;
        WaterSystem.UseNetworkTime = UseNetworkTime;
    }
    private void Update() 
    { 
        if (!UseNetworkTime) return;
        WaterSystem.NetworkTime = NetworkManager.ServerTime.TimeAsFloat; 
    }
    public WaterSurfaceData GetWaterData(Vector3 position)
    {
        return waterSystem.GetCurrentWaterSurfaceData(position);
    }
    public float GetWaterHeight(Vector3 position)
    {
        WaterSurfaceData data = waterSystem.GetCurrentWaterSurfaceData(position);
        return data.IsActualDataReady ? data.Position.y : 0;
    }
    public Vector3 GetWaterNormal(Vector3 position)
    {
        WaterSurfaceData data = waterSystem.GetCurrentWaterSurfaceData(position);
        return data.IsActualDataReady ? data.Normal : Vector3.zero;
    }
}
