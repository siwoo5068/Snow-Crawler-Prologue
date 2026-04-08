using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight")]
    public Light flashlight;

    [Header("Settings")]
    public bool startsOn = true;
    public KeyCode toggleKey = KeyCode.F;

    void Start()
    {
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
        }

        if (flashlight != null)
            flashlight.enabled = startsOn;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (flashlight != null)
            {
                flashlight.enabled = !flashlight.enabled;
                Debug.Log("Flashlight toggled. Now enabled: " + flashlight.enabled);
            }
            else
            {
                Debug.LogWarning("Flashlight toggled but flashlight reference is missing! Trying to find it again.");
                flashlight = GetComponentInChildren<Light>();
                if (flashlight != null) flashlight.enabled = !flashlight.enabled;
            }
        }
    }
}
