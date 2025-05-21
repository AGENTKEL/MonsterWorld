using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStats : MonoBehaviour
{
    public int baseHP = 100;
    public int baseDamage = 10;

    [Tooltip("HP growth rate per level in % (e.g., 0.15 = 15%)")]
    public float hpGrowthPercent = 0.15f;

    [Tooltip("Damage growth rate per level in % (e.g., 0.12 = 12%)")]
    public float damageGrowthPercent = 0.12f;

    [HideInInspector] public int currentHP;
    [HideInInspector] public int currentDamage;

    public void ScaleStats(int level)
    {
        currentHP = Mathf.RoundToInt(baseHP * Mathf.Pow(1 + hpGrowthPercent, level - 1));
        currentDamage = Mathf.RoundToInt(baseDamage * Mathf.Pow(1 + damageGrowthPercent, level - 1));
    }
}
