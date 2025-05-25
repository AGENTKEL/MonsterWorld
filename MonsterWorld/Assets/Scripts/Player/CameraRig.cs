using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
    public Transform target; // Usually the Player
    public Vector3 offset = new Vector3(0f, 3f, -6f);
    public float mouseSensitivity = 3f;
    public float rotationSmoothTime = 0.1f;
    public float pitchMin = -30f;
    public float pitchMax = 60f;

    private float yaw;
    private float pitch;
    private Vector3 currentRotation;
    private Vector3 rotationSmoothVelocity;

    public Player player;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (player.cursorVisible) return;
        
        HandleRotation();
        FollowTarget();
    }

    void HandleRotation()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        transform.eulerAngles = currentRotation;
    }

    void FollowTarget()
    {
        if (target == null) return;
        transform.position = target.position + transform.rotation * offset;
    }
}
