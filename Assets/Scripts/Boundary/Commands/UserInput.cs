using Boundary.UI.GameMenu;
using Boundary.UI.StatueMenu;
using Control.DialogueHandler;
using Control.Shop;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.Commands
{
    public class UserInput : MonoBehaviour
    {
        private IEventBus _eventBus;
        private IDialogueHandler _dialogueHandler;
        private IStatueMenuHandler _statueMenuHandler;
        private IGameMenuHandler _gameMenuHandler;
        private IShopModel _shopModel;
        
        public static UserInput instance;

        [HideInInspector] public Controls Controls;
        [HideInInspector] public Vector2 moveInput;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _dialogueHandler = GameContext.Instance.DialogueHandler;
            _statueMenuHandler = GameContext.Instance.StatueMenuHandler;
            _shopModel = GameContext.Instance.ShopModel;
            _gameMenuHandler = GameContext.Instance.GameMenuHandler;
            
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            } else
            {
                Destroy(gameObject);
            }

            Controls = new Controls();
            
            Controls.Movement.Move.performed += ctx => moveInput = !IsSomeMenuOpen() ? ctx.ReadValue<Vector2>() : Vector2.zero;
        }

        private void OnEnable()
        {
            Controls.Enable();
        }

        private void OnDisable()
        {
            Controls.Disable();
        }
        
        private void Update()
        {
            HandlePitChoice();
            HandleDialogueInput();
            HandleShopInput();
            HandleGameMenuInput();
        }
        
        #region Game Menu
        
        private bool IsSomeMenuOpen()
        {
            // This is not the best way to check if dialogue is playing, but it works for now.
            // Anyway, maybe a refactor on the various type of input at the end will be needed.
            var dialogueMenuIsOpen = _dialogueHandler.IsDialoguePlaying();
            var shopMenuIsOpen = _shopModel.IsShopOpen();
            var gameMenuIsOpen = _gameMenuHandler.IsGameMenuOpen();
            var statueMenuIsOpen = _statueMenuHandler.IsStatueMenuOpen();
            
            return gameMenuIsOpen || statueMenuIsOpen || dialogueMenuIsOpen || shopMenuIsOpen;
        }

        #endregion
        
        private void HandlePitChoice()
        {
            // Handle Pit Object Command
            if (Controls.Pits.ChooseNext.WasPressedThisFrame())
            {
                _eventBus.Publish(new ENextPitObjectCommandExecuted());
            }
            
            if (Controls.Pits.ChoosePrev.WasPressedThisFrame())
            {
                _eventBus.Publish(new EPreviousPitObjectCommandExecuted());
            }
        }

        private void HandleDialogueInput()
        {
            if (_dialogueHandler.IsDialoguePlaying() && !_shopModel.IsShopOpen() && Controls.NpcInteract.Continue.WasPressedThisFrame())
            {
                _eventBus.Publish(new EContinueDialogueCommandExecuted());
            }
        }

        private void HandleShopInput()
        {
            if (_shopModel.IsShopOpen() && Controls.Shop.Confirm.WasPressedThisFrame())
            {
                _eventBus.Publish(new EShopConfirmCommandExecuted());
            }
            
            if (_shopModel.IsShopOpen() && Controls.Shop.Cancel.WasPressedThisFrame())
            {
                _eventBus.Publish(new EShopCancelCommandExecuted());
            }
        }
        
        private void HandleGameMenuInput()
        {
            if (Controls.GameMenu.ToggleMenu.WasPressedThisFrame())
            {
                _gameMenuHandler.ToggleMenu();
            }
        }
        
        #region Gameplay Controls
        public bool RunReleased() => !IsSomeMenuOpen() && Controls.Running.Run.WasReleasedThisFrame();
        public bool RunIsPressed() => !IsSomeMenuOpen() && Controls.Running.Run.IsPressed();
        public bool CrouchIsPressed() => !IsSomeMenuOpen() && Controls.Crouching.Crouch.IsPressed();
        public bool CrouchWasPressedThisFrame() => !IsSomeMenuOpen() && Controls.Crouching.Crouch.WasPressedThisFrame();
        public bool CrouchWasReleasedThisFrame() => !IsSomeMenuOpen() && Controls.Crouching.Crouch.WasReleasedThisFrame();
        public bool JumpWasPressedThisFrame() => !IsSomeMenuOpen() && Controls.Jumping.Jump.WasPressedThisFrame();
        public bool JumpIsPressed() => !IsSomeMenuOpen() && Controls.Jumping.Jump.IsPressed();
        public bool JumpWasReleasedThisFrame() => !IsSomeMenuOpen() && Controls.Jumping.Jump.WasReleasedThisFrame();
        public bool DashWasPressedThisFrame() => !IsSomeMenuOpen() && Controls.Dashing.Dash.WasPressedThisFrame();
        public bool LootIsPressed() => !IsSomeMenuOpen() && Controls.Looting.Loot.IsPressed();
        public bool OpenDoorIsPressed() => !IsSomeMenuOpen() && Controls.OpeningDoors.Open.IsPressed();
        public bool PitGenerateWasPressedThisFrame() => !IsSomeMenuOpen() && Controls.Pits.Generate.WasPressedThisFrame();
        public bool GrabThrowWasPressedThisFrame() =>
            !IsSomeMenuOpen() && Controls.GrabRelease.GrabThrow.WasPressedThisFrame();
        public bool ReleaseObjectWasPressedThisFrame() =>
            !IsSomeMenuOpen() && Controls.GrabRelease.Release.WasPressedThisFrame();
        public bool NpcTalkWasPressedThisFrame() =>
            !IsSomeMenuOpen() && Controls.NpcInteract.Talk.WasPressedThisFrame();
        public bool GeneralInteractWasPressedThisFrame() =>
            !IsSomeMenuOpen() && Controls.GeneralInteraction.Interact.WasPressedThisFrame();
        public bool OpenStatueMenuWasPressedThisFrame() =>
            !IsSomeMenuOpen() && Controls.StatueMenu.OpenMenu.WasPressedThisFrame();
        public bool CloseStatueMenuWasPressedThisFrame() =>
            _statueMenuHandler.IsStatueMenuOpen() && Controls.StatueMenu.CloseMenu.WasPressedThisFrame();
        public bool StatueMenuIncreaseValueWasPressedThisFrame() =>
            _statueMenuHandler.IsStatueMenuOpen() && Controls.StatueMenu.Increase.WasPressedThisFrame();
        public bool StatueMenuDecreaseValueWasPressedThisFrame() =>
            _statueMenuHandler.IsStatueMenuOpen() && Controls.StatueMenu.Decrease.WasPressedThisFrame();
        public bool StatueMenuConfirmWasPressedThisFrame() =>
            _statueMenuHandler.IsStatueMenuOpen() && Controls.StatueMenu.Confirm.WasPressedThisFrame();
        public bool StatueMenuGoBackWasPressedThisFrame() =>
            _statueMenuHandler.IsStatueMenuOpen() && Controls.StatueMenu.GoBack.WasPressedThisFrame();
        public bool GameMenuNextTabWasPressedThisFrame() =>
            _gameMenuHandler.IsGameMenuOpen() && Controls.GameMenu.TabMenuRight.WasPressedThisFrame();
        public bool GameMenuPreviousTabWasPressedThisFrame() =>
            _gameMenuHandler.IsGameMenuOpen() && Controls.GameMenu.TabMenuLeft.WasPressedThisFrame();
        public bool GameMenuConfirmWasPressedThisFrame() =>
            _gameMenuHandler.IsGameMenuOpen() && Controls.GameMenu.Confirm.WasPressedThisFrame();
        public bool GameMenuGoBackWasPressedThisFrame() =>
            _gameMenuHandler.IsGameMenuOpen() && Controls.GameMenu.GoBack.WasPressedThisFrame();

        #endregion
    }
}
