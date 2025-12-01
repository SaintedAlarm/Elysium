using UnityEngine;
using UnityEngine.UI;

public class BaseHealthBarUI : MonoBehaviour
{
    public Slider slider;
    public Image fillImage;   // <-- ADD THIS

    // Called by Health.onHealthChanged01
    public void SetHealth01(float value01)
    {
        if (slider != null)
        {
            slider.value = value01;
        }

        // Optional: change color based on value
        if (fillImage != null)
        {
            fillImage.color = GetColorForValue(value01);
        }
    }

    // 1 = green, 0.5 = yellow, 0 = red
    private Color GetColorForValue(float v)
    {
        v = Mathf.Clamp01(v);

        if (v >= 0.5f)
        {
            // 0.5 → 1 maps to 0 → 1
            float t = (v - 0.5f) / 0.5f;
            return Color.Lerp(Color.yellow, Color.green, t);
        }
        else
        {
            // 0 → 0.5 maps to 0 → 1
            float t = v / 0.5f;
            return Color.Lerp(Color.red, Color.yellow, t);
        }
    }
}
