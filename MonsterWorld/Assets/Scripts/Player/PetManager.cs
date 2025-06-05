using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YG;

public class PetManager : MonoBehaviour
{
    
    public enum PetRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }
    
    [System.Serializable]
    public class PetData
    {
        public GameObject petObject;
        public int cost;
        public Image boughtImage;
        public Image petImage;
        public bool isBought;
        public Button petButton;

        public PetRarity rarity;
        [Range(0f, 1f)] public float hpBonus;
        [Range(0f, 1f)] public float dmgBonus;
        [Range(0f, 1f)] public float xpMultiplierBonus;
        [Range(0f, 1f)] public float moveSpeedBonus;
    }

    public PetData[] pets = new PetData[6];

    private int currentActivePetIndex = -1;
    private int currentBuffedPetIndex = -1;

    // Reference to the player to apply buffs
    public LevelSystem levelSystem;
    public Player player;
    public InteractionSphere interactionSphere;
    
    private float[] unlockProbabilities = new float[] { 0.3f, 0.25f, 0.2f, 0.15f, 0.07f, 0.03f };
    
    public GameObject newPetUnlockedImage;
    public Button acknowledgeButton;
    
    
    [Header("Pet Lootbox")]
    public GameObject unlockedPetDisplay;
    public Image unlockedPetImageDisplay; // UI Image for showing the unlocked pet
    public TextMeshProUGUI unlockedPetNameDisplay; // Pet name text (use TextMeshProUGUI)
    public Button buyRandomPetButton;
    
    private Dictionary<PetRarity, float> rarityChances = new Dictionary<PetRarity, float>
    {
        { PetRarity.Common, 0.8f },
        { PetRarity.Uncommon, 0.6f },
        { PetRarity.Rare, 0.4f },
        { PetRarity.Epic, 0.2f },
        { PetRarity.Legendary, 0.1f },
        { PetRarity.Mythic, 0.05f }
    };

    private void Start()
    {
        for (int i = 0; i < pets.Length; i++)
        {
            if (pets[i].petButton != null)
            {
                // Restore saved state if available
                if (i < YG2.saves.petButtonUnlocked.Length && YG2.saves.petButtonUnlocked[i])
                    pets[i].petButton.interactable = true;
                else
                    pets[i].petButton.interactable = false;
            }
        }
        
        if (acknowledgeButton != null)
            acknowledgeButton.onClick.AddListener(HideNewPetImage);
        
        if (YG2.saves.isPetBought)
        {
            StartCoroutine(LoadStats());
        }
        
    }
    
    private IEnumerator LoadStats()
    {
        yield return new WaitForSeconds(0.6f);
        ActivatePet(YG2.saves.petIndex);
    }
    
    public void UnlockRandomPetByProbability()
    {
        List<int> eligibleIndexes = new List<int>();
        List<float> weightedChances = new List<float>();

        for (int i = 0; i < pets.Length; i++)
        {
            if (!pets[i].petButton.interactable)
            {
                eligibleIndexes.Add(i);
                weightedChances.Add(rarityChances[pets[i].rarity]);
            }
        }

        if (eligibleIndexes.Count == 0)
        {
            Debug.Log("All pets unlocked!");
            return;
        }

        float total = weightedChances.Sum();
        float rand = UnityEngine.Random.value * total;

        float cumulative = 0f;
        for (int i = 0; i < eligibleIndexes.Count; i++)
        {
            cumulative += weightedChances[i];
            if (rand <= cumulative)
            {
                UnlockPetButton(eligibleIndexes[i]);
                return;
            }
        }

        UnlockPetButton(eligibleIndexes.Last()); // Fallback
    }
    
    public void UnlockPetButton(int index)
    {
        if (index < 0 || index >= pets.Length)
        {
            Debug.LogError("Invalid pet index to unlock.");
            return;
        }

        PetData pet = pets[index];

        if (pet.petButton != null)
        {
            pet.petButton.interactable = true;

            // Save to YG2
            YG2.saves.petButtonUnlocked[index] = true;
            YG2.SaveProgress();
        }
        unlockedPetDisplay.SetActive(true);

        if (newPetUnlockedImage != null)
            newPetUnlockedImage.SetActive(true);

        if (unlockedPetImageDisplay != null)
            unlockedPetImageDisplay.sprite = pet.petImage.sprite;
        

        if (unlockedPetNameDisplay != null)
            unlockedPetNameDisplay.text = pet.petObject.name;
        
        
        if (AreAllPetsUnlocked() && buyRandomPetButton != null)
            buyRandomPetButton.gameObject.SetActive(false);

        Debug.Log($"Unlocked pet: {pet.petObject.name} at index {index}");
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
        }
        else
        {
            ActivatePet(index);
        }
        
        // Un-buy all other pets
        for (int i = 0; i < pets.Length; i++)
        {
            pets[i].isBought = false;
            pets[i].boughtImage.gameObject.SetActive(false);
            
        }

        // Mark selected pet as bought
        selectedPet.isBought = true;
        selectedPet.boughtImage.gameObject.SetActive(true);
        
        
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
            pets[i].boughtImage.gameObject.SetActive(false);
            
        }

        // Mark selected pet as bought
        selectedPet.isBought = true;
        selectedPet.boughtImage.gameObject.SetActive(true);
        

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
        // Hide button if all are already unlocked
        if (AreAllPetsUnlocked() && buyRandomPetButton != null)
            buyRandomPetButton.gameObject.SetActive(false);
    }

    private void ApplyPetBuff(int index)
    {
        // Remove old buffs if any
        if (currentBuffedPetIndex != -1)
        {
            PetData oldPet = pets[currentBuffedPetIndex];
            player.maxHP = Mathf.RoundToInt(player.maxHP / (1 + oldPet.hpBonus));
            player.damage = Mathf.RoundToInt(player.damage / (1 + oldPet.dmgBonus));
            player.moveSpeed = player.moveSpeed / (1 + oldPet.moveSpeedBonus);
            levelSystem.xpBonusMultiplier -= oldPet.xpMultiplierBonus;
        }

        // Apply new buffs
        PetData newPet = pets[index];
        player.maxHP = Mathf.RoundToInt(player.maxHP * (1 + newPet.hpBonus));
        player.damage = Mathf.RoundToInt(player.damage * (1 + newPet.dmgBonus));
        player.moveSpeed = player.moveSpeed * (1 + newPet.moveSpeedBonus);
        levelSystem.xpBonusMultiplier += newPet.xpMultiplierBonus;

        // Track the new buffed pet
        currentBuffedPetIndex = index;

        player.Heal(0); // Refresh HP UI
        
        // Un-buy all other pets
        for (int i = 0; i < pets.Length; i++)
        {
            pets[i].isBought = false;
            pets[i].boughtImage.gameObject.SetActive(false);
            
        }

        // Mark selected pet as bought
        newPet.isBought = true;
        newPet.boughtImage.gameObject.SetActive(true);
    }
    
    public void UpdatePetBuff()
    {
        if (currentActivePetIndex != -1)
        {
            // Apply new buffs
            PetData newPet = pets[currentActivePetIndex];
            player.maxHP = Mathf.RoundToInt(player.maxHP * (1 + newPet.hpBonus));
            player.damage = Mathf.RoundToInt(player.damage * (1 + newPet.dmgBonus));

            // Track the new buffed pet
            currentBuffedPetIndex = currentActivePetIndex;

            player.Heal(0); // Refresh HP UI
        }
        
    }
    
    public void UpdatePetShopUI()
    {
        for (int i = 0; i < pets.Length; i++)
        {
            PetData pet = pets[i];

            if (pet.isBought)
            {
                pet.boughtImage.gameObject.SetActive(true);
                
            }
        }
    }
    
    public void HideNewPetImage()
    {
        if (newPetUnlockedImage != null)
            newPetUnlockedImage.SetActive(false);
    }
    
    private bool AreAllPetsUnlocked()
    {
        for (int i = 0; i < pets.Length; i++)
        {
            if (!pets[i].petButton.interactable)
                return false;
        }
        return true;
    }
}
