using System.Collections;
using Boundary.Interactable;
using Boundary.UI.StatueMenu;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.UI
{
    public class StatueMenuHandler: MonoBehaviour, IStatueMenuHandler
    {
        private IEventBus _eventBus;

        private bool _isStatueMenuOpen;
        
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EPlayerRestOnStatue>(OnStatueMenuOpened);
            _eventBus.Subscribe<EStatueMenuClosed>(OnStatueMenuClosed);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EPlayerRestOnStatue>(OnStatueMenuOpened);
            _eventBus.Unsubscribe<EStatueMenuClosed>(OnStatueMenuClosed);
        }
        
        public bool IsStatueMenuOpen()
        {
            return _isStatueMenuOpen;
        }

        private void OnStatueMenuOpened(EPlayerRestOnStatue ePlayerRestOnStatue)
        {
            _isStatueMenuOpen = true;
        }
        
        private void OnStatueMenuClosed(EStatueMenuClosed eStatueMenuClosed)
        {
            StartCoroutine(ExitMenu());
        }

        private IEnumerator ExitMenu()
        {
            yield return null;
            
            _isStatueMenuOpen = false;
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
    }
}