using UnityEngine;

public class EyeLookAtPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform innerEye;

    [Header("Movement")]
    [SerializeField] private float maxMoveDistanceX = 0.15f;
    [SerializeField] private float maxMoveDistanceY = 0.08f;
    [SerializeField] private float moveSpeed = 8f;

    [Header("Detection")]
    [SerializeField] private float lookDistance = 6f;

    private Vector3 startLocalPosition;

    private void Start()
    {
        startLocalPosition = innerEye.localPosition;
    }

    private void Update()
    {
        if (player == null || innerEye == null)
            return;

        float distanceToPlayer =
            Vector2.Distance(transform.position, player.position);

        Vector3 targetPosition = startLocalPosition;

        // Only follow player when they're close enough
        if (distanceToPlayer <= lookDistance)
        {
            Vector2 worldDirection =
                (player.position - transform.position).normalized;

            Vector2 localDirection =
                transform.InverseTransformDirection(worldDirection);

            targetPosition = startLocalPosition + new Vector3(
                localDirection.x * maxMoveDistanceX,
                localDirection.y * maxMoveDistanceY,
                0f
            );
        }

        // Follow player OR smoothly return to centre
        innerEye.localPosition = Vector3.Lerp(
            innerEye.localPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }
}