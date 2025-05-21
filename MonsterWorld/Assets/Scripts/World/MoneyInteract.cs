using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyInteract : MonoBehaviour
{
    [Header("Money Settings")]
    public int moneyCost = 50;

    [Header("References")]
    public GameObject modelToDisable; // Set the model to disable (or entire object)
    public TextMeshProUGUI costText; // Show cost

    private bool hasInteracted = false;

    private void Start()
    {
        if (costText != null)
        {
            costText.text = $"${moneyCost}";
        }
    }

    public void Interact(LevelSystem _levelSystem)
    {
        if (hasInteracted) return;

        if (_levelSystem != null && _levelSystem.money >= moneyCost)
        {
            _levelSystem.SubtractMoney(moneyCost);
            if (modelToDisable != null)
            {
                modelToDisable.SetActive(false);
            }

            hasInteracted = true;
        }
        else
        {
            Debug.Log("Not enough money to interact.");
        }
    }
}
