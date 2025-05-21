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
    public float xpIncreaseFactor = 1.25f;

    [Header("UI References")]
    public Slider xpSlider;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI moneyText;

    [Header("Money Settings")]
    public int money = 0;

    public Player player;

    [Header("Level Obstacles")] 
    [SerializeField] private LevelObstacle _levelObstacleDesert;
    [SerializeField] private LevelObstacle _levelObstacleSnow;

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
                if (currentLevel >= _levelObstacleDesert.levelToUnlock)
                {
                    _levelObstacleDesert.Interact();
                }
                
                if (currentLevel >= _levelObstacleSnow.levelToUnlock)
                {
                    _levelObstacleSnow.Interact();
                }
            }
        }

        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoneyUI();
    }

    public void SubtractMoney(int amount)
    {
        money -= amount;
        if (money < 0) money = 0;
        UpdateMoneyUI();
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

        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"{money}";
        }
    }

    public void ForceUpgrade()
    {
        AddXP(xpToNextLevel);
    }
}
