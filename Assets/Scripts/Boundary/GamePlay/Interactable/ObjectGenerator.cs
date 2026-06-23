using UnityEngine;

namespace Boundary.Interactable
{
    public class ObjectGenerator : MonoBehaviour
    {
        [SerializeField] public GameObject objectToSpawn;

        private GameObject _objectInstance;
    
        private void Update()
        {
            SpawnObject();
        }

        private void SpawnObject()
        {
            if (!_objectInstance)
            {
                _objectInstance = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
            }
        }

        private void OnDestroy()
        {
            Destroy(_objectInstance);
            _objectInstance = null;
        }
    }
}
