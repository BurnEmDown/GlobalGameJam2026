using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Controls the in-game HUD displaying points and speed
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("HUD Text Elements")]
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI speedText;
    
    [Header("HUD Visual Elements")]
    [SerializeField] private Slider speedMeter;
    
    [Header("Display Settings")]
    [SerializeField] private bool showSpeed = true;
    
    [Header("Formatting")]
    [SerializeField] private string pointsFormat = "Points: {0}";
    [SerializeField] private string speedFormat = "Speed: {0:F0} km/h";

    private void Start()
    {
        // Initialize HUD elements visibility
        if (speedText != null) speedText.gameObject.SetActive(showSpeed);
        
        // Initialize points display
        UpdatePoints(0);
    }

    /// <summary>
    /// Update points display - called by GameManager
    /// </summary>
    public void UpdatePoints(int points)
    {
        if (pointsText != null)
        {
            pointsText.text = string.Format(pointsFormat, points);
        }
    }

    /// <summary>
    /// Update speed display
    /// </summary>
    public void UpdateSpeed(float currentSpeed, float maxSpeed = 30f)
    {
        if (showSpeed && speedText != null)
        {
            speedText.text = string.Format(speedFormat, currentSpeed);
        }

        // Update speed meter if using a slider
        if (speedMeter != null)
        {
            speedMeter.value = currentSpeed / maxSpeed;
        }
    }

    /// <summary>
    /// Show or hide the entire HUD
    /// </summary>
    public void SetHUDVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// Flash points text (useful for score feedback)
    /// </summary>
    public void FlashPoints()
    {
        if (pointsText != null)
        {
            StartCoroutine(FlashTextRoutine(pointsText));
        }
    }

    private System.Collections.IEnumerator FlashTextRoutine(TextMeshProUGUI text)
    {
        Color original = text.color;
        text.color = Color.yellow;
        yield return new WaitForSeconds(0.15f);
        text.color = original;
    }
}