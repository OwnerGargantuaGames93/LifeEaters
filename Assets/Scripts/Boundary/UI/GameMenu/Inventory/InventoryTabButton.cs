using UnityEngine;
using UnityEngine.UI;

public class InventoryTabButton : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Button button;

    public void SetSelection(bool isSelected)
    {
        if (isSelected)
        {
            button.image.material.SetFloat("_OutlineWidth", 1f);
            button.image.material.SetColor("_OutlineColor", Color.white);
        }
        else
        {
            button.image.material.SetFloat("_OutlineWidth", 0f);
            button.image.material.SetColor("_OutlineColor", Color.clear);
        }
    }
    
}
