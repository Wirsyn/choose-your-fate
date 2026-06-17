using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    [Header("Baza Danych Przedmiotów")]
    public List<ItemData> allPossibleItems;

    [Header("Wyposa¿enie")]
    public ItemData equippedWeapon;
    public ItemData equippedShield;
    public ItemData equippedHelmet;
    public ItemData equippedArmor;
    public ItemData equippedTrinket;
    public ItemData equippedRing;
    public ItemData equippedPotion;
    public ItemData equippedBoots;
    public ItemData equippedGloves;

    [Header("G³ówny Plecak")]
    public int maxBackpackSlots = 20;
    public List<ItemData> backpack = new List<ItemData>();

    [Header("Ma³y Plecak (Small Items)")]
    public int maxSmallBackpackSlots = 3;
    public List<ItemData> smallBackpack = new List<ItemData>();

    public int GetCurrentBackpackLoad()
    {
        int load = 0;
        foreach (ItemData item in backpack)
        {
            load += item.slotsTaken;
        }
        return load;
    }

    public bool AddToBackpack(ItemData newItem)
    {
        if (newItem == null) return false;

        // --- POPRAWKA: Mikstury traktujemy jako zwyk³y, du¿y, niestockowalny przedmiot! ---
        if (newItem.slotType == EquipmentSlot.SmallItem)
        {
            if (newItem.isStackable)
            {
                foreach (ItemData item in smallBackpack)
                {
                    if (item.itemName == newItem.itemName && item.currentStack < item.maxStack)
                    {
                        int spaceLeft = item.maxStack - item.currentStack;
                        if (newItem.currentStack <= spaceLeft)
                        {
                            item.currentStack += newItem.currentStack;
                            return true;
                        }
                        else
                        {
                            item.currentStack += spaceLeft;
                            newItem.currentStack -= spaceLeft;
                        }
                    }
                }
            }
            if (smallBackpack.Count < maxSmallBackpackSlots)
            {
                smallBackpack.Add(InstantiateItemSafely(newItem));
                return true;
            }
            Debug.Log("Ma³y plecak pe³en!");
            return false;
        }
        else
        {
            // Potion i wszystkie inne za³o¿eniowe itemy
            if (newItem.isStackable && newItem.slotType != EquipmentSlot.Potion)
            {
                foreach (ItemData item in backpack)
                {
                    if (item.itemName == newItem.itemName && item.currentStack < item.maxStack)
                    {
                        int spaceLeft = item.maxStack - item.currentStack;
                        if (newItem.currentStack <= spaceLeft)
                        {
                            item.currentStack += newItem.currentStack;
                            return true;
                        }
                        else
                        {
                            item.currentStack += spaceLeft;
                            newItem.currentStack -= spaceLeft;
                        }
                    }
                }
            }

            if (GetCurrentBackpackLoad() + newItem.slotsTaken <= maxBackpackSlots)
            {
                backpack.Add(InstantiateItemSafely(newItem));
                return true;
            }
            Debug.Log("G³ówny plecak pe³en!");
            return false;
        }
    }

    private ItemData InstantiateItemSafely(ItemData originalItem)
    {
        if (!originalItem.name.EndsWith("(Clone)"))
        {
            ItemData clone = Instantiate(originalItem);
            clone.name = originalItem.name + "(Clone)";
            clone.currentDurability = originalItem.currentDurability < originalItem.maxDurability ? originalItem.currentDurability : clone.maxDurability;
            clone.currentStack = originalItem.currentStack;
            return clone;
        }
        return originalItem;
    }

    public void EquipFromBackpack(ItemData itemToEquip)
    {
        if (itemToEquip == null) return;

        if (smallBackpack.Contains(itemToEquip))
        {
            smallBackpack.Remove(itemToEquip);
        }
        else if (backpack.Contains(itemToEquip))
        {
            backpack.Remove(itemToEquip);
        }

        // Zabezpieczenie na wypadek, gdyby gracz próbowa³ za³o¿yæ "ma³y przedmiot" (SmallItem) z plecaka
        if (itemToEquip.slotType == EquipmentSlot.SmallItem)
        {
            smallBackpack.Add(itemToEquip);
            return;
        }

        EquipItem(itemToEquip);
    }

    public void EquipItem(ItemData item)
    {
        if (item == null) return;

        switch (item.slotType)
        {
            case EquipmentSlot.Weapon:
                if (equippedWeapon != null) AddToBackpack(equippedWeapon);
                equippedWeapon = item;
                break;
            case EquipmentSlot.Shield:
                if (equippedShield != null) AddToBackpack(equippedShield);
                equippedShield = item;
                break;
            case EquipmentSlot.Helmet:
                if (equippedHelmet != null) AddToBackpack(equippedHelmet);
                equippedHelmet = item;
                break;
            case EquipmentSlot.Armor:
                if (equippedArmor != null) AddToBackpack(equippedArmor);
                equippedArmor = item;
                break;
            case EquipmentSlot.Trinket:
                if (equippedTrinket != null) AddToBackpack(equippedTrinket);
                equippedTrinket = item;
                break;
            case EquipmentSlot.Ring:
                if (equippedRing != null) AddToBackpack(equippedRing);
                equippedRing = item;
                break;
            case EquipmentSlot.Potion:
                if (equippedPotion != null) AddToBackpack(equippedPotion);
                equippedPotion = item;
                break;
            case EquipmentSlot.Boots:
                if (equippedBoots != null) AddToBackpack(equippedBoots);
                equippedBoots = item;
                break;
            case EquipmentSlot.Gloves:
                if (equippedGloves != null) AddToBackpack(equippedGloves);
                equippedGloves = item;
                break;
        }
    }

    public void UnequipItem(EquipmentSlot slot, bool addToBackpack = true)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon:
                if (equippedWeapon != null) { if (addToBackpack && !AddToBackpack(equippedWeapon)) return; equippedWeapon = null; }
                break;
            case EquipmentSlot.Shield:
                if (equippedShield != null) { if (addToBackpack && !AddToBackpack(equippedShield)) return; equippedShield = null; }
                break;
            case EquipmentSlot.Helmet:
                if (equippedHelmet != null) { if (addToBackpack && !AddToBackpack(equippedHelmet)) return; equippedHelmet = null; }
                break;
            case EquipmentSlot.Armor:
                if (equippedArmor != null) { if (addToBackpack && !AddToBackpack(equippedArmor)) return; equippedArmor = null; }
                break;
            case EquipmentSlot.Trinket:
                if (equippedTrinket != null) { if (addToBackpack && !AddToBackpack(equippedTrinket)) return; equippedTrinket = null; }
                break;
            case EquipmentSlot.Ring:
                if (equippedRing != null) { if (addToBackpack && !AddToBackpack(equippedRing)) return; equippedRing = null; }
                break;
            case EquipmentSlot.Potion:
                if (equippedPotion != null) { if (addToBackpack && !AddToBackpack(equippedPotion)) return; equippedPotion = null; }
                break;
            case EquipmentSlot.Boots:
                if (equippedBoots != null) { if (addToBackpack && !AddToBackpack(equippedBoots)) return; equippedBoots = null; }
                break;
            case EquipmentSlot.Gloves:
                if (equippedGloves != null) { if (addToBackpack && !AddToBackpack(equippedGloves)) return; equippedGloves = null; }
                break;
        }
    }

    public void RemoveItem(ItemData itemToRemove)
    {
        if (itemToRemove == null) return;
        if (backpack.Contains(itemToRemove)) backpack.Remove(itemToRemove);
        else if (smallBackpack.Contains(itemToRemove)) smallBackpack.Remove(itemToRemove);
        else if (equippedWeapon == itemToRemove) equippedWeapon = null;
        else if (equippedShield == itemToRemove) equippedShield = null;
        else if (equippedHelmet == itemToRemove) equippedHelmet = null;
        else if (equippedArmor == itemToRemove) equippedArmor = null;
        else if (equippedTrinket == itemToRemove) equippedTrinket = null;
        else if (equippedRing == itemToRemove) equippedRing = null;
        else if (equippedPotion == itemToRemove) equippedPotion = null;
        else if (equippedBoots == itemToRemove) equippedBoots = null;
        else if (equippedGloves == itemToRemove) equippedGloves = null;
    }

    public void ApplyUpgradeStats(ItemData item)
    {
        if (item == null) return;
        if (item.bonusAttackTokens > 0) item.bonusAttackTokens += 1;
        if (item.bonusDefenseTokens > 0) item.bonusDefenseTokens += 1;
        if (item.bonusMaxHP > 0) item.bonusMaxHP += 2;
        if (item.bonusMaxBet > 0) item.bonusMaxBet += 1;
        if (item.bonusBaseDamage > 0) item.bonusBaseDamage += 2;
        if (item.restoreHP > 0) item.restoreHP += 5;
        if (item.restoreArmor > 0) item.restoreArmor += 5;
        if (item.bonusGoldMultiplier > 0f) item.bonusGoldMultiplier += 0.1f;
    }

    public List<string> DecreaseDurabilityAndGetDestroyed()
    {
        List<string> destroyedItems = new List<string>();

        int damageToDurability = 10;

        if (equippedWeapon != null) { equippedWeapon.currentDurability -= damageToDurability; if (equippedWeapon.currentDurability <= 0) { destroyedItems.Add(equippedWeapon.itemName); equippedWeapon = null; } }
        if (equippedShield != null) { equippedShield.currentDurability -= damageToDurability; if (equippedShield.currentDurability <= 0) { destroyedItems.Add(equippedShield.itemName); equippedShield = null; } }
        if (equippedHelmet != null) { equippedHelmet.currentDurability -= damageToDurability; if (equippedHelmet.currentDurability <= 0) { destroyedItems.Add(equippedHelmet.itemName); equippedHelmet = null; } }
        if (equippedArmor != null) { equippedArmor.currentDurability -= damageToDurability; if (equippedArmor.currentDurability <= 0) { destroyedItems.Add(equippedArmor.itemName); equippedArmor = null; } }
        if (equippedTrinket != null) { equippedTrinket.currentDurability -= damageToDurability; if (equippedTrinket.currentDurability <= 0) { destroyedItems.Add(equippedTrinket.itemName); equippedTrinket = null; } }
        if (equippedRing != null) { equippedRing.currentDurability -= damageToDurability; if (equippedRing.currentDurability <= 0) { destroyedItems.Add(equippedRing.itemName); equippedRing = null; } }
        if (equippedBoots != null) { equippedBoots.currentDurability -= damageToDurability; if (equippedBoots.currentDurability <= 0) { destroyedItems.Add(equippedBoots.itemName); equippedBoots = null; } }
        if (equippedGloves != null) { equippedGloves.currentDurability -= damageToDurability; if (equippedGloves.currentDurability <= 0) { destroyedItems.Add(equippedGloves.itemName); equippedGloves = null; } }

        return destroyedItems;
    }

    public void SaveEquipment(SaveData data)
    {
        data.savedWeaponName = equippedWeapon != null ? equippedWeapon.itemName + "#" + equippedWeapon.upgradeLevel + "#" + equippedWeapon.currentDurability : "";
        data.savedShieldName = equippedShield != null ? equippedShield.itemName + "#" + equippedShield.upgradeLevel + "#" + equippedShield.currentDurability : "";
        data.savedHelmetName = equippedHelmet != null ? equippedHelmet.itemName + "#" + equippedHelmet.upgradeLevel + "#" + equippedHelmet.currentDurability : "";
        data.savedArmorName = equippedArmor != null ? equippedArmor.itemName + "#" + equippedArmor.upgradeLevel + "#" + equippedArmor.currentDurability : "";
        data.savedTrinketName = equippedTrinket != null ? equippedTrinket.itemName + "#" + equippedTrinket.upgradeLevel + "#" + equippedTrinket.currentDurability : "";
        data.savedRing = equippedRing != null ? equippedRing.itemName + "#" + equippedRing.upgradeLevel + "#" + equippedRing.currentDurability : "";
        data.savedPotion = equippedPotion != null ? equippedPotion.itemName + "#" + equippedPotion.upgradeLevel + "#" + equippedPotion.currentDurability : "";
        data.savedBootsName = equippedBoots != null ? equippedBoots.itemName + "#" + equippedBoots.upgradeLevel + "#" + equippedBoots.currentDurability : "";
        data.savedGlovesName = equippedGloves != null ? equippedGloves.itemName + "#" + equippedGloves.upgradeLevel + "#" + equippedGloves.currentDurability : "";

        data.savedBackpack.Clear();
        foreach (ItemData item in backpack)
        {
            if (item != null) data.savedBackpack.Add(item.itemName + "#" + item.upgradeLevel + "#" + item.currentDurability + "#" + item.currentStack);
        }

        data.savedSmallBackpack.Clear();
        foreach (ItemData item in smallBackpack)
        {
            if (item != null) data.savedSmallBackpack.Add(item.itemName + "#" + item.upgradeLevel + "#" + item.currentDurability + "#" + item.currentStack);
        }
    }

    public void LoadEquipment(SaveData data)
    {
        equippedWeapon = LoadItemFromString(data.savedWeaponName);
        equippedShield = LoadItemFromString(data.savedShieldName);
        equippedHelmet = LoadItemFromString(data.savedHelmetName);
        equippedArmor = LoadItemFromString(data.savedArmorName);
        equippedTrinket = LoadItemFromString(data.savedTrinketName);
        equippedRing = LoadItemFromString(data.savedRing);
        equippedPotion = LoadItemFromString(data.savedPotion);
        equippedBoots = LoadItemFromString(data.savedBootsName);
        equippedGloves = LoadItemFromString(data.savedGlovesName);

        backpack.Clear();
        foreach (string itemDataString in data.savedBackpack)
        {
            ItemData loadedItem = LoadItemFromString(itemDataString);
            if (loadedItem != null) backpack.Add(loadedItem);
        }

        smallBackpack.Clear();
        foreach (string itemDataString in data.savedSmallBackpack)
        {
            ItemData loadedItem = LoadItemFromString(itemDataString);
            if (loadedItem != null) smallBackpack.Add(loadedItem);
        }
    }

    private ItemData LoadItemFromString(string itemDataString)
    {
        if (string.IsNullOrEmpty(itemDataString)) return null;

        string[] parts = itemDataString.Split('#');
        string name = parts[0];
        int upgLvl = parts.Length > 1 ? int.Parse(parts[1]) : 0;
        int dur = parts.Length > 2 ? int.Parse(parts[2]) : 100;
        int stack = parts.Length > 3 ? int.Parse(parts[3]) : 1;

        ItemData referenceItem = allPossibleItems.Find(i => i.itemName == name);
        if (referenceItem != null)
        {
            ItemData instItem = Instantiate(referenceItem);
            instItem.upgradeLevel = upgLvl;
            instItem.currentDurability = dur;
            instItem.currentStack = stack;

            for (int i = 0; i < upgLvl; i++) ApplyUpgradeStats(instItem);

            return instItem;
        }
        return null;
    }

    public void InitializeStartingEquipment(PlayerClassData classData)
    {
        if (classData == null) return;

        if (classData.startingWeapon != null) equippedWeapon = Instantiate(classData.startingWeapon);
        if (classData.startingShield != null) equippedShield = Instantiate(classData.startingShield);
        if (classData.startingHelmet != null) equippedHelmet = Instantiate(classData.startingHelmet);
        if (classData.startingArmor != null) equippedArmor = Instantiate(classData.startingArmor);
        if (classData.startingTrinket != null) equippedTrinket = Instantiate(classData.startingTrinket);
        if (classData.startingRing != null) equippedRing = Instantiate(classData.startingRing);
        if (classData.startingPotion != null) equippedPotion = Instantiate(classData.startingPotion);
        if (classData.startingBoots != null) equippedBoots = Instantiate(classData.startingBoots);
        if (classData.startingGloves != null) equippedGloves = Instantiate(classData.startingGloves);

        backpack.Clear();
        smallBackpack.Clear();

        foreach (ItemData item in classData.startingBackpack)
        {
            if (item != null) AddToBackpack(Instantiate(item));
        }
    }

    public int GetTotalBaseDamageBonus()
    {
        int bonus = 0;
        if (equippedWeapon != null) bonus += equippedWeapon.bonusBaseDamage;
        if (equippedShield != null) bonus += equippedShield.bonusBaseDamage;
        if (equippedHelmet != null) bonus += equippedHelmet.bonusBaseDamage;
        if (equippedArmor != null) bonus += equippedArmor.bonusBaseDamage;
        if (equippedTrinket != null) bonus += equippedTrinket.bonusBaseDamage;
        if (equippedRing != null) bonus += equippedRing.bonusBaseDamage;
        if (equippedBoots != null) bonus += equippedBoots.bonusBaseDamage;
        if (equippedGloves != null) bonus += equippedGloves.bonusBaseDamage;
        return bonus;
    }

    public int GetTotalAttackBonus()
    {
        int bonus = 0;
        if (equippedWeapon != null) bonus += equippedWeapon.bonusAttackTokens;
        if (equippedShield != null) bonus += equippedShield.bonusAttackTokens;
        if (equippedHelmet != null) bonus += equippedHelmet.bonusAttackTokens;
        if (equippedArmor != null) bonus += equippedArmor.bonusAttackTokens;
        if (equippedTrinket != null) bonus += equippedTrinket.bonusAttackTokens;
        if (equippedRing != null) bonus += equippedRing.bonusAttackTokens;
        if (equippedBoots != null) bonus += equippedBoots.bonusAttackTokens;
        if (equippedGloves != null) bonus += equippedGloves.bonusAttackTokens;
        return bonus;
    }

    public int GetTotalDefenseBonus()
    {
        int bonus = 0;
        if (equippedWeapon != null) bonus += equippedWeapon.bonusDefenseTokens;
        if (equippedShield != null) bonus += equippedShield.bonusDefenseTokens;
        if (equippedHelmet != null) bonus += equippedHelmet.bonusDefenseTokens;
        if (equippedArmor != null) bonus += equippedArmor.bonusDefenseTokens;
        if (equippedTrinket != null) bonus += equippedTrinket.bonusDefenseTokens;
        if (equippedRing != null) bonus += equippedRing.bonusDefenseTokens;
        if (equippedBoots != null) bonus += equippedBoots.bonusDefenseTokens;
        if (equippedGloves != null) bonus += equippedGloves.bonusDefenseTokens;
        return bonus;
    }

    public int GetTotalMaxHPBonus()
    {
        int bonus = 0;
        if (equippedWeapon != null) bonus += equippedWeapon.bonusMaxHP;
        if (equippedShield != null) bonus += equippedShield.bonusMaxHP;
        if (equippedHelmet != null) bonus += equippedHelmet.bonusMaxHP;
        if (equippedArmor != null) bonus += equippedArmor.bonusMaxHP;
        if (equippedTrinket != null) bonus += equippedTrinket.bonusMaxHP;
        if (equippedRing != null) bonus += equippedRing.bonusMaxHP;
        if (equippedBoots != null) bonus += equippedBoots.bonusMaxHP;
        if (equippedGloves != null) bonus += equippedGloves.bonusMaxHP;
        return bonus;
    }

    public int GetTotalMaxBetBonus()
    {
        int bonus = 0;
        if (equippedWeapon != null) bonus += equippedWeapon.bonusMaxBet;
        if (equippedShield != null) bonus += equippedShield.bonusMaxBet;
        if (equippedHelmet != null) bonus += equippedHelmet.bonusMaxBet;
        if (equippedArmor != null) bonus += equippedArmor.bonusMaxBet;
        if (equippedTrinket != null) bonus += equippedTrinket.bonusMaxBet;
        if (equippedRing != null) bonus += equippedRing.bonusMaxBet;
        if (equippedBoots != null) bonus += equippedBoots.bonusMaxBet;
        if (equippedGloves != null) bonus += equippedGloves.bonusMaxBet;
        return bonus;
    }

    public float GetTotalGoldMultiplier()
    {
        float multiplier = 1f;
        if (equippedWeapon != null) multiplier += equippedWeapon.bonusGoldMultiplier;
        if (equippedShield != null) multiplier += equippedShield.bonusGoldMultiplier;
        if (equippedHelmet != null) multiplier += equippedHelmet.bonusGoldMultiplier;
        if (equippedArmor != null) multiplier += equippedArmor.bonusGoldMultiplier;
        if (equippedTrinket != null) multiplier += equippedTrinket.bonusGoldMultiplier;
        if (equippedRing != null) multiplier += equippedRing.bonusGoldMultiplier;
        if (equippedBoots != null) multiplier += equippedBoots.bonusGoldMultiplier;
        if (equippedGloves != null) multiplier += equippedGloves.bonusGoldMultiplier;
        return multiplier;
    }

    public void ConsumePotion()
    {
        equippedPotion = null;
    }
}