using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Get reference to the main camera (tagged "MainCamera")
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // Make the UI face the camera directly
        transform.forward = mainCamera.transform.forward;
    }
}
