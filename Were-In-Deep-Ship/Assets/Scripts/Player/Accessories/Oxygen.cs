using System;
using KWS;
using Unity.Netcode;
using UnityEngine;

public class Oxygen : NetworkBehaviour
{
    public NetworkVariable<float> CurrentOxygenLevel = new(30);
    public NetworkVariable<int> CurrentOxygenTankCapacity = new(30);
    public NetworkVariable<int> DepthBeforeCustomRates = new(50);
    public int defaultOxygenLevel = 30;
    public int defaultDepthRate = 50;
    public int asphyxiationDamage = 8;
    public float damageInterval;
    public float ylevelOffset;
    private float oxygenDepletionRate = 2f;
    private float oxygenTimer = 0;
    private float asphyxiateTimer = 0f;
    private bool InWater;
    private int currentDepth;
    public event Action<int,bool, RpcParams> AsphyxiateEvent;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        
        GetComponent<PlayerMovement>().InWater.OnValueChanged += IsInWater;
        GetComponent<DepthSensor>().Depth.OnValueChanged += UpdateOxygenDepletionRate;

        SetOxygenLevelRpc(defaultOxygenLevel);
        SetOxygenTankCapacityRpc(defaultOxygenLevel);
        SetDepthThresholdRpc(defaultDepthRate);
    }


    public void Update()
    {
        if (!IsOwner) return;

        if (InWater)
        {
            AsphyxiationCheck();
            oxygenTimer += Time.deltaTime;

            if (oxygenTimer >= 3f)
            {
                oxygenTimer = 0f;
                SetOxygenLevelRpc(CurrentOxygenLevel.Value - oxygenDepletionRate);
            }
        }

        if (!WaterSystem.IsPositionUnderWater(new(transform.position.x, transform.position.y + ylevelOffset, transform.position.z)))
        {
            oxygenTimer += Time.deltaTime;
            if (oxygenTimer >= 0.1f && CurrentOxygenLevel.Value < CurrentOxygenTankCapacity.Value)
            {
                oxygenTimer = 0f;
                SetOxygenLevelRpc(CurrentOxygenLevel.Value + 2f);
            }
        }
        
    }

    void UpdateOxygenDepletionRate(int old, int newvalue)
    {
        currentDepth = newvalue;

        if (currentDepth > DepthBeforeCustomRates.Value){ 
            oxygenDepletionRate = FindNearestDepletionRate(currentDepth);
        }
    }

    private float FindNearestDepletionRate(int currentDepth)
    {
        int lastPassedThreshold = -1; // Initialize with a value that represents no threshold passed

        foreach (var depthRate in WaterManager.Singleton.OxygenDepletionRates)
        {
            if (currentDepth >= depthRate.depthThreshold && depthRate.depthThreshold > lastPassedThreshold)
            {
                lastPassedThreshold = depthRate.depthThreshold;
                return depthRate.depletionRate;
            }
        }
        return 0;
    }

    void AsphyxiationCheck()
    {
        if (CurrentOxygenLevel.Value <= 0 || currentDepth > DepthBeforeCustomRates.Value)
        {
            asphyxiateTimer += Time.deltaTime;
            
            if (asphyxiateTimer >= damageInterval)
            {
                asphyxiateTimer = 0f;
                AsphyxiateEvent?.Invoke(asphyxiationDamage, false, default);
            }
        }
        else
        {
            asphyxiateTimer = 0f;
        }
    }
    private void IsInWater(bool previousValue, bool newValue)
    {
        InWater = newValue;
        oxygenTimer = 0;
        asphyxiateTimer = 0;
    }

    [Rpc(SendTo.Server)]
    public void SetDepthThresholdRpc(int value)
    {
        DepthBeforeCustomRates.Value = value;
    }

    [Rpc(SendTo.Server)]
    public void SetOxygenLevelRpc(float value)
    {
        CurrentOxygenLevel.Value = value;
        CurrentOxygenLevel.Value = Mathf.Clamp(CurrentOxygenLevel.Value, 0, CurrentOxygenTankCapacity.Value);
    }
    [Rpc(SendTo.Server)]
    public void SetOxygenTankCapacityRpc(int value)
    {
        CurrentOxygenTankCapacity.Value = value;
    }
}
