using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "NPC/Wave")]
public class WaveData : ScriptableObject
{
    [Header("Przeciwnicy w tej fali")]
    public List<MobData> mobsInWave;

    [Header("Ustawienia Fali")]
    public bool isFinalBossWave = false;

    [Header("Loot na koniec fali")]
    public List<ItemData> possibleLootPool;
    public int minLootItems = 1;
    public int maxLootItems = 2;
}