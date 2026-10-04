using System.Collections;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private ParticleSystem deathParticles;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 0.5f;
    [SerializeField] private GameObject playerVisuals;

    private Rigidbody2D rb;
    private PlayerMovement playerMovement;

    private bool isRespawning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Hazard"))
        {
            StartCoroutine(Respawn());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hazard"))
        {
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator Respawn()
    {
        if (isRespawning)
            yield break;

        isRespawning = true;

        // Stop player movement
        rb.linearVelocity = Vector2.zero;
        playerMovement.enabled = false;

        // Play particle effect where player died
        if (deathParticles != null)
        {
            Instantiate(
                deathParticles,
                transform.position,
                Quaternion.identity
            );
        }

        // Hide player
        if (playerVisuals != null)
        {
            playerVisuals.SetActive(false);
        }

        // Wait before respawning
        yield return new WaitForSeconds(respawnDelay);

        // Reset position and gravity
        transform.position = respawnPoint.position;
        playerMovement.SetGravity(Vector2.down);

        rb.linearVelocity = Vector2.zero;

        // Show player again
        if (playerVisuals != null)
        {
            playerVisuals.SetActive(true);
        }

        playerMovement.enabled = true;

        isRespawning = false;
    }
}