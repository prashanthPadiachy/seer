using System.Collections.Generic;
using UnityEngine;

public class PlatformEye : MonoBehaviour
{
    private HashSet<EyePlatform> platformsInside =
        new HashSet<EyePlatform>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EyePlatform platform =
            collision.GetComponentInParent<EyePlatform>();

        if (platform == null)
            return;

        if (platformsInside.Add(platform))
        {
            platform.AddEye(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        EyePlatform platform =
            collision.GetComponentInParent<EyePlatform>();

        if (platform == null)
            return;

        if (platformsInside.Remove(platform))
        {
            platform.RemoveEye(this);
        }
    }

    private void OnDisable()
    {
        foreach (EyePlatform platform in platformsInside)
        {
            if (platform != null)
                platform.RemoveEye(this);
        }

        platformsInside.Clear();
    }
}