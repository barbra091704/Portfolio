using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MoveAroundObjects : MonoBehaviourPun
{
    [SerializeField]
    private float _mouseSensitivity = 3.0f;

    private float _rotationY;
    private float _rotationX;
    public float scrollSpeed = 5;
    public Vector3 nextRotation;


    public GameObject _target;

    public static float _distanceFromTarget = 0f;

    private Vector3 _currentRotation;
    private Vector3 _smoothVelocity = Vector3.zero;

    [SerializeField]
    private float _smoothTime = 0.00f;

    [SerializeField]
    private Vector2 _rotationXMinMax = new Vector2(30, 85);

    private void Start()
    {
        
        _target = GameObject.FindGameObjectWithTag("Target");
    }
    void LateUpdate()
    {
            float mouseX = Input.GetAxisRaw("Mouse X") * _mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * _mouseSensitivity;
            _rotationY += mouseX;
            _rotationX -= mouseY;

            // Apply clamping for x rotation 
            _rotationX = Mathf.Clamp(_rotationX, _rotationXMinMax.x, _rotationXMinMax.y);

            nextRotation = new Vector3(_rotationX, _rotationY);

            // Apply damping between rotation changes
            _currentRotation = Vector3.SmoothDamp(_currentRotation, nextRotation, ref _smoothVelocity, _smoothTime);
            transform.localEulerAngles = _currentRotation;

            // Substract forward vector of the GameObject to point its forward vector to the target
            transform.position = _target.transform.position - transform.forward * _distanceFromTarget;
            if (EnableUI.isHunter)
            {
                _distanceFromTarget = Mathf.Clamp(_distanceFromTarget, 0, 0);
            }
            else
            {
                _distanceFromTarget -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
                _distanceFromTarget = Mathf.Clamp(_distanceFromTarget, 0.5f, 10);
            }
        

    }

}