using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelObstacle : MonoBehaviour
{
    [Header("Money Settings")]
    public int levelToUnlock = 15;

    [Header("References")]
    public GameObject modelToDisable; // Set the model to disable (or entire object)
    public TextMeshProUGUI levelText; // Show cost

    private bool hasInteracted = false;

    private void Start()
    {
        if (levelText != null)
        {
            levelText.text = $"${levelToUnlock}";
        }
    }

    public void Interact()
    {

        if (hasInteracted) return;

        if (modelToDisable != null)
        {
            modelToDisable.SetActive(false);
            hasInteracted = true;
        }
        
        
    }
}
