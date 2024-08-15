using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHoppingCart : MonoBehaviour
{
    public Monster m;
    [SerializeField] private Camera cam;
    [SerializeField] public Rigidbody rb;
    [SerializeField] private Transform tf;
    public AudioSource cartAudio;
    [SerializeField] private AudioSource carthitAudio;
    private float rbSpeed; 
    private Vector3 tfPos;
    public float lookSpeed;
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float bouncebackSpeed;
    [SerializeField] private float bouncebackForce;
    public float sprintSpeed = 9;
    public float maxSpeed = 9;
    [SerializeField] private GameObject F_light;
    [SerializeField] AudioSource flashSource;
    public bool _flashlightON;
    float rotationX = 0;
    private float lookXLimit = 65.0f;
    private bool isSprinting;
    public float sprintTimer;
    public float sprintTime;
    public float sprintRecovery;
    public AudioSource punch;

    // KEYBINDS
    [SerializeField] private KeyCode _forwardkey = KeyCode.W;
    [SerializeField] private KeyCode _leftkey = KeyCode.A;
    [SerializeField] private KeyCode _backkey = KeyCode.S;
    [SerializeField] private KeyCode _rightkey = KeyCode.D;
    [SerializeField] private KeyCode _flashlightkey = KeyCode.F;
    [SerializeField] private KeyCode _sprintkey = KeyCode.LeftShift;
    public KeyCode _listkey = KeyCode.Tab;

    private void Start()
    {
       if (Sensitivity.sensitivity == 0)
        {
            Sensitivity.sensitivity = 2;
        }
        lookSpeed = Sensitivity.sensitivity;
        sprintTimer = sprintTime;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        maxSpeed = moveSpeed;
        _flashlightON = false;
        flashSource.Play();
        Rigidbody rb = GetComponent<Rigidbody>();

    }

    private void Update()
    {

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        cam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        tfPos = tf.position;

        if (Input.GetKeyUp(_flashlightkey))
        {
            if (_flashlightON)
            {
                F_light.gameObject.SetActive(false);
                flashSource.Play();
                _flashlightON = false;
            }
            else if (!_flashlightON)
            {
                F_light.gameObject.SetActive(true);
                flashSource.Play();
                _flashlightON = true;
            }

        }

}
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("bounce"))
            if (rbSpeed >= bouncebackSpeed)
            { 
                rb.AddRelativeForce(Vector3.back * bouncebackForce, ForceMode.Impulse);
                carthitAudio.Play();
            }
        
    }

    private void LateUpdate()
    {
        cartAudio.volume = rb.velocity.sqrMagnitude / 100;
        rbSpeed = rb.velocity.magnitude;
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        if (m.isAttacked)
        {
            rb.velocity = Vector3.zero;
        }
        if (Input.GetKey(_forwardkey))
        {
            rb.AddRelativeForce(Vector3.forward * Time.deltaTime * moveSpeed * 150f);
            cam.transform.Translate(Vector3.back * 0.002f);
            cam.transform.Translate(Vector3.up * 0.002f);
            if (Input.GetKey(_sprintkey))
            {
                if (sprintTimer > 0)
                {
                    isSprinting = true;
                    maxSpeed = sprintSpeed;
                    rb.AddRelativeForce(Vector3.forward * Time.deltaTime * sprintSpeed * 100f);
                    cam.transform.Translate(Vector3.back * 0.003f);
                }
            }
            if (Input.GetKeyUp(_sprintkey))
            {
                isSprinting = false;
                maxSpeed = moveSpeed;
            }
            if (isSprinting && sprintTimer > 0)
            {
                sprintTimer = sprintTimer - 1 * Time.deltaTime;
            }
            if (sprintTimer <= 0)
            {
                isSprinting = false;
                maxSpeed = moveSpeed;
                StartCoroutine("timerReset");
            }
        }

        if (Input.GetKey(_leftkey))
        {
            rb.AddRelativeForce(Vector3.left * Time.deltaTime * sprintSpeed * 50f);
            cam.transform.Translate(Vector3.right * 0.002f);
        }

        if (Input.GetKey(_rightkey))
        {
            rb.AddRelativeForce(Vector3.right * Time.deltaTime * sprintSpeed * 50f);
            cam.transform.Translate(Vector3.left * 0.002f);
        }

        if (Input.GetKey(_backkey))
        {
            rb.AddRelativeForce(Vector3.back * Time.deltaTime * sprintSpeed * 100f);
            cam.transform.Translate(Vector3.forward * 0.002f);
        }

        if (!Input.GetKey(_backkey) || (!Input.GetKey(_leftkey) || (!Input.GetKey(_rightkey) || (!Input.GetKey(_forwardkey)))))
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, tfPos, Time.deltaTime);
        }
        
    }
    IEnumerator timerReset()
    {
        yield return new WaitForSeconds(sprintRecovery);
        sprintTimer = sprintTime;
        StopCoroutine("timerReset");
 
    }

}
   
