using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MobHoverInfo : MonoBehaviour
{
    public GameObject mobInfoTextObject;

    private void Start()
    {
        if (mobInfoTextObject != null)
        {
            mobInfoTextObject.SetActive(false); // Domyœlnie ukryte
        }
    }

    private void OnMouseEnter()
    {
        if (mobInfoTextObject != null)
        {
            mobInfoTextObject.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if (mobInfoTextObject != null)
        {
            mobInfoTextObject.SetActive(false);
        }
    }
}