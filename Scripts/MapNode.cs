using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    [Header("Ustawienia Wêz³a")]
    public int nodeID;           
    public int nodeLevel;        

    [Header("Œcie¿ki Prowadz¹ce W Górê")]
    public List<MapNode> connectedNodes; 

    [Header("UI Wêz³a")]
    public Button nodeButton;
    public Image iconImage;

    [HideInInspector] public PathData assignedPath;
}