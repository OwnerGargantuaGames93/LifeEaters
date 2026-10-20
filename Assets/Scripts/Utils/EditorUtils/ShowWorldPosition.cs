using UnityEngine;

namespace Utils.EditorUtils
{
    /// <summary>
    /// Debug helper: shows the GameObject's world position in the Inspector (read-only, for reference)
    /// and as a label in the Scene view when selected. Useful for children, whose Transform shows local values.
    /// </summary>
    public class ShowWorldPosition : MonoBehaviour
    {
        [SerializeField] private Vector3 worldPosition;

        private void OnValidate() => worldPosition = transform.position;

        private void OnDrawGizmosSelected()
        {
            worldPosition = transform.position;
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position, $"{(Vector2)transform.position}");
#endif
        }
    }
}
