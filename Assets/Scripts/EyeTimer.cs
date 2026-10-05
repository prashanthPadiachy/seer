using System.Collections;
using UnityEngine;

public class EyeTimer : MonoBehaviour
{
    [Header("Eye")]
    [SerializeField] private SpriteRenderer eyeSprite;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closedSprite;

    [Header("Eye Parts")]
    [SerializeField] private GameObject innerEye;
    [SerializeField] private GameObject zone;

    [Header("Timing")]
    [SerializeField] private float activeTime = 3f;
    [SerializeField] private float inactiveTime = 2f;
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private bool startActive = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip toggleSound;

    private bool isActive;

    private IEnumerator Start()
    {
        SetEyeActive(startActive);

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        while (true)
        {
            if (isActive)
            {
                yield return new WaitForSeconds(activeTime);
                SetEyeActive(false);
            }
            else
            {
                yield return new WaitForSeconds(inactiveTime);
                SetEyeActive(true);
            }
        }
    }

    private void SetEyeActive(bool active)
    {
        isActive = active;

        // Play sound when opening or closing
        if (audioSource != null && toggleSound != null)
        {
            audioSource.PlayOneShot(toggleSound);
        }

        // Change sprite
        eyeSprite.sprite = active ? openSprite : closedSprite;

        // Hide pupil/inner eye when closed
        if (innerEye != null)
            innerEye.SetActive(active);

        // Disable the actual eye effect
        if (zone != null)
            zone.SetActive(active);
    }
}