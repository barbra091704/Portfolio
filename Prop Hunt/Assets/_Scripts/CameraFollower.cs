using Photon.Pun;
using UnityEngine;

public class CameraFollower : MonoBehaviourPun
{
    public Camera cam;
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (photonView.IsMine)
        {
            transform.position = Vector3.SmoothDamp(transform.position, cam.transform.position, ref velocity, smoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, cam.transform.rotation, smoothTime);
        }
    }
}