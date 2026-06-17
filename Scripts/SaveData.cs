using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SavedCardUpgrade
{
    public string cardName;
    public List<string> upgradeNames = new List<string>();
}

[System.Serializable]
public class SavedNodeData
{
    public int nodeID;
    public string contentName;
    public bool isCleared;
}

[System.Serializable]
public class SaveData
{
    public int savedPlayerHP;
    public int savedPlayerMaxHP;
    public int savedPlayerArmor;
    public int savedTokensAttack;
    public int savedTokensDefense;
    public int savedPlayerCoins;
    public PlayerClassType savedClassType;

    public int savedMobIndex;
    public string savedWaveName;

    public string savedWeaponName = "";
    public string savedShieldName = "";
    public string savedHelmetName = "";
    public string savedArmorName = "";
    public string savedTrinketName = "";
    public string savedRing;
    public string savedPotion;
    public string savedBootsName = "";
    public string savedGlovesName = ""; // DODANE: Zapis Rêkawic

    public string activeTavernBuff = ""; // Karczma

    public List<string> savedBackpack = new List<string>();
    public List<string> savedSmallBackpack = new List<string>();

    public int currentMapLevel = 0;
    public string targetWaveName = "";

    public List<SavedCardUpgrade> upgradedCards = new List<SavedCardUpgrade>();
    public int currentNodeID = 0;
    public List<SavedNodeData> mapNodes = new List<SavedNodeData>();
    public string currentMapTheme = "";
    public int savedDummyHP;
    public int savedDummyArmor;
}