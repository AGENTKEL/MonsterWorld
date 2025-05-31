using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LuckyWheel : MonoBehaviour
{
    [Header("Wheel Settings")]
    public Transform wheelTransform;
    public float minSpinDuration = 3f;
    public float maxSpinDuration = 5f;

    [Header("UI")]
    public Button spinButton;
    public TextMeshProUGUI spinCostText;
    public int spinCost = 100;

    [Header("References")]
    public PetManager petManager;
    public LevelSystem levelSystem;

    private bool isSpinning = false;
    private const int SegmentCount = 8;
    private float segmentAngle = 360f / SegmentCount;

    private void Start()
    {
        spinButton.onClick.AddListener(SpinWheel);
        spinCostText.text = spinCost.ToString();
    }

    public void SpinWheel()
    {
        if (isSpinning) return;

        if (levelSystem.money < spinCost)
        {
            Debug.Log("Not enough money to spin!");
            return;
        }

        levelSystem.SubtractMoney(spinCost);

        // Pick random segment index (for choosing final landing segment)
        int selectedRewardIndex = Random.Range(0, SegmentCount);

        // Calculate total rotation: full spins + target segment angle (clockwise)
        float extraRotations = Random.Range(4, 7); // full spins
        float targetAngle = selectedRewardIndex * segmentAngle;
        float totalRotation = (extraRotations * 360f) + targetAngle;

        float spinDuration = Random.Range(minSpinDuration, maxSpinDuration);
        StartCoroutine(SpinAnimation(totalRotation, spinDuration));
    }

    private IEnumerator SpinAnimation(float totalRotation, float duration)
    {
        isSpinning = true;

        float elapsed = 0f;
        float startRotation = wheelTransform.eulerAngles.z;
        float endRotation = startRotation + totalRotation; // Spin clockwise (increase angle)

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = EaseOutCubic(t);
            float zRotation = Mathf.Lerp(startRotation, endRotation, easedT);
            wheelTransform.eulerAngles = new Vector3(0, 0, zRotation);
            elapsed += Time.deltaTime;
            yield return null;
        }

        wheelTransform.eulerAngles = new Vector3(0, 0, endRotation % 360f);

        int rewardIndex = GetRewardIndexFromAngle(wheelTransform.eulerAngles.z);
        GrantReward((RewardType)rewardIndex);
        isSpinning = false;
    }

    private float EaseOutCubic(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }

    private int GetRewardIndexFromAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0) angle += 360f;

        // Since the wheel spins clockwise, the angle directly corresponds to the segment index:
        int index = (int)(angle / segmentAngle);
        return index % SegmentCount;
    }

    private void GrantReward(RewardType reward)
    {
        switch (reward)
        {
            case RewardType.Money100:
                levelSystem.AddMoney(100);
                break;
            case RewardType.XP200:
                levelSystem.AddXP(200);
                break;
            case RewardType.Pet3:
                petManager.UnlockRandomPetByProbability();
                break;
            case RewardType.Money500:
                levelSystem.AddMoney(25);
                break;
            case RewardType.XP500:
                levelSystem.AddXP(500);
                break;
            case RewardType.Money250:
                levelSystem.AddMoney(50);
                break;
            case RewardType.Pet0:
                petManager.UnlockRandomPetByProbability();
                break;
            case RewardType.XP50:
                levelSystem.AddXP(50);
                break;
        }

        Debug.Log("You won: " + reward.ToString());
    }

    private enum RewardType
    {
        Money100 = 0,
        XP200 = 1,
        Pet3 = 2,
        Money500 = 3,
        Money250 = 4,
        XP500 = 5,
        Pet0 = 6,
        XP50 = 7
    }
}
