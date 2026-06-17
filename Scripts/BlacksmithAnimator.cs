using UnityEngine;
using UnityEngine.UI; 

public class BlacksmithAnimator : MonoBehaviour
{
    [Header("Komponenty")]
    public Image backgroundImage; 
    public Sprite[] frames;

    [Header("Prêdkoœæ Animacji")]
    [Range(0.01f, 1f)]
    public float timePerFrame = 0.1f;

    [Header("DŸwiêk M³ota")]
    public AudioSource audioSource;
    public AudioClip hammerSound;
    [Tooltip("Numer klatki, na której uderza m³ot. Liczymy od 0")]
    public int strikeFrameIndex = 3;

    private int currentFrame = 0;
    private float timer = 0f;

    void Update()
    {
        if (frames == null || frames.Length == 0 || backgroundImage == null) return;

        timer += Time.deltaTime;

        if (timer >= timePerFrame)
        {
            timer -= timePerFrame;

            currentFrame = (currentFrame + 1) % frames.Length;
            backgroundImage.sprite = frames[currentFrame];

           // Debug.Log("Zmieni³em klatkê na: " + currentFrame + ". Czekam na uderzenie w klatce: " + strikeFrameIndex);

            if (currentFrame == strikeFrameIndex)
            {
                PlayHammerSound();
            }
        }
    }

    private void PlayHammerSound()
    {
        //Debug.Log("BUM! Próbujê zagraæ dŸwiêk kowad³a!");
        if (audioSource != null && hammerSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(hammerSound);
        }
    }
}