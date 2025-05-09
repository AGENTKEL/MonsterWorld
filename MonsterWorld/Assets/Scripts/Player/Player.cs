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
    
    [Header("Interaction Sphere")]
    public InteractionSphere interactionSphere;
    
    [Header("Level System")]
    public LevelSystem levelSystem;
    
    [Header("Monsters")]
    public List<GameObject> monsters; // Drag your monster GameObjects here
    private int currentMonsterIndex = 0;

    private Vector3 velocity;
    private bool isGrounded;

    // 🔁 Interaction cooldown tracker
    private float interactionCooldownTimer = 0f;
    private bool isAttacking = false;
    public float attackDelay;

    void Update()
    {
        HandleMovement();
        HandleAttack();
        ApplyGravity();

        // Update cooldown timer
        if (interactionCooldownTimer > 0f)
            interactionCooldownTimer -= Time.deltaTime;
    }

    void HandleMovement()
    {
        isGrounded = characterController.isGrounded;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0f, v).normalized;

        bool isWalking = input.magnitude >= 0.1f;
        animator.SetBool("Walk", isWalking);

        if (isWalking)
        {
            Vector3 moveDir = cameraRig.forward * input.z + cameraRig.right * input.x;
            moveDir.y = 0f;
            moveDir.Normalize();

            characterController.Move(moveDir * moveSpeed * Time.deltaTime);

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && interactionCooldownTimer <= 0f && !isAttacking)
        {
            animator.SetTrigger("Attack");
            StartCoroutine(DelayedInteraction());
        }

    }
    
    public void OnLevelUp(int newLevel)
    {
        if (monsters == null || monsters.Count == 0) return;

        // Disable current monster
        if (currentMonsterIndex < monsters.Count)
            monsters[currentMonsterIndex].SetActive(false);

        // Clamp new index to available monsters
        currentMonsterIndex = Mathf.Min(newLevel - 1, monsters.Count - 1);

        // Enable new monster
        monsters[currentMonsterIndex].SetActive(true);

        // Replace animator with the one from new monster
        Animator newAnimator = monsters[currentMonsterIndex].GetComponentInChildren<Animator>();
        if (newAnimator != null)
        {
            animator = newAnimator;
        }
    }
    
    IEnumerator DelayedInteraction()
    {
        isAttacking = true;
        
        yield return new WaitForSeconds(attackDelay);

        if (interactionSphere != null)
        {
            interactionSphere.StartInteractionSphere();
            interactionCooldownTimer = interactionSphere.interactionCooldown;
        }

        isAttacking = false;
    }
}
