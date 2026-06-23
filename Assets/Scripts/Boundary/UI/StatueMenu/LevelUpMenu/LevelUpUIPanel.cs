using System.Collections.Generic;
using Boundary.Commands;
using Boundary.UI.StatueMenu;
using Control.Player;
using Control.Player.UseCase;
using Data.Entities.Player;
using Infra.EventBus;
using TMPro;
using UnityEngine;

namespace Boundary.UI.Shop
{
    public class LevelUpUIPanel: MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject contentPanel;
        // 0 -> Vitality, 1 -> Strength, 2 -> Agility, 3 -> Human, 4 -> Alien, 5 -> God
        [SerializeField] private List<LevelUpMenuButton> levelUpMenuButtons = new List<LevelUpMenuButton>();
        [SerializeField] private TextMeshProUGUI attributesSummaryLabel;
        [SerializeField] private TextMeshProUGUI pointsSummaryLabel;
        
        private int _currentSelectedButtonIndex = -1;
        
        private PlayerCharacteristics _targetCharacteristics;

        private float _currentPoints;
        private int _pointsToBeUsed;
        private int _pointForNextLevel;

        private bool _isMenuOpen;
        
        private IEventBus _eventBus;
        private IPlayerModel _player;
        
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _player = GameContext.Instance.Player;
            
            contentPanel.SetActive(false);
            _isMenuOpen = false;
            
