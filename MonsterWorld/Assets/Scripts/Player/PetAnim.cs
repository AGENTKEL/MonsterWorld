using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetAnim : MonoBehaviour
{
    public float amplitude = 0.5f;  // Height of up-down motion
    public float speed = 2f;        // Speed of floating

    private Vector3 startLocalPosition;

    void Start()
    {
        startLocalPosition = transform.localPosition;
    }

    void Update()
    {
        float newY = startLocalPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.localPosition = new Vector3(startLocalPosition.x, newY, startLocalPosition.z);
    }
}
