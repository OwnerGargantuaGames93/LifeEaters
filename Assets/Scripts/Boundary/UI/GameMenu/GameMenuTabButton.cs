using System;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuTabButton : MonoBehaviour
{
    private Image _background;

    private void Awake()
    {
        _background = GetComponent<Image>();
    }
    
    public void SetSelection(bool isSelected)
    {
        _background.color = isSelected ? new Color(_background.color.r, _background.color.g, _background.color.b, 1f) :
            new Color(_background.color.r, _background.color.g, _background.color.b, 0f);
    }
}
