using System.Collections.Generic;
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
    [SerializeField] private float exitDelay = 0.1f;

    private Coroutine exitCoroutine;

    private HashSet<Collider2D> playerCollidersInside = new HashSet<Collider2D>();

    private PlayerMovement currentPlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMovement player =
            collision.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        playerCollidersInside.Add(collision);
        currentPlayer = player;

        if (exitCoroutine != null)
        {
            StopCoroutine(exitCoroutine);
            exitCoroutine = null;
        }

        player.SetGravity(GetGravityDirection(), transform.position);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerMovement player =
            collision.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        playerCollidersInside.Remove(collision);

        if (playerCollidersInside.Count == 0)
        {
            exitCoroutine = StartCoroutine(ResetGravityAfterDelay(player));
        }
    }

    private System.Collections.IEnumerator ResetGravityAfterDelay(
    PlayerMovement player)
    {
        yield return new WaitForSeconds(0.1f);

        if (playerCollidersInside.Count == 0)
        {
            player.SetGravity(Vector2.down, transform.position);
            currentPlayer = null;
        }

        exitCoroutine = null;
    }
    private Vector2 GetGravityDirection()
    {
        return gravityDirection switch
        {
            EyeGravityDirection.Down => Vector2.down,
            EyeGravityDirection.Up => Vector2.up,
            EyeGravityDirection.Left => Vector2.left,
            EyeGravityDirection.Right => Vector2.right,
            _ => Vector2.down
        };
    }
}