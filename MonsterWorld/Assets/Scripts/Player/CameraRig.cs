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
    
    private int touchFingerId = -1;
    private Vector2 lastTouchPosition;
    private float touchSensitivity = 0.2f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        FollowTarget();

        HandleRotation();
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Handle right-side screen touch for mobile
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began && touch.position.x > Screen.width / 2f)
            {
                touchFingerId = touch.fingerId;
                lastTouchPosition = touch.position;
            }
            else if (touch.fingerId == touchFingerId && touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition * touchSensitivity;
                mouseX += delta.x;
                mouseY += delta.y;
            }
            else if (touch.fingerId == touchFingerId && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
            {
                touchFingerId = -1;
            }
        }

        yaw += mouseX * mouseSensitivity;
        pitch -= mouseY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        transform.eulerAngles = currentRotation;
    }

    void FollowTarget()
    {
        if (target == null) return;

        Vector3 desiredCameraPos = target.position + transform.rotation * offset;
        Vector3 direction = desiredCameraPos - target.position;
        float distance = offset.magnitude;

        RaycastHit hit;
        if (Physics.SphereCast(target.position, 0.3f, direction.normalized, out hit, distance))
        {
            // Adjust camera position to hit point minus a small offset
            transform.position = hit.point - direction.normalized * 0.2f;
        }
        else
        {
            transform.position = desiredCameraPos;
        }
    }
}
