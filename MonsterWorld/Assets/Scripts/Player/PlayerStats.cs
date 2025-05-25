using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float hpMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float xpGainMultiplier = 1f;
    public float moveSpeedMultiplier = 1f;
    public float moneyGainMultiplier = 1f;
    public float damageRadiusMultiplier = 1f;

    public void ResetBuffs()
    {
        hpMultiplier = 1f;
        damageMultiplier = 1f;
        xpGainMultiplier = 1f;
        moveSpeedMultiplier = 1f;
        moneyGainMultiplier = 1f;
        damageRadiusMultiplier = 1f;
    }

    public void ApplyBuffs()
    {
        Debug.Log("Buffs applied:");
        Debug.Log($"HP x{hpMultiplier}, Damage x{damageMultiplier}, XP x{xpGainMultiplier}, Speed x{moveSpeedMultiplier}, Money x{moneyGainMultiplier}, Radius x{damageRadiusMultiplier}");
        // Apply changes to actual gameplay stats here.
    }
}
