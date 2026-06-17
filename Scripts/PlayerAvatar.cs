using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAvatar : MonoBehaviour
{
    public PlayerClassData currentClass;
    public Image avatarImage;

    [Header("Ustawienia Animacji")]
    [Range(0.05f, 0.5f)]
    public float blinkDuration = 0.15f;
    [Range(1f, 5f)]
    public float minTimeBetweenBlinks = 2f;
    [Range(2f, 10f)]
    public float maxTimeBetweenBlinks = 5f;

    private bool isDead = false;
    public bool isHurt = false; 
    private bool isWounded = false;

    void OnEnable()
    {
        if (avatarImage == null) avatarImage = GetComponent<Image>();

        // Na wszelki wypadek czyœcimy stare pêtle, gdyby obiekt by³ restartowany gwa³townie
        StopAllCoroutines();
        isHurt = false;

        if (currentClass != null && !isDead)
        {
            avatarImage.sprite = isWounded ? currentClass.woundedIdleSprite : currentClass.idleSprite;
        }

        if (!isDead)
        {
            StartCoroutine(BlinkRoutine());
        }
    }

    public void UpdateAvatarState(int currentHP, int maxHP)
    {
        if (currentClass == null) return;

        if (currentHP <= 0)
        {
            isDead = true;
            avatarImage.sprite = currentClass.deadSprite;
            StopAllCoroutines();
            return;
        }

        
        if (isDead)
        {
            isDead = false;
            StartCoroutine(BlinkRoutine()); 
        }

        isWounded = (currentHP <= maxHP / 2);

        if (!isHurt)
        {
            avatarImage.sprite = isWounded ? currentClass.woundedIdleSprite : currentClass.idleSprite;
        }
    }

    public void PlayHurtAnimation()
    {
        if (isDead) return;
        Debug.Log("AUÆ! Awatar otrzyma³ polecenie animacji obra¿eñ.");
        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        isHurt = true;
        avatarImage.sprite = isWounded ? currentClass.woundedHurtSprite : currentClass.hurtSprite;

        yield return new WaitForSeconds(0.3f);

        if (currentClass == null) yield break;
        isHurt = false;
        avatarImage.sprite = isWounded ? currentClass.woundedIdleSprite : currentClass.idleSprite;
    }

    private IEnumerator BlinkRoutine()
    {
        while (!isDead)
        {
            float waitTime = Random.Range(minTimeBetweenBlinks, maxTimeBetweenBlinks);
            yield return new WaitForSeconds(waitTime);

            if (currentClass == null) continue;

            if (!isHurt)
            {
                avatarImage.sprite = isWounded ? currentClass.woundedBlinkSprite : currentClass.blinkSprite;

                
                yield return new WaitForSeconds(blinkDuration);

                if (!isHurt)
                {
                    avatarImage.sprite = isWounded ? currentClass.woundedIdleSprite : currentClass.idleSprite;
                }
            }
        }
    }
}