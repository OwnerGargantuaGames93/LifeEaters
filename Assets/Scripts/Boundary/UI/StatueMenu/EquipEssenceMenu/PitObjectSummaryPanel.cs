using Data.Entities.Item;
using TMPro;
using UnityEngine;

namespace Boundary.UI.StatueMenu
{
    public class PitObjectSummaryPanel: MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI label;

        public void SetData(PitObjectData data)
        {
            label.text = data.Name;
        }
    }
}