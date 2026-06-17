using UnityEngine;

[CreateAssetMenu(fileName = "NewMapTheme", menuName = "Environment/Map Theme")]
public class MapThemeData : ScriptableObject
{
    public string themeID;               
    public Sprite staticBackground;     
    public RuntimeAnimatorController themeAnimator; 
    public float transitionDuration = 2f;
}