using System.Collections;
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

    private HashSet<Collider2D> playerCollidersInside =
        new HashSet<Collider2D>();

    private PlayerMovement currentPlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMovement player =
            collision.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        playerCollidersInside.Add(collision);
        currentPlayer = player;

        // Cancel pending exit if player came back in
        if (exitCoroutine != null)
        {
            StopCoroutine(exitCoroutine);
            exitCoroutine = null;
        }

        // Tell the player THIS eye now owns gravity
        player.SetGravityFromEye(
            this,
            GetGravityDirection(),
            transform.position
        );
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerMovement player =
            collision.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        playerCollidersInside.Remove(collision);

        // Only start leaving once every player collider is out
        if (playerCollidersInside.Count == 0)
        {
            exitCoroutine =
                StartCoroutine(ResetGravityAfterDelay(player));
        }
    }

    private IEnumerator ResetGravityAfterDelay(PlayerMovement player)
    {
        yield return new WaitForSeconds(exitDelay);

        if (playerCollidersInside.Count == 0)
        {
            // Only resets gravity if THIS eye is still
            // the player's currently active gravity eye
            player.ExitGravityEye(
                this,
                transform.position
            );

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

    private void OnDisable()
    {
        if (exitCoroutine != null)
        {
            StopCoroutine(exitCoroutine);
            exitCoroutine = null;
        }

        if (currentPlayer != null)
        {
            currentPlayer.ExitGravityEye(
                this,
                transform.position
            );
        }

        playerCollidersInside.Clear();
        currentPlayer = null;
    }
}