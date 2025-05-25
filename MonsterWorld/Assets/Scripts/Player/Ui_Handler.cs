using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ui_Handler : MonoBehaviour
{
    [Header("Main UI")]
    public GameObject mainUI;
    public GameObject petShopUI;
    public GameObject wheelUI;
    public GameObject rewardTimeUI;

    public PetManager petManager; // Assign in inspector

    public void TogglePetShop()
    {
        bool isActive = petShopUI.activeSelf;

        // Toggle pet shop
        petShopUI.SetActive(!isActive);

        // Disable others
        wheelUI.SetActive(false);
        rewardTimeUI.SetActive(false);

        // Update pet UI when opening
        if (!isActive && petManager != null)
        {
            petManager.UpdatePetShopUI();
        }
    }

    public void ToggleWheel()
    {
        bool isActive = wheelUI.activeSelf;

        // Toggle wheel
        wheelUI.SetActive(!isActive);

        // Disable others
        petShopUI.SetActive(false);
        rewardTimeUI.SetActive(false);
    }
    
    public void ToggleRewards()
    {
        bool isActive = rewardTimeUI.activeSelf;

        // Toggle wheel
        rewardTimeUI.SetActive(!isActive);

        // Disable others
        petShopUI.SetActive(false);
        wheelUI.SetActive(false);
    }
}
