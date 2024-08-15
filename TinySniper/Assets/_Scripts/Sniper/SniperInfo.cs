using TMPro;
using UnityEngine;

public class SniperInfo : MonoBehaviour
{
    private Sniper sniper;

    public Transform windDirection;
    public TMP_Text windSpeedText;
    public TMP_Text distanceText;
    public TMP_Text zoomText;
    public TMP_Text requiredXOffsetText;
    public TMP_Text requiredYOffsetText;

    public void Start()
    {
        sniper = GetComponent<Sniper>();
        sniper.OnWindParameterChanged += UpdateWindText;
        sniper.OnDistanceChanged += UpdateDistanceText;
        sniper.OnZoomChanged += UpdateZoomText;
        sniper.OnOffsetChanged += UpdateRequiredOffsetText;
    }

    void OnDisable()
    {
        sniper.OnWindParameterChanged -= UpdateWindText;
        sniper.OnDistanceChanged -= UpdateDistanceText;
        sniper.OnZoomChanged -= UpdateZoomText;
        sniper.OnOffsetChanged -= UpdateRequiredOffsetText;
    }

    public void UpdateDistanceText(int distance)
    {
        distanceText.text = $"{distance}";
    }

    public void UpdateZoomText(float zoom)
    {
        zoomText.text = $"{zoom}x";
    }

    public void UpdateRequiredOffsetText(Vector2 offset)
    {
        requiredXOffsetText.text = $"{offset.x:F1}";
        requiredYOffsetText.text = $"{offset.y:F1}";
    }


    public void UpdateWindText(float windSpeed, Sniper.WindDirection direction)
    {
        // Set the wind speed text
        windSpeedText.text = $"{windSpeed}";

        // Adjust the wind direction arrow to point in the correct direction
        windDirection.rotation = direction switch
        {
            Sniper.WindDirection.Left => Quaternion.Euler(0, 0, 0),// Point left
            Sniper.WindDirection.Right => Quaternion.Euler(0, 0, 180),// Point right
            Sniper.WindDirection.Up => Quaternion.Euler(0, 0, -90),// Point up
            Sniper.WindDirection.Down => Quaternion.Euler(0, 0, 90),// Point down
            Sniper.WindDirection.LeftDown => Quaternion.Euler(0, 0, 45),// Point left-down
            Sniper.WindDirection.LeftUp => Quaternion.Euler(0, 0, -45),// Point left-up
            Sniper.WindDirection.RightDown => Quaternion.Euler(0, 0, 135),// Point right-down
            Sniper.WindDirection.RightUp => Quaternion.Euler(0, 0, -135),// Point right-up
            _ => Quaternion.identity,// Default to no rotation
        };
    }
}
