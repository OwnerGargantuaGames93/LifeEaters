using Boundary.UI.Shop;
using Data.Entities.Player;
using Infra.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Boundary.UI.StatueMenu
{
    public class LevelUpMenuButton: MonoBehaviour, ISelectHandler
    {
        [Header("UI Components")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI valueLabel;
        [SerializeField] private TextMeshProUGUI modifierLabel;

        private int _choiceIndex = -1;
        
        private IEventBus _eventBus;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
        }
        
        public void SetChoiceIndex(int index)
        {
            _choiceIndex = index;
        }

        public void SelectButton()
        {
            button.Select();
        }
        
        public void OnSelect(BaseEventData eventData)
        {
            _eventBus.Publish(new EUpdateLevelUpMenuButtonIndex(_choiceIndex));
        }
        
        public void MakeUnresponsive()
        {
            button.interactable = false;
        }
        
        public void MakeResponsive()
        {
            button.interactable = true;
        }

        public void UpdateDisplayedValue(PlayerCharacteristics current, PlayerCharacteristics target)
        {
            switch (_choiceIndex)
            {
                case 0:
                {
                    titleLabel.text = "VITALITY";
                    valueLabel.text = $"{current.Vitality}";
                    modifierLabel.text = target.Vitality > current.Vitality ? $"+({target.Vitality - current.Vitality})" : "";
                    break;
                }
                case 1:
                {
                    titleLabel.text = "STRENGTH";
                    valueLabel.text = $"{current.Strength}";
                    modifierLabel.text = target.Strength > current.Strength ? $"+({target.Strength - current.Strength})" : "";
                    break;
                }
                case 2:
                {
                    titleLabel.text = "AGILITY";
                    valueLabel.text = $"{current.Agility}";
                    modifierLabel.text = target.Agility > current.Agility ? $"+({target.Agility - current.Agility})" : "";
                    break;
                }
                case 3:
                {
                    titleLabel.text = "HUMAN";
                    valueLabel.text = $"{current.Human}";
                    modifierLabel.text = target.Human > current.Human ? $"+({target.Human - current.Human})" : "";
                    break;
                }
                case 4:
                {
                    titleLabel.text = "ALIEN";
                    valueLabel.text = $"{current.Alien}";
                    modifierLabel.text = target.Alien > current.Alien ? $"+({target.Alien - current.Alien})" : "";
                    break;
                }
                case 5:
                {
                    titleLabel.text = "GOD";
                    valueLabel.text = $"{current.God}";
                    modifierLabel.text = target.God > current.God ? $"+({target.God - current.God})" : "";
                    break;
                }
                default:
                {
                    Debug.LogError("Invalid choice index");
                    break;   
                }
            }
        }
    }
}