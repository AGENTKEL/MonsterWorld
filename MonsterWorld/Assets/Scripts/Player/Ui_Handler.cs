using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class Ui_Handler : MonoBehaviour
{
    [Header("Main UI")]
    public GameObject mainUI;
    public GameObject petShopUI;
    public GameObject wheelUI;
    public GameObject rewardTimeUI;

    public PetManager petManager; // Assign in inspector
    
    public GameObject pcButton;
    public GameObject pcButtonTutorial;
    
    public GameObject androidButton;

    public Player _player;

    private void Start()
    {
        if (YG2.envir.isDesktop)
        {
            pcButton.SetActive(true);
            pcButtonTutorial.SetActive(true);
            androidButton.SetActive(false);
            _player.cursorVisible = false;
            _player.CursorToggle();
        }
        
        else
        {
            androidButton.SetActive(true);
            pcButton.SetActive(false);
            pcButtonTutorial.SetActive(false);
            _player.cursorVisible = true;
            _player.CursorToggle();
        }
    }

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
    
    public void Close(GameObject ui)
    {
        if (ui != null)
        {
            ui.SetActive(false);
        }
    }
}
