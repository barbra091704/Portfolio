using System;
using System.Collections;
using FirstGearGames.SmoothCameraShaker;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

public class Sniper : NetworkBehaviour
{
    public enum WindDirection
    {
        Left,
        Right,
        Up,
        Down,
        LeftDown,
        LeftUp,
        RightDown,
        RightUp
    }

    [SerializeField] private int windspeed = 0; // m/s
    public int Windspeed
    {
        get { return windspeed; }
        set
        {
            if (windspeed != value)
            {
                windspeed = value;
                OnWindParameterChanged?.Invoke(windspeed, windDirection);
            }
        }
    }

    [SerializeField] private WindDirection windDirection = WindDirection.Right;
    public WindDirection WindDir
    {
        get { return windDirection; }
        set
        {
            if (windDirection != value)
            {
                windDirection = value;
                OnWindParameterChanged?.Invoke(windspeed, windDirection);
            }
        }
    }

    [SerializeField] private int distance = 150; // meters
    public int Distance
    {
        get { return distance; }
        set
        {
            if (distance != value)
            {
                distance = value;
                OnDistanceChanged?.Invoke(distance);
            }
        }
    }

    [SerializeField] float zoom = 0;
    public float Zoom
    {
        get { return zoom; }
        set
        {
            if (zoom != value)
            {
                zoom = value;
                OnZoomChanged?.Invoke(zoom);
            }
        }
    }

    [SerializeField] float convertedZoom = 0;
    public float ConvertedZoom
    {
        get { return convertedZoom; }
        set
        {
            if (convertedZoom != value)
            {
                convertedZoom = value;
                OnConvertedZoomChanged?.Invoke(convertedZoom);
            }
        }
    }

    public float lastZoom = 5;

    [SerializeField] private Vector2 offset;
    public Vector2 Offset
    {
        get { return offset; }
        set
        {
            if (offset != value)
            {
                offset = value;
                OnOffsetChanged?.Invoke(offset);
            }
        }
    }
    [SerializeField] private GameObject bullet;

    public Vector2Int windSpeeds;
    public Vector2Int distances;
    public GameObject redDot;
    private Camera cam;

    private float airDensity = 1.293f; // kg/m^3


    [SerializeField] private float shootDelay = 1;
    [SerializeField] private int clipSize = 6;
    private int civilianKillCount;
    private int currentAmmo;
    public bool isScoped = false;
    public ShakeData shakeData;
    public event Action<float, WindDirection> OnWindParameterChanged;
    public event Action<int> OnDistanceChanged;
    public event Action<float> OnZoomChanged;

    public event Action<float> OnConvertedZoomChanged;

    public event Action<Vector2> OnOffsetChanged;

    private float gravity = 9.8f;
    private float velocity = 0; // m/s
    private float drag = 0;
    private float windDriftX = 0;
    private float windDriftY = 0;
    private float bulletArea = 63.86f / 1000000f; // m^2 (converted from mm^2)
    private float dragCoefficient = 0.261f;
    private Vector2 poi = Vector2.zero;
    private Vector2 por = Vector2.zero;
    private bool isBulletFired = false;
    private float timeOfFlight = 0;
    private BoxCollider2D boxCollider;

    private IEnumerator Start()
    {
        if (!IsOwner) 
        {
            GetComponent<AudioListener>().enabled = false;
            enabled = false;
            yield break;
        }
        SetBulletVisibilityRpc(false);
        redDot.SetActive(false);
        cam = transform.GetComponent<Camera>();
        boxCollider = bullet.GetComponent<BoxCollider2D>();
        currentAmmo = clipSize;

        yield return new WaitForEndOfFrame();

        ClassicManager.Instance.IsEveryPlayerDead.OnValueChanged += AllPlayersDead;

        RandomizeValues();
    }


    void Update()
    {
        if (!IsOwner) return;


        if (Input.GetMouseButtonDown(0) && !isBulletFired && isScoped)
        {
            if (currentAmmo > 0)
            {
                FireShot();
            }
            else
            {
                SoundManager.Instance.PlayOutOfAmmoSound();
            }
        }
        if (Input.GetKeyDown(KeyCode.R) && !isScoped)
        {
            Reload();
        }
        if(Input.GetMouseButton(1)){

            Zoom = Mathf.Lerp(4, 20, (5 - cam.orthographicSize) / (5 - 2));
            ConvertedZoom = 5 - (Zoom - 4) * (5 - 2) / (20 - 4);
            Zoom = Mathf.FloorToInt(Zoom);
            Offset = CalculateReqOffset();
        }
        if(Input.GetMouseButtonUp(1)){

            lastZoom = ConvertedZoom;
        }
    }

    void FireShot()
    {
        SetBulletVisibilityRpc(true);
        CameraShakerHandler.Shake(shakeData);
        por = transform.position;
        velocity = 710;
        timeOfFlight = distance / velocity;
        isBulletFired = true;
        currentAmmo--; // Decrease ammo

        // Adjust pitch based on remaining ammo
        float pitch = 1.0f + (clipSize - currentAmmo) * 0.15f;
        SoundManager.Instance.PlaySniperSoundRpc(pitch);

        StartCoroutine(SimulateBulletTrajectory());
    }

