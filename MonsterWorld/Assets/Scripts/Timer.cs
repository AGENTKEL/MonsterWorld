using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class Timer : MonoBehaviour
{
    public enum RewardType
    {
        Money,
        XP,
        Pet
    }
    
    [System.Serializable]
    public class TimedButton
    {
        public Button button;
        public Image unlockImage;        // The image to activate when time is up
        public float activationTime;     // Time (in seconds) after which the button becomes active
        public RewardType rewardType;
        public int rewardAmount;
        [HideInInspector] public bool isActivated = false;
        [HideInInspector] public bool isBought = false;
    }
    
    public List<TimedButton> buttons = new List<TimedButton>();
    
    public LevelSystem levelSystem;
    public PetManager petManager;
    
    [SerializeField] private float timer = 0f;
    private bool isRunning = true;
    public TextMeshProUGUI timerText;
    
    [SerializeField] private float adTimer = 0f;
    private const float triggerInterval = 150f; // 2.5 minutes in seconds

    private void Start()
    {
        foreach (var btn in buttons)
        {
            if (btn.button != null)
            {
                btn.button.interactable = false;
                btn.button.onClick.AddListener(() => OnButtonPressed(btn));
            }

            if (btn.unlockImage != null)
                btn.unlockImage.enabled = false;
        }
    }

    private void Update()
    {
        if (!isRunning) return;

        timer += Time.deltaTime;
        adTimer += Time.deltaTime;

        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        timerText.text = $"{minutes}:{seconds:00}";

        foreach (var btn in buttons)
        {
            if (!btn.isActivated && timer >= btn.activationTime && !btn.isBought)
            {
                if (btn.button != null)
                    btn.button.interactable = true;

                if (btn.unlockImage != null)
                    btn.unlockImage.enabled = true;

                btn.isActivated = true;
            }
        }

        if (adTimer >= triggerInterval)
        {
            adTimer = 0f;
            TriggerAd();
        }
    }
    
    private void TriggerAd()
    {
        YG2.InterstitialAdvShow();
    }
    
    private void OnButtonPressed(TimedButton btn)
    {
        // Grant reward
        switch (btn.rewardType)
        {
            case RewardType.Money:
                levelSystem.AddMoney(btn.rewardAmount);
                break;
            case RewardType.XP:
                levelSystem.AddXP(btn.rewardAmount);
                break;
            case RewardType.Pet:
                petManager.TryGetPet(5);
                break;
        }

        // Disable the button again
        btn.isBought = true;
        btn.button.interactable = false;
        if (btn.unlockImage != null)
            btn.unlockImage.enabled = false;

        Debug.Log($"Button bought. Granted {btn.rewardAmount} {btn.rewardType}");
    }

    // Optional: Call this to stop the timer
    public void StopTimer()
    {
        isRunning = false;
    }

    // Optional: Call this to reset the timer to 0
    public void ResetTimer()
    {
        timer = 0f;
        isRunning = true;
    }

    public void SetTimeLeaderBoard()
    {
        if (YG2.saves.timeRecord < timer)
        {
            YG2.SetLBTimeConvert("MonsterGameLeaderBoardTimeYG2", timer);
            YG2.saves.timeRecord = timer;
        }
    }
}
