using Unity.Netcode;
public interface IUsable
{
    public bool CanUseCheck { get; set; }

    public void Use(NetworkObject player);

    public void Drop(NetworkObject player);

    public void OnBatteryDead();

}
