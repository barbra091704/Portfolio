using UnityEngine;

public class PlayerVisibility : MonoBehaviour
{
    private SpriteRenderer playerRenderer;

    [SerializeField]
    private float changeSpeed = 1f;
    [SerializeField]
    private LayerMask ignoreLayer;

    private void Start()
    {
        MoveAroundObjects._distanceFromTarget = 3;
        playerRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (EnableUI.isHunter == false)
        {
            Ray ray = new Ray(Camera.main.transform.position, transform.position - Camera.main.transform.position);
            RaycastHit hitInfo;
            bool isVisible = !Physics.Raycast(ray, out hitInfo, 1.8f, ~ignoreLayer);

            if (!isVisible)
            {
                MoveAroundObjects._distanceFromTarget = Mathf.Lerp(MoveAroundObjects._distanceFromTarget, 0.8f, Time.deltaTime * changeSpeed);
            }
        }
    }
}
