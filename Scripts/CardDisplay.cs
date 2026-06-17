using UnityEngine;
using System.Collections;

public class CardDisplay : MonoBehaviour
{
    public SpriteRenderer CardRender;
    public Card cardValue;
    public Sprite cardBack;

    public void DisplayCard(Card drawedCard)
    {
        CardRender.sprite = drawedCard.graphic;
        cardValue = drawedCard;
    }

    public void SetFaceUp(bool isFaceUp)
    {
        if (isFaceUp)
        {
            StartCoroutine(FlipCardAnimation(isFaceUp));
        }
        else
        {
            CardRender.sprite = cardBack;
        }
    }

    private IEnumerator FlipCardAnimation(bool isFaceUp)
    {
        float duration = 0.2f;
        float timeElapsed = 0f;

        Vector3 normalScale = new Vector3(1f, 1f, 1f);
        Vector3 flatScale = new Vector3(0f, 1f, 1f);

        while (timeElapsed < duration)
        {
            transform.localScale = Vector3.Lerp(normalScale, flatScale, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = flatScale;

        if (isFaceUp)
        {
            CardRender.sprite = cardValue.graphic;
        }
        else
        {
            CardRender.sprite = cardBack;
        }

        timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            transform.localScale = Vector3.Lerp(flatScale, normalScale, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = normalScale;
    }
}