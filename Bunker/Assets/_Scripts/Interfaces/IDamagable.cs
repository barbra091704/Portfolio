using Unity.Netcode;

public interface IDamagable
{
    void TakeDamage(int value, bool applyResistance);
}

