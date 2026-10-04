using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer flagSprite;
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Transform respawnPoint;

    private bool activated;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated)
            return;

        PlayerRespawn playerRespawn =
            collision.GetComponentInParent<PlayerRespawn>();

        if (playerRespawn == null)
            return;

        activated = true;

        // Set this checkpoint as the new respawn point
        playerRespawn.SetCheckpoint(respawnPoint.position);

        // Change flag appearance
        if (flagSprite != null && activeSprite != null)
        {
            flagSprite.sprite = activeSprite;
        }
    }
}