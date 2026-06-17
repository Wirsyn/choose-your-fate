using UnityEngine;

[CreateAssetMenu(fileName = "NewPath", menuName = "NPC/PathData")]
public class PathData : ScriptableObject
{
    public string pathName;
    public Sprite pathIcon;
    [TextArea]
    public string pathDescription;

    // DODANE: Tavern na koñcu listy
    public enum PathType { Combat, Blacksmith, Mystery, Tavern }
    public PathType typeOfPath;

    public WaveData waveToLoad;
}