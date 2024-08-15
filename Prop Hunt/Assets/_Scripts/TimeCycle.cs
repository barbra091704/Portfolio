using UnityEngine;
using Photon.Pun;

public class TimeCycle : MonoBehaviourPun
{
    private float rotation = 0f;
    public float exposure;
    Vector3 rot = Vector3.zero;
    public float speed = 5;
    public GameObject lights;
    public float density;
    public AudioSource aud;
    public AudioClip day;
    public AudioClip night;
    public bool music;

    private void Start()
    {
        music = true;
        aud.PlayOneShot(day);
        RenderSettings.skybox.SetFloat("_Exposure", 0.1f);
        RenderSettings.skybox.SetFloat("_Rotation", 0);
        RenderSettings.fogDensity = 0.002f;
    }

    public void Update()
    {

        rot.x = speed * Time.deltaTime;
        transform.Rotate(rot, Space.World);
        rotation = (rotation + 0.003f) % 360f;
        RenderSettings.skybox.SetFloat("_Rotation", rotation);
        if (transform.rotation.eulerAngles.x >= 0 && transform.rotation.eulerAngles.x <= 90)
        {
            speed = 0.7f;
            gameObject.GetComponent<Light>().intensity = Mathf.Lerp(0f, 1f, (transform.rotation.eulerAngles.x) / 90);
            exposure = Mathf.Lerp(0.15f, 1.5f, (transform.rotation.eulerAngles.x) / 90);
            density = Mathf.Lerp(0.06f, 0.002f, (transform.rotation.eulerAngles.x) / 90 * 2f);
            lights.SetActive(false);
        }
        else if (transform.rotation.eulerAngles.x >= -90 && transform.rotation.eulerAngles.x < 0)
        {
            speed = 0.7f;
            gameObject.GetComponent<Light>().intensity = Mathf.Lerp(1f, 0, (-transform.rotation.eulerAngles.x) / 90 * 2f);
            exposure = Mathf.Lerp(1.5f, 0.15f, (-transform.rotation.eulerAngles.x) / 90);
            density = Mathf.Lerp(0.002f, 0.06f, (-transform.rotation.eulerAngles.x) / 90 * 2f);
            lights.SetActive(true);
        }
        else
        {
            music = true;
            lights.SetActive(true);
            speed = 1.5f;
            exposure = 0.15f;
            density = 0.06f;
        }
        if (transform.rotation.eulerAngles.x >= 0 && music)
        {
            aud.Stop();
            aud.PlayOneShot(day);
            music = false;
        }
        if (transform.rotation.eulerAngles.x < 0 && music)
        {
            aud.Stop();
            aud.PlayOneShot(night);
            music = false;
        }
        RenderSettings.skybox.SetFloat("_Exposure", exposure);
        RenderSettings.fogDensity = density;
        photonView.RPC("SyncSkybox", RpcTarget.All, rotation, exposure, density);
    }

    [PunRPC]
    public void SyncSkybox(float rotation, float exposure, float density)
    {
        RenderSettings.fogDensity = density;
        RenderSettings.skybox.SetFloat("_Rotation", rotation);
        RenderSettings.skybox.SetFloat("_Exposure", exposure);
    }
}