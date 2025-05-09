using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSystem : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public float xpIncreaseFactor = 1.25f; // Increase XP needed each level

    [Header("UI References")]
    public Slider xpSlider;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    
    public Player player;

    private void Start()
    {
        UpdateUI();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            currentLevel++;
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpIncreaseFactor);

            if (player != null)
            {
                player.OnLevelUp(currentLevel);
            }
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (xpSlider != null)
        {
            xpSlider.maxValue = xpToNextLevel;
            xpSlider.value = currentXP;
        }

        if (xpText != null)
        {
            xpText.text = $"{currentXP}/{xpToNextLevel}";
        }

        if (levelText != null)
        {
            levelText.text = $"{currentLevel}";
        }
    }
}
