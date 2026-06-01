using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;

    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
    public void SetHealth(float current, float max)
    {
        float value = Mathf.Clamp01(current / max);
        fillImage.fillAmount = value;
    }
}