using UnityEngine;

public class CameraAutoActivate : MonoBehaviour
{
    public GameObject cameraObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!cameraObject) return;

        // if the camera is disabled, enable it
        if (!cameraObject.GetComponent<Camera>().enabled)
        {
            cameraObject.GetComponent<Camera>().enabled = true;
        }

        // if the audio listener is disabled, enable it
        if (!cameraObject.GetComponent<AudioListener>().enabled)
        {
            cameraObject.GetComponent<AudioListener>().enabled = true;
        }
    }
}
