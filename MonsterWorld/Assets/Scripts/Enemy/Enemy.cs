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
    private float timeSinceLastDamage = 0f;
    private float regenDelay = 5f;
    private float regenRate = 0.05f; // 5% per second
    private bool isRegenerating = false;

    [Header("UI")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("Model")]
    public GameObject botModel;
    public GameObject botUI;

    [Header("Player Reference")]
    public Transform player;
    public int xpReward = 20;
    public int moneyReward = 15;
    private LevelSystem levelSystem;

    private bool isAttacking = false;
    private bool isDead = false;

    public float attackDelay = 0.3f;

    private Canvas _canvas;

    void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();
        player = FindFirstObjectByType<Player>().transform;
        levelSystem = player.GetComponent<LevelSystem>();
        _canvas = GetComponentInChildren<Canvas>();
        _canvas.enabled = false;
    }

    void Update()
    {
        if (isDead || player == null) return;

        attackTimer -= Time.deltaTime;
        timeSinceLastDamage += Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && attackTimer <= 0f && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }

        if (distanceToPlayer <= attackRange)
            RotateTowardsPlayer();

        if (timeSinceLastDamage >= regenDelay && !isRegenerating && currentHP < maxHP)
        {
            StartCoroutine(RegenerateHP());
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        animator.SetTrigger("Attack");
        _canvas.enabled = true;
        yield return new WaitForSeconds(attackDelay); // Sync with animation if needed

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

        timeSinceLastDamage = 0f; // Reset regen timer

        if (currentHP <= 0)
        {
            Die();
        }
    }
    
    IEnumerator RegenerateHP()
    {
        isRegenerating = true;

        while (currentHP < maxHP && timeSinceLastDamage >= regenDelay && !isDead)
        {
            int regenAmount = Mathf.CeilToInt(maxHP * regenRate);
            currentHP += regenAmount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            UpdateHPUI();

            yield return new WaitForSeconds(1f);

            // Stop if took damage during regen
            if (timeSinceLastDamage < regenDelay)
                break;
        }

        isRegenerating = false;
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");
        animator.SetBool("Died", true);
        levelSystem.AddXP(xpReward);
        levelSystem.AddMoney(moneyReward);

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
