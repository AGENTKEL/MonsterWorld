using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Food : MonoBehaviour
{
    [Header("Health Settings")]
        public int maxHP = 100;
        public int xpReward = 25;
        private int currentHP;
        private bool isDead = false;
    
        [Header("UI References")]
        public Slider hpSlider;
        public TextMeshProUGUI hpText;
    
        [Header("Visuals")]
        public MeshRenderer foodModel; // Drag your visual model here
        public GameObject _ui;
    
        private void Start()
        {
            currentHP = maxHP;
            UpdateUI();
        }
    
        public void TakeDamage(int amount)
        {
            if (isDead) return;
    
            currentHP -= amount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            UpdateUI();
    
            if (currentHP <= 0)
            {
                Die();
            }
        }
    
        private void Die()
        {
            isDead = true;
            foodModel.enabled = false;
            _ui.SetActive(false);

            Player player = FindObjectOfType<Player>();
            if (player != null && player.levelSystem != null)
            {
                player.levelSystem.AddXP(xpReward);
            }

            StartCoroutine(Respawn());
        }
    
        private IEnumerator Respawn()
        {
            yield return new WaitForSeconds(10f);
            currentHP = maxHP;
            isDead = false;
            foodModel.enabled = true;
            _ui.SetActive(true);
            UpdateUI();
        }
    
        private void UpdateUI()
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
