using System;
using System.Collections.Generic;
using Boundary.Commands;
using UnityEngine;
using Utils;

namespace Boundary.Interactable.Buttons
{
    [Serializable]
    public class SewersPlatformAction
    {
        public SewersPuzzlePlatform platform;
        /// <summary>+1 = sale di un livello, -1 = scende di un livello, 0 = non si muove</summary>
        public int delta = 1;
    }

    public class SewersPlatformPuzzleButton : MonoBehaviour
    {
        [SerializeField] private List<SewersPlatformAction> platformActions;

        private bool _playerCanPressButton;

        private void Update()
        {
            if (!_playerCanPressButton) return;
            if (AnyPlatformIsMoving()) return;
            if (UserInput.instance.GeneralInteractWasPressedThisFrame())
                PressButton();
        }

        private void PressButton()
        {
            foreach (var action in platformActions)
            {
                if (action.platform != null && action.delta != 0)
                    action.platform.TryMove(action.delta);
            }
        }

        private bool AnyPlatformIsMoving()
        {
            foreach (var action in platformActions)
            {
                if (action.platform != null && action.platform.IsMoving())
                    return true;
            }
            return false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag)) _playerCanPressButton = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag)) _playerCanPressButton = false;
        }
    }
}
