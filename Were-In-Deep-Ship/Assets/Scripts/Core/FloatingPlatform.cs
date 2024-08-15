using UnityEngine;

public class FloatingPlatform : MonoBehaviour
{
    [HideInInspector] public Vector3 desiredPosition;
    public float floatOffset = 1;
    public void Update()
    {
        float targetY = WaterManager.Singleton.GetWaterHeight(new(transform.position.x, transform.position.y + floatOffset, transform.position.z));
        desiredPosition = new(0, targetY, 0);
        float boatPosY = desiredPosition.y - transform.position.y;

        transform.position += 6 * Time.deltaTime * new Vector3(desiredPosition.x, boatPosY, desiredPosition.z);
    }
}
