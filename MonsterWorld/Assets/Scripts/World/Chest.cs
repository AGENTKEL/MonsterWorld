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
        levelSystem.AddXP(levelSystem.xpToNextLevel - levelSystem.currentXP);
    }
    
}
