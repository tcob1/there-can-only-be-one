using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        // Get main camera transform
        if (Camera.main != null)
            camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (camTransform != null)
        {
            // Rotate the canvas to face the camera
            transform.LookAt(transform.position + camTransform.rotation * Vector3.forward,
                             camTransform.rotation * Vector3.up);
        }
    }
}