            var index = 0;
            foreach (var levelUpMenuButton in levelUpMenuButtons)
            {
                levelUpMenuButton.SetChoiceIndex(index);
                index++;
            }
        }
        
        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
            ResetPanel();
        }
        
        private void OnOpen(EOpenLevelUpMenu e)
        {
            contentPanel.SetActive(true);
            _isMenuOpen = true;
            
            _targetCharacteristics = _player.Characteristics;
            _currentPoints = _player.Status.points;
            _pointsToBeUsed = 0;
            _pointForNextLevel = ULevelUp.GetNextLevelPoints(_targetCharacteristics);
            
            // Select the first button by default
            if (levelUpMenuButtons.Count <= 0)
            {
                Debug.LogError("LevelUpUIPanel: No level up menu buttons found!");
                return;
            }

            foreach (var button in levelUpMenuButtons)
            {
                button.MakeResponsive();
            }
            
            levelUpMenuButtons[0].SelectButton();
            _currentSelectedButtonIndex = 0;

            RefreshUI();
        }

        private void Update()
        {
            if (!_isMenuOpen) return;
            
            if (UserInput.instance.StatueMenuGoBackWasPressedThisFrame())
            {
                CloseMenu();
            }

            if (UserInput.instance.StatueMenuIncreaseValueWasPressedThisFrame())
            {
                IncreaseTargetCharacteristicsIfPossible();
            }
            
            if (UserInput.instance.StatueMenuDecreaseValueWasPressedThisFrame())
            {
                DecreaseTargetCharacteristicsIfPossible();
            }

            if (UserInput.instance.StatueMenuConfirmWasPressedThisFrame())
            {
                ConfirmLevelUp();
            }
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EOpenLevelUpMenu>(OnOpen);
            _eventBus.Subscribe<ELevelUp>(OnLevelUp);
            _eventBus.Subscribe<EUpdateLevelUpMenuButtonIndex>(OnUpdateLevelUpMenuButtonIndex);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EUpdateLevelUpMenuButtonIndex>(OnUpdateLevelUpMenuButtonIndex);
            _eventBus.Unsubscribe<ELevelUp>(OnLevelUp);
            _eventBus.Unsubscribe<EOpenLevelUpMenu>(OnOpen);
        }

        private void OnUpdateLevelUpMenuButtonIndex(EUpdateLevelUpMenuButtonIndex index)
        {
            _currentSelectedButtonIndex = index.ButtonIndex;
        }

        private void ResetPanel()
        {
            _currentSelectedButtonIndex = -1;
            _targetCharacteristics = _player.Characteristics;
            
            _pointsToBeUsed = 0;
            _pointForNextLevel = 0;
            
            foreach (var button in levelUpMenuButtons)
            {
                button.MakeUnresponsive();
            }
            
            attributesSummaryLabel.text = "";
            pointsSummaryLabel.text = "";
        }

        private void CloseMenu()
        {
            contentPanel.SetActive(false);
            _isMenuOpen = false;

            ResetPanel();
            
            _eventBus.Publish(new EStatueSubMenuClosed());
        }
        
        private void OnLevelUp(ELevelUp e)
        {
            CloseMenu();
        }

        private void IncreaseTargetCharacteristicsIfPossible()
        {
            if (_targetCharacteristics == null)
            {
                Debug.LogWarning("LevelUpUIPanel: Target characteristics is null!");
                return;
            }
            
            // Check if we have enough points to increase the selected characteristic
            if (_currentPoints < _pointsToBeUsed + _pointForNextLevel)
            {
                Debug.Log("LevelUpUIPanel: Not enough points to increase characteristic!");
                return;
            }
            
            // Increase the selected characteristic
            var characteristicModifier = PlayerCharacteristics.Identity();
            switch (_currentSelectedButtonIndex)
            {
                case 0:
                    characteristicModifier.Vitality = 1;
                    break;
                case 1:
                    characteristicModifier.Strength = 1;
                    break;
                case 2:
                    characteristicModifier.Agility = 1;
                    break;
                case 3:
                    characteristicModifier.Human = 1;
                    break;
                case 4:
                    characteristicModifier.Alien = 1;
                    break;
                case 5:
                    characteristicModifier.God = 1;
                    break;
                default:
                    Debug.LogError("LevelUpUIPanel: Invalid button index!");
                    break;
            }
            
            _targetCharacteristics += characteristicModifier;
            
            _pointsToBeUsed += _pointForNextLevel;
            _pointForNextLevel = ULevelUp.GetNextLevelPoints(_targetCharacteristics);

            RefreshUI();
        }

        private void DecreaseTargetCharacteristicsIfPossible()
        {
            if (_targetCharacteristics == null || _targetCharacteristics.Equals(_player.Characteristics))
            {
                Debug.LogWarning("LevelUpUIPanel: Target characteristics is null or equal to current characteristics!");
                return;
            }
            
            // Check if we can decrease the selected characteristic
            var characteristicModifier = PlayerCharacteristics.Identity();
            switch (_currentSelectedButtonIndex)
            {
                case 0:
                    if (_targetCharacteristics.Vitality - _player.Characteristics.Vitality > 0)
                    {
                        characteristicModifier.Vitality = -1;
                    }
                    break;
                case 1:
                    if (_targetCharacteristics.Strength - _player.Characteristics.Strength > 0)
                    {
                        characteristicModifier.Strength = -1;    
                    }
                    break;
                case 2:
                    if (_targetCharacteristics.Agility - _player.Characteristics.Agility > 0)
                    {
                        characteristicModifier.Agility = -1;    
                    }
                    break;
                case 3:
                    if (_targetCharacteristics.Human - _player.Characteristics.Human > 0)
                    {
                        characteristicModifier.Human = -1;    
                    }
                    break;
                case 4:
                    if (_targetCharacteristics.Alien - _player.Characteristics.Alien > 0)
                    {
                        characteristicModifier.Alien = -1;    
                    }
                    break;
                case 5:
                    if (_targetCharacteristics.God - _player.Characteristics.God > 0)
                    {
                        characteristicModifier.God = -1;    
                    }
                    break;
                default:
                    Debug.LogWarning("LevelUpUIPanel: Invalid button index!");
                    return;
            }

            if (characteristicModifier.GetLevelPoints() >= 0)
            {
                return;
            }
            
            _targetCharacteristics += characteristicModifier;
            _pointsToBeUsed -= _pointForNextLevel;
            _pointForNextLevel = ULevelUp.GetNextLevelPoints(_targetCharacteristics);

            RefreshUI();
        }

        private void ConfirmLevelUp()
        {
            if (_targetCharacteristics == null || _targetCharacteristics.Equals(_player.Characteristics))
            {
                Debug.LogWarning("LevelUpUIPanel: Target characteristics is null or equal to current characteristics!");
                return;
            }
            
            _eventBus.Publish(new ELevelUpUiCommand(_targetCharacteristics, _pointsToBeUsed));
        }

        private void RefreshUI()
        {
            // TODO: Update attributes summary label
            
            // Update points summary label
            var pointUsedText = _pointsToBeUsed > 0 ? $"(-{_pointsToBeUsed})" : "";
            var currentLevel = _player.Characteristics.GetLevelPoints();
            var targetLevel = _targetCharacteristics.GetLevelPoints();
            pointsSummaryLabel.text = $"Current Points: {_currentPoints} {pointUsedText}\n" +
                                      $"Points For Next Level: {_pointForNextLevel}\n" +
                                      $"Level: {currentLevel} -> {targetLevel}";
            
            // Update each button's displayed value
            foreach (var button in levelUpMenuButtons)
            {
                button.UpdateDisplayedValue(_player.Characteristics, _targetCharacteristics);
            }
        }
    }
    
    #region Events

    public struct EUpdateLevelUpMenuButtonIndex
    {
        public readonly int ButtonIndex;

        public EUpdateLevelUpMenuButtonIndex(int buttonIndex)
        {
            ButtonIndex = buttonIndex;
        }
    }

    /// <summary>
    /// Notify that the player wants to level up with the given characteristics increase.
    /// </summary>
    public struct ELevelUpUiCommand
    {
        public readonly PlayerCharacteristics NewCharacteristics;
        public readonly int PointsToUse;
        
        public ELevelUpUiCommand(PlayerCharacteristics newCharacteristics, int pointsToUse)
        {
            NewCharacteristics = newCharacteristics;
            PointsToUse = pointsToUse;
        }
    }

    #endregion
}