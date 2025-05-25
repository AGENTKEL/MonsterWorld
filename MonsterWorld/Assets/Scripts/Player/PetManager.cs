using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YG;

public class PetManager : MonoBehaviour
{
    [System.Serializable]
    public class PetData
    {
        public GameObject petObject;
        public int cost;
        public TextMeshProUGUI costText;
        public Image boughtImage;
        public bool isBought;
    }

    public PetData[] pets = new PetData[6];

    private int currentActivePetIndex = -1;

    // Reference to the player to apply buffs
    public LevelSystem levelSystem;
    public Player player;
    public InteractionSphere interactionSphere;

    private void Start()
    {
        if (YG2.saves.isPetBought)
        {
            ActivatePet(YG2.saves.petIndex);
        }
    }


    public void TryBuyPet(int index)
    {
        if (index < 0 || index >= pets.Length)
        {
            Debug.LogError("Invalid pet index.");
            return;
        }

        PetData selectedPet = pets[index];

        // If already bought, just activate it
        if (selectedPet.isBought)
        {
            ActivatePet(index);
            return;
        }

        // Check if player has enough money
        if (levelSystem.money >= selectedPet.cost)
        {
            levelSystem.SubtractMoney(selectedPet.cost);

            // Un-buy all other pets
            for (int i = 0; i < pets.Length; i++)
            {
                pets[i].isBought = false;
                pets[i].costText.gameObject.SetActive(true);
                pets[i].boughtImage.gameObject.SetActive(false);

                Button btn = pets[i].costText.GetComponentInParent<Button>();
                if (btn != null)
                    btn.interactable = true;
            }

            // Mark selected pet as bought
            selectedPet.isBought = true;
            selectedPet.costText.gameObject.SetActive(false);
            selectedPet.boughtImage.gameObject.SetActive(true);

            Button selectedBtn = selectedPet.costText.GetComponentInParent<Button>();
            if (selectedBtn != null)
                selectedBtn.interactable = false;

            ActivatePet(index);
        }
        else
        {
            Debug.Log("Not enough money to buy this pet!");
        }
    }
    
    public void TryGetPet(int index)
    {
        if (index < 0 || index >= pets.Length)
        {
            Debug.LogError("Invalid pet index.");
            return;
        }

        PetData selectedPet = pets[index];

        // If already bought, just activate it
        if (selectedPet.isBought)
        {
            ActivatePet(index);
            return;
        }

        // Un-buy all other pets
        for (int i = 0; i < pets.Length; i++)
        {
            pets[i].isBought = false;
            pets[i].costText.gameObject.SetActive(true);
            pets[i].boughtImage.gameObject.SetActive(false);

            Button btn = pets[i].costText.GetComponentInParent<Button>();
            if (btn != null)
                btn.interactable = true;
        }

        // Mark selected pet as bought
        selectedPet.isBought = true;
        selectedPet.costText.gameObject.SetActive(false);
        selectedPet.boughtImage.gameObject.SetActive(true);

        Button selectedBtn = selectedPet.costText.GetComponentInParent<Button>();
        if (selectedBtn != null)
            selectedBtn.interactable = false;

        ActivatePet(index);
    }

    private void ActivatePet(int index)
    {
        // Disable all pets
        for (int i = 0; i < pets.Length; i++)
        {
            pets[i].petObject.SetActive(false);
        }

        // Enable selected pet
        pets[index].petObject.SetActive(true);
        currentActivePetIndex = index;
        YG2.saves.petIndex = index;
        YG2.saves.isPetBought = true;
        YG2.SaveProgress();

        ApplyPetBuff(index);
    }

    private void ApplyPetBuff(int index)
    {

        switch (index)
        {
            case 0: player.maxHP = player.maxHP + Mathf.RoundToInt(player.maxHP * 0.25f);
               player.Heal(0); break;
            case 1: player.damage = player.damage + Mathf.RoundToInt(player.damage * 0.25f); break;
            case 2: levelSystem.isXpPet = true; break;
            case 3: player.moveSpeed = player.moveSpeed * 1.25f; break;
            case 4: levelSystem.isXpPet = true; break;
            case 5: interactionSphere.maxRadius = interactionSphere.maxRadius * 1.25f; break;
        }
        
    }
    
    public void UpdatePetShopUI()
    {
        for (int i = 0; i < pets.Length; i++)
        {
            PetData pet = pets[i];

            if (pet.isBought)
            {
                pet.costText.gameObject.SetActive(false);
                pet.boughtImage.gameObject.SetActive(true);

                // Disable the button (assumes the button is on the same GameObject or nearby)
                Button button = pet.costText.GetComponentInParent<Button>();
                if (button != null)
                {
                    button.interactable = false;
                }
            }
        }
    }
}
