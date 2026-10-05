using System.Collections;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private Transform startingRespawnPoint;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private CameraController cameraController;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 0.5f;
    [SerializeField] private GameObject playerVisuals;

    private Rigidbody2D rb;
    private PlayerMovement playerMovement;

    private bool isRespawning;
    private Vector3 respawnPosition;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathSound;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        respawnPosition = startingRespawnPoint.position;
    }

    public void SetCheckpoint(Vector3 newPosition)
    {
        respawnPosition = newPosition;
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

        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Stop player movement
        rb.linearVelocity = Vector2.zero;
        playerMovement.enabled = false;

        if (deathParticles != null)
        {
            ParticleSystem particles = Instantiate(
                deathParticles,
                transform.position,
                Quaternion.identity
            );

            Destroy(
                particles.gameObject,
                particles.main.duration + particles.main.startLifetime.constantMax
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
        transform.position = respawnPosition;
        playerMovement.SetGravity(Vector2.down, transform.position);

        rb.linearVelocity = Vector2.zero;

        cameraController.SnapToTarget();
        // Show player again
        if (playerVisuals != null)
        {
            playerVisuals.SetActive(true);
        }

        playerMovement.enabled = true;

        isRespawning = false;
    }
}