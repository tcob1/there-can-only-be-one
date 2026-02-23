using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class CameraMovement : MonoBehaviour
{
    public float cameraSpeed = 50f;
    public float cameraRotateSpeed = 1f;
    public float cameraZoomSpeed = 100f;
    public Vector3 startPos = new Vector3(0f, 50f, 0f);
    public Vector3 startAng = new Vector3(35f, 90f, 0f);

    private Vector3 cameraMovement;
    private float currScrollSpeed;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = startPos;
        transform.eulerAngles = startAng;
    }

    // Update is called once per frame
    void Update()
    {
        cameraMovement = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
        {
            cameraMovement += transform.forward;

        }
        if (Keyboard.current.aKey.isPressed)
        {
            cameraMovement -= transform.right;

        }
        if (Keyboard.current.sKey.isPressed)
        {
            cameraMovement -= transform.forward;

        }
        if (Keyboard.current.dKey.isPressed)
        {
            cameraMovement += transform.right;

        }
        if (Keyboard.current.qKey.isPressed)
        {
            transform.Rotate(0, -cameraRotateSpeed * Time.deltaTime, 0, Space.World);

        }
        if (Keyboard.current.eKey.isPressed)
        {
            transform.Rotate(0, cameraRotateSpeed * Time.deltaTime, 0, Space.World);

        }

        cameraMovement[1] = 0;
        cameraMovement = cameraMovement.normalized * cameraSpeed * Time.deltaTime;

        //transform.position += cameraMovement;


        currScrollSpeed = 0;//Mouse.current.scroll.value.y;
        transform.position += transform.forward * cameraZoomSpeed * currScrollSpeed * Time.deltaTime;


    }
}
