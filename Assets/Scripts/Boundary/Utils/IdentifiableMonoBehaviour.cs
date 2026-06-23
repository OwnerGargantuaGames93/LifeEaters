using UnityEngine;

namespace Boundary.Utils
{

    public class IdentifiableMonoBehaviour : MonoBehaviour
    {
        [Header("Unique Identifier")]
        private string _id;
        
        public string Id => _id;

        protected void CommonAwake()
        {
            SetId();
        }

        private void SetId()
        {
            // name + position
            var worldPosition = transform.position;
            _id = $"{gameObject.name}_{worldPosition.x}_{worldPosition.y}";   
        }
    }
}