    public void Reload()
    {
        if (currentAmmo < clipSize)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }
    private IEnumerator ReloadCoroutine()
    {
        SoundManager.Instance.PlayReloadSound();
        yield return new WaitForSeconds(2f);
        currentAmmo = clipSize;
    }

    public Vector2 CalculateReqOffset()
    {
        float expectedDrag = 0.5f * airDensity * Mathf.Pow(710, 2) * bulletArea * dragCoefficient;
        float expectedVelocity = 710;
        float expectedTOF = distance / expectedVelocity;


        return new(GetWindVector().x * distance / (cam.orthographicSize / 5 * 1000), expectedTOF * gravity / (expectedDrag + GetWindVector().y) / (expectedDrag + gravity) * (WindDir == WindDirection.RightDown || WindDir == WindDirection.LeftDown ? 1000 / cam.orthographicSize : WindDir == WindDirection.Down ? 500 / cam.orthographicSize : 100 / cam.orthographicSize) * -1);
    }

    IEnumerator SimulateBulletTrajectory()
    {
        bullet.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        float xcoord = 0;
        float ycoord = 0;

        // Calculate wind components based on the enum
        Vector2 wind = GetWindVector();

        for (float t = 0; t <= timeOfFlight; t += 0.02f)
        {
            drag = 0.5f * airDensity * Mathf.Pow(velocity, 2) * bulletArea * dragCoefficient;
            velocity -= drag * 0.02f;

            windDriftX = wind.x * t;
            windDriftY = wind.y * t;
            xcoord += windDriftX * 0.02f;

            ycoord += 0.5f * gravity * Mathf.Pow(t, 2) * 0.02f + windDriftY * 0.5f * 0.02f;

            poi = new Vector2(por.x + xcoord, por.y - ycoord);
            bullet.transform.position = poi;
            bullet.transform.localScale -= new Vector3(t * 0.02f, t * 0.02f, t * 0.02f);

            yield return new WaitForSeconds(0.02f); // simulate the time step
        }

        Collider2D[] overlap = Physics2D.OverlapAreaAll(boxCollider.bounds.min, boxCollider.bounds.max);

        if (overlap.Length > 0)
        {
            foreach (var item in overlap)
            {
                if (item.CompareTag("Player"))
                {
                    print("Hit Player");
                    if (item.transform.root.TryGetComponent(out IDamagable component) && item.GetComponent<PlayerMovement>().Health.Value > 0)
                    {
                        component.DamageRpc(100);
                        SoundManager.Instance.PlayHitSoundRpc();
                    }
                }
                else if (item.transform.root.CompareTag("Civilian"))
                {
                    print("Hit Civilian!");
                    if (item.TryGetComponent(out IDamagable component) && item.TryGetComponent(out CivilianAI civilianAI))
                    {
                        if (civilianAI.IsAlive.Value)
                        {
                            component.DamageRpc(100);
                            SoundManager.Instance.PlayHitSoundRpc();
                            civilianKillCount++;

                            if (civilianKillCount >= 3)
                            {
                                ClassicManager.Instance.EndGameRpc(PlayerType.Sniper, SteamClient.Name, false);
                            }
                        }
                    }
                }
            }
        }

        SetBulletVisibilityRpc(false);

        // Ensure the bullet can be fired again regardless of what was hit.
        yield return new WaitForSeconds(shootDelay);
        isBulletFired = false;
    }

    private void AllPlayersDead(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            Tasks.Instance.CompleteTask("Kill all Players");
        }
    }

    Vector2 GetWindVector()
    {
        return windDirection switch
        {
            WindDirection.Left => new Vector2(-windspeed, 0),
            WindDirection.Right => new Vector2(windspeed, 0),
            WindDirection.Up => new Vector2(0, -windspeed),
            WindDirection.Down => new Vector2(0, windspeed),
            WindDirection.LeftDown => new Vector2(-windspeed, windspeed),
            WindDirection.LeftUp => new Vector2(-windspeed, -windspeed),
            WindDirection.RightDown => new Vector2(windspeed, windspeed),
            WindDirection.RightUp => new Vector2(windspeed, -windspeed),
            _ => Vector2.zero,
        };
    }

    private WindDirection GetRandomWindDirection()
    {
        Array values = Enum.GetValues(typeof(WindDirection));
        return (WindDirection)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }

    private void RandomizeValues()
    {
        Distance = UnityEngine.Random.Range(distances.x, distances.y);
        Windspeed = UnityEngine.Random.Range(windSpeeds.x, windSpeeds.y);
        
        StartCoroutine(SlowlyChangeWindSpeed());
        StartCoroutine(SlowlyChangeWindDirection());
    }
    private IEnumerator SlowlyChangeWindSpeed()
    {
        while (true)
        {
            // Gradually adjust wind speed by a small amount every 2 seconds
            Windspeed += UnityEngine.Random.Range(-1, 1);
            Windspeed = Mathf.Clamp(Windspeed, windSpeeds.x, windSpeeds.y);
            
            yield return new WaitForSeconds(3f);
        }
    }

    private IEnumerator SlowlyChangeWindDirection()
    {
        while (true)
        {
            // Change wind direction every 60 seconds
            WindDir = GetRandomWindDirection();
            
            yield return new WaitForSeconds(UnityEngine.Random.Range(30, 60));
        }
    }


    [Rpc(SendTo.Everyone)]
    public void SetBulletVisibilityRpc(bool value)
    {
        bullet.SetActive(value);
    }
}
