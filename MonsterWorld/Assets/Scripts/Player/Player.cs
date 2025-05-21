 using System;
 using System.Collections;
using System.Collections.Generic;
 using TMPro;
 using UnityEngine;
 using UnityEngine.UI;

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

    public int damage = 10;
    
    [Header("Level System")]
    public LevelSystem levelSystem;

    public MonsterStats monsterStats;
    
    [Header("Monsters")]
    public List<GameObject> monsters; // Drag your monster GameObjects here
    private int currentMonsterIndex = 0;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isDead = false;

    // Interaction cooldown tracker
    private float interactionCooldownTimer = 0f;
    private bool isAttacking = false;
    public float attackDelay;
    
    [Header("Health")]
    public int maxHP = 100;
    [SerializeField] private int currentHP;
    public float respawnTime = 3f;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;
    public Transform respawnPoint; // Assign in Inspector

    private void Start()
    {
        currentHP = maxHP;
        damage = monsterStats.baseDamage;
        UpdateHPUI();
    }

    void Update()
    {
        if (isDead) return;
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

        // Clamp new index
        currentMonsterIndex = Mathf.Min(newLevel - 1, monsters.Count - 1);

        // Enable new monster
        var newMonster = monsters[currentMonsterIndex];
        newMonster.SetActive(true);

        // Replace animator
        Animator newAnimator = newMonster.GetComponentInChildren<Animator>();
        if (newAnimator != null) animator = newAnimator;
        
        if (monsterStats != null)
        {
            monsterStats.ScaleStats(newLevel);
            // Optionally apply new stats to the player as well
            maxHP = monsterStats.currentHP;
            currentHP = maxHP;
            damage = monsterStats.currentDamage;
            UpdateHPUI();
        }
    }
    
    IEnumerator DelayedInteraction()
    {
        isAttacking = true;
        
        yield return new WaitForSeconds(attackDelay);

        if (interactionSphere != null)
        {
            interactionSphere.StartInteractionSphere(damage);
            interactionCooldownTimer = interactionSphere.interactionCooldown;
        }

        isAttacking = false;
    }
    
    public void TakeDamage(int amount)
    {
        if (currentHP <= 0) return;
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHPUI();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHPUI();
    }

    void Die()
    {
        animator.SetTrigger("Death");
        animator.SetBool("Died", true);
        characterController.enabled = false;
        isDead = true;
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        animator.SetBool("Died", false);

        // Reset HP
        currentHP = maxHP;
        UpdateHPUI();

        // Move to respawn point
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;
        
        characterController.enabled = true;
        isDead = false;
    }
    
    void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";
        }
    }
}
