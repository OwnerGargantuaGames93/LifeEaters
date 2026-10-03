using System.Collections;
using UnityEngine;

namespace Boundary.GamePlay
{
    // Shared "Super Mario World"-style death visual: bounce up, flip upside down once falling,
    // stay above everything while it plays, and finish once the target has fallen off screen.
    // Used by both PlayerController and BaseEnemy so the sequence looks identical for both.
    public static class DeathFallEffect
    {
        public static IEnumerator Play(
            Transform target,
            Rigidbody2D rb,
            SpriteRenderer[] renderers,
            float bounceForce,
            int sortingOrderBoost,
            float offscreenViewportMargin)
        {
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder += sortingOrderBoost;
            }

            rb.linearVelocity = new Vector2(0f, bounceForce);

            var flipped = false;
            var camera = UnityEngine.Camera.main;

            while (true)
            {
                if (!flipped && rb.linearVelocity.y < 0f)
                {
                    target.rotation = Quaternion.Euler(0f, 0f, 180f);
                    flipped = true;
                }

                var viewportPosition = camera.WorldToViewportPoint(target.position);
                if (viewportPosition.y < -offscreenViewportMargin)
                {
                    yield break;
                }

                yield return null;
            }
        }
    }
}
