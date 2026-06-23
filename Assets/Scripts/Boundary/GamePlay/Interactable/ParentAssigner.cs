using UnityEngine;

namespace Boundary.Interactable
{
    public class ParentAssigner: MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private string[] excludedTags;
        
        // MARK:- Userò un boc collider trigger per capire se un gameobject è sopra alla piattaforma
        // se lo è setterò il suo parent a questo gameobject, in modo che si muova insieme alla piattaforma. Quando il gameobject esce dal trigger, resetterò il parent a null.
        // Vorrei escludere però tilemaps e background
        private void OnTriggerEnter(Collider other)
        {
            foreach (var t in excludedTags)
            {
                if (other.CompareTag(t))
                {
                    return;
                }
            }
            
            target.transform.SetParent(transform);
        }
        
        private void OnTriggerExit(Collider other)
        {
            foreach (var t in excludedTags)
            {
                if (other.CompareTag(t))
                {
                    return;
                }
            }
            
            target.transform.SetParent(null);
        }
        
    }
}