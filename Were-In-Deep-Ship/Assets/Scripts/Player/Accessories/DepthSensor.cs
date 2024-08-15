using Unity.Netcode;

public class DepthSensor : NetworkBehaviour
{
    public NetworkVariable<int> Depth = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    void FixedUpdate()
    {
        if (!IsOwner) return;

        Depth.Value = (int)transform.position.y >= 0 ? 0 : -(int)transform.position.y;
    }


}
