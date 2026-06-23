using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Boundary.Interactable
{
    public class InvisibleSpaceTrigger : MonoBehaviour
    {
        [SerializeField] private Tilemap coverTilemap;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(Constants.PlayerTag))
            {
                return;
            }

            Debug.Log("Player entered invisible space trigger.");
            
            // Here you can add logic to handle what happens when the player enters the invisible space
            coverTilemap.gameObject.SetActive(false);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(Constants.PlayerTag))
            {
                return;
            }

            Debug.Log("Player exited invisible space trigger.");
            
            // Here you can add logic to handle what happens when the player exits the invisible space
            coverTilemap.gameObject.SetActive(true);
        }
    }
}
