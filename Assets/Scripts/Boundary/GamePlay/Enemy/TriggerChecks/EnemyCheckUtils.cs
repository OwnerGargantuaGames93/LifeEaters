using System.Collections.Generic;
using Boundary.GamePlay.Enemy.Base;
using UnityEngine;
using Utils;

namespace Boundary.GamePlay.Enemy.TriggerChecks
{
    public static class EnemyCheckUtils
    {
        public static bool EnemyIsWatchingPlayer(BaseEnemy enemy, GameObject playerTarget, bool considerObstacles)
        { 
            // Player direction
            Vector2 playerDirection = (playerTarget.transform.position - enemy.eyes.transform.position).normalized;

            // Ray cast from enemy eyes to player
            var raycastHit2Ds = new List<RaycastHit2D>();
            var hitsCount = Physics2D.Raycast(enemy.eyes.transform.position, playerDirection, ContactFilter2D.noFilter, raycastHit2Ds, Mathf.Infinity);

            // TODO: Add only in debug mode
            Debug.DrawRay(enemy.eyes.transform.position, playerDirection, Color.red);

            // Debug.Log("/////////////////////////////");
            // for (int i = 0; i < hitsCount; i++) {
            //   RaycastHit2D hit = raycastHit2Ds[i];
            //   // Debug.Log(hit.collider.gameObject.name);
            //   // Debug.Log(hit.collider.gameObject.layer);
            //   // Debug.Log(hit.collider.gameObject.tag);
            // }
            // Debug.Log("/////////////////////////////");

            // If in the ray cast exists a player...
            if (hitsCount <= 0 || !raycastHit2Ds.Exists(hit => hit.collider.CompareTag(Constants.PlayerTag)))
            {
                return false;
            }
            
            if (!considerObstacles)
            {
                return true;
            }
            
            var groundIndex = raycastHit2Ds.FindIndex(hit => hit.collider.gameObject.layer == Constants.GroundLayerNumber);
            var wallIndex = raycastHit2Ds.FindIndex(hit => hit.collider.gameObject.layer == Constants.WallLayerNumber);
            var playerIndex = raycastHit2Ds.FindIndex(hit => hit.collider.CompareTag(Constants.PlayerTag));

            if (groundIndex < 0) {
                groundIndex = int.MaxValue;
            }

            if (wallIndex < 0) {
                wallIndex = int.MaxValue;
            }

            // If the player is in front of the enemy and there are no obstacles between them
            return playerIndex < groundIndex && playerIndex < wallIndex;
        }
    }
}