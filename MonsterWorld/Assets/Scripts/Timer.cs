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
        public GameObject claimButton;
        public TextMeshProUGUI countdownText;
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
    
    public GameObject countdownUI;
    public TextMeshProUGUI countdownText;

    private bool isAdCountingDown = false;
    
    [SerializeField] private GameObject newRewardNotification;
    [SerializeField] private Button dismissNotificationButton;
    private bool notificationShown = false;

    private void Start()
    {
        foreach (var btn in buttons)
        {
            if (btn.button != null)
            {
                btn.button.interactable = false;
                btn.claimButton.SetActive(false);
                btn.button.onClick.AddListener(() => OnButtonPressed(btn));
                if (btn.claimButton != null)
                {
                    Button claimBtnComponent = btn.claimButton.GetComponent<Button>();
                    if (claimBtnComponent != null)
                    {
                        claimBtnComponent.onClick.AddListener(() => OnButtonPressed(btn));
                    }
                }
            }

            if (btn.unlockImage != null)
                btn.unlockImage.enabled = false;
        }

        if (dismissNotificationButton != null)
            dismissNotificationButton.onClick.AddListener(HideNewRewardNotification);

        if (newRewardNotification != null)
            newRewardNotification.SetActive(false);
    }

    private void Update()
    {
        if (!isRunning || isAdCountingDown) return;

        timer += Time.deltaTime;
        adTimer += Time.deltaTime;

        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        timerText.text = $"{minutes}:{seconds:00}";

        foreach (var btn in buttons)
        {
            if (!btn.isActivated && !btn.isBought)
            {
                float timeLeft = btn.activationTime - timer;
                if (timeLeft <= 0f)
                {
                    if (btn.button != null)
                    {
                        btn.button.interactable = true;
                        btn.claimButton.SetActive(true);
                    }

                    if (btn.unlockImage != null)
                        btn.unlockImage.enabled = true;

                    if (btn.countdownText != null)
                        btn.countdownText.text = "";

                    btn.isActivated = true;

                    newRewardNotification.SetActive(true);
                }
                else if (btn.countdownText != null)
                {
                    int minutesLeft = Mathf.FloorToInt(timeLeft / 60f);
                    int secondsLeft = Mathf.FloorToInt(timeLeft % 60f);
                    btn.countdownText.text = $"{minutesLeft}:{secondsLeft:00}";
                }
            }
        }

        if (adTimer >= triggerInterval)
        {
            adTimer = 0f;
            StartCoroutine(AdCountdownCoroutine(3f)); // Start 3-second countdown
        }
    }
    
    private IEnumerator AdCountdownCoroutine(float countdownTime)
    {
        isAdCountingDown = true;
        countdownUI.SetActive(true);

        float timeLeft = countdownTime;
        while (timeLeft > 0)
        {
            countdownText.text = $"{Mathf.CeilToInt(timeLeft)}";
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        countdownUI.SetActive(false);
        isAdCountingDown = false;

        TriggerAd();
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
        btn.claimButton.SetActive(false);
        if (btn.unlockImage != null)
            btn.unlockImage.enabled = false;
        
        btn.claimButton.SetActive(false);

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
    
    public void HideNewRewardNotification()
    {
        if (newRewardNotification != null)
            newRewardNotification.SetActive(false);
    }
}
