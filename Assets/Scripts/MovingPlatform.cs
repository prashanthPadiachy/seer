using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;

    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private Vector2 previousPosition;
    private Vector2 platformMovement;

    private Rigidbody2D playerRb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        targetPosition = pointB.position;
        previousPosition = rb.position;
    }

    private void FixedUpdate()
    {
        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            targetPosition,
            speed * Time.fixedDeltaTime
        );

        platformMovement = newPosition - rb.position;

        rb.MovePosition(newPosition);

        // Carry player without parenting them
        if (playerRb != null)
        {
            playerRb.position += platformMovement;
        }

        if (Vector2.Distance(newPosition, targetPosition) < 0.01f)
        {
            if (targetPosition == (Vector2)pointA.position)
                targetPosition = pointB.position;
            else
                targetPosition = pointA.position;
        }

        previousPosition = newPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        // Only carry player if they're standing on top
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                playerRb = collision.rigidbody;
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.rigidbody == playerRb)
                playerRb = null;
        }
    }
}