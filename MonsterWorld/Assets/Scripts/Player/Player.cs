using System.Collections;
using System.Collections.Generic;
 using TMPro;
 using UnityEngine;
 using UnityEngine.UI;
using YG;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public CharacterController characterController;
    public Timer timer;
    public PetManager _petManager;
    public Transform cameraRig;
    public Joystick joystick;

    [Header("Animation")]
    public Animator animator;
    
    [Header("Interaction Sphere")]
    public InteractionSphere interactionSphere;

    public int damage = 10;
    
    [Header("Level System")]
    public LevelSystem levelSystem;
    public ParticleSystem levelUpParticle;

    public MonsterStats monsterStats;
    
    [Header("Monsters")]
    public List<GameObject> monsters; // Drag your monster GameObjects here
    private int currentMonsterIndex = 0;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isDead = false;
    public bool cursorVisible = false;

    // Interaction cooldown tracker
    private float interactionCooldownTimer = 0f;
    private bool isAttacking = false;
    public float attackDelay;
    
    [Header("Health")]
    public int maxHP = 100;
    public int currentHP;
    public float respawnTime = 3f;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;
    public Transform respawnPoint;
    public Transform respawnPoint2;
    public Transform respawnPoint3;
    private float timeSinceLastDamage = 0f;
    private bool isRegenerating = false;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] attackSounds;
    public AudioClip deathSound;

    private void Start()
    {
        currentHP = maxHP;
        damage = monsterStats.baseDamage;
        UpdateHPUI();
        CursorToggle();
    }

    void Update()
    {
        if (isDead) return;

        ApplyGravity();
        HandleCursorToggle();

        if (interactionCooldownTimer > 0f)
            interactionCooldownTimer -= Time.deltaTime;
        

        HandleMovement();
        HandleAttack();

        // Track time since last damage
        timeSinceLastDamage += Time.deltaTime;

        // Start regen if conditions are met
        if (timeSinceLastDamage >= 5f && !isRegenerating && currentHP < maxHP)
        {
            StartCoroutine(RegenerateHP());
        }
    }

    void HandleMovement()
    {

        isGrounded = characterController.isGrounded;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Include joystick input
        if (Mathf.Approximately(h, 0) && Mathf.Approximately(v, 0))
        {
            h = joystick.Horizontal;
            v = joystick.Vertical;
        }

        Vector3 input = new Vector3(h, 0f, v).normalized;
        bool isWalking = input.magnitude >= 0.1f;
        animator.SetBool("Walk", isWalking);

        Vector3 moveDir = Vector3.zero;

        if (isWalking)
        {
            moveDir = cameraRig.forward * input.z + cameraRig.right * input.x;
            moveDir.y = 0f;
            moveDir.Normalize();

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        characterController.Move(moveDir * moveSpeed * Time.deltaTime);

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    
    void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            cursorVisible = !cursorVisible;
            Cursor.visible = cursorVisible;
            Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
    
    public void CursorToggle()
    {
        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
    
    public void SetCursorToggle()
    {
        cursorVisible = !cursorVisible;
        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
        animator.SetBool("Walk", false);
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
        if (YG2.envir.isDesktop)
        {
            // Hold to attack
            if (Input.GetMouseButton(0) && interactionCooldownTimer <= 0f && !isAttacking)
            {
                animator.SetTrigger("Attack");
                StartCoroutine(DelayedInteraction());
            }
        }
    }
    
    public void Attack()
    {
        if (interactionCooldownTimer <= 0f && !isAttacking)
        {
            animator.SetTrigger("Attack");
            StartCoroutine(DelayedInteraction());
        }
    }
    
    IEnumerator DelayedInteraction()
    {
        isAttacking = true;
        

        // Wait for animation delay before executing the attack logic
        yield return new WaitForSeconds(attackDelay);

        if (interactionSphere != null)
        {
            interactionSphere.StartInteractionSphere(damage);
            interactionCooldownTimer = interactionSphere.interactionCooldown;
        }
        
        // Play random attack sound
        if (attackSounds != null && attackSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
            audioSource.PlayOneShot(clip);
        }

        // Wait for the cooldown before allowing another attack
        yield return new WaitForSeconds(interactionCooldownTimer);

        isAttacking = false;
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
        timer.SetTimeLeaderBoard();
        levelUpParticle.Play();

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
            _petManager.UpdatePetBuff();
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHP <= 0) return;

        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHPUI();

        timeSinceLastDamage = 0f;

        if (isRegenerating)
        {
            StopCoroutine(RegenerateHP());
            isRegenerating = false;
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }
    
    IEnumerator RegenerateHP()
    {
        isRegenerating = true;

        while (currentHP < maxHP)
        {
            Heal(Mathf.RoundToInt(maxHP * 0.1f));
            yield return new WaitForSeconds(1f);

            if (timeSinceLastDamage < 5f) // If damaged during regen, stop
            {
                break;
            }
        }

        isRegenerating = false;
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
        
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        animator.SetBool("Died", false);

        // Reset HP
        currentHP = maxHP;
        UpdateHPUI();

        // Move to respawn point
        if (levelSystem.currentLevel < 15)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation; 
        }
        
        else if (levelSystem.currentLevel >= 15 && levelSystem.currentLevel < 25)
        {
            transform.position = respawnPoint2.position;
            transform.rotation = respawnPoint2.rotation; 
        }
        
        else if (levelSystem.currentLevel >= 25)
        {
            transform.position = respawnPoint3.position;
            transform.rotation = respawnPoint3.rotation; 
        }

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
    
    public void BuyPet(int price)
    {
        if (levelSystem.money >= price)
        {
            levelSystem.SubtractMoney(price);

            _petManager.UnlockRandomPetByProbability();
        }
    }
}
