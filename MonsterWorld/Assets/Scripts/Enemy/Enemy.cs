using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public Animator animator;
    public EnemySphere enemySphere;

    [Header("Detection")]
    public float attackRange = 5f;
    public float attackCooldown = 1.5f;
    private float attackTimer;

    [Header("Damage & Health")]
    public int maxHP = 100;
    public int currentHP;
    public int damage = 10;
    public float respawnDelay = 3f;

    [Header("UI")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("Model")]
    public GameObject botModel;
    public GameObject botUI;

    [Header("Player Reference")]
    public Transform player; // Assign this at runtime or in inspector
    public int xpReward = 20;
    private LevelSystem levelSystem;

    private bool isAttacking = false;
    private bool isDead = false;

    void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();
        player = FindFirstObjectByType<Player>().transform;
        levelSystem = player.GetComponent<LevelSystem>();
    }

    void Update()
    {
        if (isDead || player == null) return;

        attackTimer -= Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && attackTimer <= 0f && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
        
        if (distanceToPlayer <= attackRange)
            RotateTowardsPlayer();
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.3f); // Sync with animation if needed

        if (enemySphere != null)
        {
            enemySphere.StartInteractionSphere(damage);
        }

        attackTimer = attackCooldown;
        isAttacking = false;
    }
    
    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        direction.y = 0f; // Ignore vertical difference

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHPUI();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");
        animator.SetBool("Died", true);
        levelSystem.AddXP(xpReward);

        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(1f); // Let death animation play
        botModel.SetActive(false);
        botUI.SetActive(false);

        yield return new WaitForSeconds(respawnDelay);
        animator.SetBool("Died", false);

        // Respawn logic
        currentHP = maxHP;
        UpdateHPUI();
        botModel.SetActive(true);
        botUI.SetActive(true);
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
