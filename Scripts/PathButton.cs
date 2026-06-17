using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PathButton : MonoBehaviour
{
    public Image pathIcon;
    public TMP_Text pathNameText;
    public TMP_Text pathDescriptionText; 
    public Button buttonComponent;

    
    public void Setup(PathData data)
    {
        if (pathIcon != null) pathIcon.sprite = data.pathIcon;
        if (pathNameText != null) pathNameText.text = data.pathName;
        if (pathDescriptionText != null) pathDescriptionText.text = data.pathDescription;
    }
}