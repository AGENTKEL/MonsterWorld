 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public CharacterController characterController;
    public Transform cameraRig;

    [Header("Animation")]
    public Animator animator;

    private Vector3 velocity;
    private bool isGrounded;

    void Update()
    {
        HandleMovement();
        HandleAttack();
        ApplyGravity();
    }

    void HandleMovement()
    {
        isGrounded = characterController.isGrounded;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0f, v).normalized;

        // Set Walk animation
        bool isWalking = input.magnitude >= 0.1f;
        animator.SetBool("Walk", isWalking);

        if (isWalking)
        {
            // Move relative to camera
            Vector3 moveDir = cameraRig.forward * input.z + cameraRig.right * input.x;
            moveDir.y = 0f;
            moveDir.Normalize();

            characterController.Move(moveDir * moveSpeed * Time.deltaTime);

            // Rotate player toward movement direction
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        // Optional: jump input
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // small downward force to stick to ground
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Attack");
        }
    }
}
