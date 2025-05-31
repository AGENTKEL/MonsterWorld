using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

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

    [HideInInspector] public float xpBonusMultiplier = 1f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip levelUpSound;
    

    private void Start()
    {
        StartCoroutine(LoadStats());
        UpdateUI();
    }

    private IEnumerator LoadStats()
    {
        yield return new WaitForSeconds(0.5f);
        money = YG2.saves.coins;
        currentLevel = YG2.saves.level;
        xpToNextLevel = YG2.saves.xpToTheNextLevel;
        player.OnLevelUp(currentLevel);
        UpdateUI();
        CheckLocations();
    }

    public void AddXP(int amount)
    {
        int finalAmount = Mathf.RoundToInt(amount * xpBonusMultiplier);
        currentXP += finalAmount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            currentLevel++;
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpIncreaseFactor);
            YG2.saves.xpToTheNextLevel = xpToNextLevel;
            YG2.saves.level = currentLevel;
            YG2.SaveProgress();

            if (player != null)
            {
                CheckLocations();
                UpdateUI();
                player.OnLevelUp(currentLevel);
                if (audioSource != null && levelUpSound != null)
                {
                    audioSource.PlayOneShot(levelUpSound);
                }
            }
        }
        
        UpdateUI();
    }

    public void CheckLocations()
    {
        if (currentLevel >= _levelObstacleDesert.levelToUnlock)
        {
            _levelObstacleDesert.Interact();
        }
                
        if (currentLevel >= _levelObstacleSnow.levelToUnlock)
        {
            _levelObstacleSnow.Interact();
        }
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoneyUI();
        if (YG2.saves.coinsRecord < money)
        {
            YG2.SetLeaderboard("MonsterGameLeaderBoardGoldenYG2", money);
            YG2.saves.coinsRecord = money;
        }
        YG2.saves.coins = money;
        YG2.SaveProgress();
    }

    public void SubtractMoney(int amount)
    {
        money -= amount;
        if (money < 0) money = 0;
        UpdateMoneyUI();
        YG2.saves.coins = money;
        YG2.SaveProgress();
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
