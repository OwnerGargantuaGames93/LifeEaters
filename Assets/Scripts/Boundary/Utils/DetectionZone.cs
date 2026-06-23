using System.Collections.Generic;
using UnityEngine;

namespace Boundary.Utils
{
    public class DetectionZone : MonoBehaviour
    {   
        // Indicates which colliders are currently detected by the zone
        private readonly List<Collider2D> _detectedColliders = new();


        // Create a getter for the detected colliders
        public List<Collider2D> DetectedColliders
        {
            get
            {
                // Filter out colliders destroyed objects
                _detectedColliders.RemoveAll(coll => coll == null || coll.gameObject == null);
                return _detectedColliders;
            }
        }

        // IMPORTANT: At the first frame iteration, the fixed update will not detect any collider
        // even if they are inside the zone.
        // So, if we want to use some detection check inside a FixedUpdate we need to tell
        // when we can archive that using this variable.
        // This variable will be set to true at the first FixedUpdate iteration,
        // in order to tell outside when it is ready to use.
        public bool readyForFixedUpdate;

        private Collider2D _col;

        private void Start()
        {
            _col = GetComponent<Collider2D>();
        }

        private void FixedUpdate() {
            if (!readyForFixedUpdate) {
                readyForFixedUpdate = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            _detectedColliders.Add(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _detectedColliders.Remove(other);
        }
    }
}
