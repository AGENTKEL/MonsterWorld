using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    [Header("Chest Settings")]
    public int price = 100;
    public bool isBought = false;

    [Header("References")]
    public Animator animator;
    public TextMeshProUGUI priceText;
    public GameObject worldSpaceUI;
    
    [Header("Reward Probabilities")]
    [Range(0f, 1f)] public float moneyChance = 0.33f;
    [Range(0f, 1f)] public float xpChance = 0.33f;
    [Range(0f, 1f)] public float petChance = 0.34f;
    
    [Header("Reward Values")]
    public int moneyRewardMin = 50;
    public int moneyRewardMax = 200;
    public int xpRewardMin = 25;
    public int xpRewardMax = 100;
    
    [Header("Reward UI")]
    public GameObject rewardUI; // UI root to show (set inactive by default)
    public Image rewardImage;   // Image to change based on reward
    public TextMeshProUGUI rewardText; // Text to show amount or "Pet"

    [Header("Reward Sprites")]
    public Sprite moneySprite;
    public Sprite xpSprite;
    public Sprite petSprite;
    
    public PetManager petManager;

    private void Start()
    {
        if (priceText != null)
        {
            priceText.text = $"${price}";
        }
    }

    public void Interact(LevelSystem levelSystem)
    {
        if (isBought) return;

        if (levelSystem.money >= price)
        {
            levelSystem.SubtractMoney(price);
            isBought = true;
            animator.SetBool("Open", true);
            worldSpaceUI.SetActive(false);

            GiveRandomReward(levelSystem);
        }
    }
    
    private void GiveRandomReward(LevelSystem levelSystem)
    {
        float rand = UnityEngine.Random.value;

        if (rand <= moneyChance)
        {
            int reward = Random.Range(moneyRewardMin, moneyRewardMax + 1);
            levelSystem.AddMoney(reward);
            ShowRewardUI(moneySprite, $"${reward}");
            Debug.Log($"Received money: ${reward}");
        }
        else if (rand <= moneyChance + xpChance)
        {
            int reward = Random.Range(xpRewardMin, xpRewardMax + 1);
            levelSystem.AddXP(reward);
            ShowRewardUI(xpSprite, $"{reward} XP");
            Debug.Log($"Received XP: {reward}");
        }
        else
        {
            if (petManager != null)
            {
                petManager.UnlockRandomPetByProbability();
                ShowRewardUI(petSprite, "Pet");
                Debug.Log("Unlocked a random pet!");
            }
            else
            {
                Debug.LogWarning("PetManager not assigned!");
            }
        }
    }
    
    private void ShowRewardUI(Sprite sprite, string text)
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);

        if (rewardImage != null)
            rewardImage.sprite = sprite;

        if (rewardText != null)
            rewardText.text = text;
    }
}
