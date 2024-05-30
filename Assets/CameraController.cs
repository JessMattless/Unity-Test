using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject playerObject;
    public Vector3 cameraOffset = new(0f, 0.75f, 0f);

    private Transform playerTransform;

    private float lookSpeed;
    private float pitch = 0.0f;
    private float yaw = 0.0f;

    void Start()
    {
        // Get the player's transform component to be used later
        playerTransform = playerObject.GetComponent<Transform>();

        /* Get the player's look speed from the player object, 
        * here to make it easier for changing variables just from the player script
        */
        lookSpeed = playerObject.GetComponent<PlayerController>().lookSpeed;

        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        // Get the player's mouse input for camera rotation
        pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
        yaw += Input.GetAxis("Mouse X") * lookSpeed;

        // Clamp the pitch of the camera, to disable the player rotating the camera upside down
        pitch = Mathf.Clamp(pitch, -90, 90);

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        // Move the camera to the position of the player, offset by the cameraOffset vector according to direction
        transform.transform.position = playerTransform.position
            + (playerTransform.forward * cameraOffset.x)
            + (playerTransform.right * cameraOffset.z)
            + new Vector3(0, cameraOffset.y, 0);
    }
}
