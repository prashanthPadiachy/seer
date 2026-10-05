using System.Collections.Generic;
using UnityEngine;

public class EyePlatform : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D solidCollider;

    [Header("Sprites")]
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite activeSprite;

    private HashSet<PlatformEye> activeEyes = new HashSet<PlatformEye>();

    private void Start()
    {
        UpdatePlatform();
    }

    public void AddEye(PlatformEye eye)
    {
        activeEyes.Add(eye);
        UpdatePlatform();
    }

    public void RemoveEye(PlatformEye eye)
    {
        activeEyes.Remove(eye);
        UpdatePlatform();
    }

    private void UpdatePlatform()
    {
        bool isActive = activeEyes.Count > 0;

        spriteRenderer.sprite = isActive
            ? activeSprite
            : inactiveSprite;

        solidCollider.enabled = isActive;
    }
}