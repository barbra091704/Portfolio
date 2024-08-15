using UnityEngine;
using Unity.Netcode;

public class Battery : NetworkBehaviour
{
    private IUsable usable;

    public NetworkVariable<int> BatteryLevel = new(100);
    public NetworkVariable<bool> IsDraining = new(false);
    public int MaxBatteryLevel = 100;
    public int drainAmount = 1;
    public float drainInterval = 0.5f;

    private float drainTimer = 0;

    public void Start()
    {
        usable = GetComponent<IUsable>();
    }

    public void Update()
    {
        if (!IsServer || !IsDraining.Value) return;

        HandleBatteryDrain();
    }

    public void HandleBatteryDrain()
    {
        drainTimer += Time.deltaTime;

        if (drainTimer > drainInterval)
        {
            if (BatteryLevel.Value > 0)
            {
                usable.CanUseCheck = true;

                drainTimer = 0;

                DrainBattery(drainAmount);
            }
            else
            {
                usable.CanUseCheck = false;
                usable.OnBatteryDead();
            }
        }
    }

    public void DrainBattery(int amount)
    {
        BatteryLevel.Value -= amount;
        BatteryLevel.Value = Mathf.Clamp(BatteryLevel.Value, 0, MaxBatteryLevel);
    }

    public void ChargeBattery(int amount)
    {
        BatteryLevel.Value += amount;
        BatteryLevel.Value = Mathf.Clamp(BatteryLevel.Value, 0, MaxBatteryLevel);
    }
}
