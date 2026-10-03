using UnityEngine;

public class GravityEye : MonoBehaviour
{
    public enum EyeGravityDirection
    {
        Down,
        Up,
        Left,
        Right
    }

    [SerializeField] private EyeGravityDirection gravityDirection;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMovement player = collision.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        Vector2 direction = gravityDirection switch
        {
            EyeGravityDirection.Down => Vector2.down,
            EyeGravityDirection.Up => Vector2.up,
            EyeGravityDirection.Left => Vector2.left,
            EyeGravityDirection.Right => Vector2.right,

            _ => Vector2.down
        };

        player.SetGravity(direction);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerMovement player =
            collision.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        player.SetGravity(Vector2.down);
    }
}