using Boundary.GamePlay.Projectile;
using Boundary.Interactable;
using Boundary.UI;
using Boundary.Utils;
using Control.Player;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        private IEventBus _eventBus;

        private Animator _animator;
        private PlayerController _playerController;
        private GrabObjects _grabObjects;
        private TouchingDirections _touchingDirections;

        private bool _isDying;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;

            _animator = GetComponent<Animator>();
            _playerController = GetComponent<PlayerController>();
            _grabObjects = GetComponent<GrabObjects>();
            _touchingDirections = GetComponent<TouchingDirections>();

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EPlayerRestOnStatue>(OnPlayerRestOnStatue);
            _eventBus.Subscribe<EStatueMenuClosed>(OnStatueMenuClosed);
            _eventBus.Subscribe<EPlayerObjectThrown>(OnPlayerObjectThrown);
            _eventBus.Subscribe<EPlayerObjectPutDown>(OnPlayerObjectPutDown);
            _eventBus.Subscribe<EPlayerGrabbedPitObject>(OnPlayerGrabbedPitObject);
            _eventBus.Subscribe<EPlayerReceiveContactDamageByEnemy>(OnPlayerHurt);
            _eventBus.Subscribe<EEnemyBulletHitPlayer>(OnPlayerHurtByBullet);
            _eventBus.Subscribe<EPlayerDeathSequenceStarted>(OnPlayerDeathSequenceStarted);
            _eventBus.Subscribe<EPlayerDeathSequenceEnded>(OnPlayerDeathSequenceEnded);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EPlayerRestOnStatue>(OnPlayerRestOnStatue);
            _eventBus.Unsubscribe<EStatueMenuClosed>(OnStatueMenuClosed);
            _eventBus.Unsubscribe<EPlayerObjectThrown>(OnPlayerObjectThrown);
            _eventBus.Unsubscribe<EPlayerObjectPutDown>(OnPlayerObjectPutDown);
            _eventBus.Unsubscribe<EPlayerGrabbedPitObject>(OnPlayerGrabbedPitObject);
            _eventBus.Unsubscribe<EPlayerReceiveContactDamageByEnemy>(OnPlayerHurt);
            _eventBus.Unsubscribe<EEnemyBulletHitPlayer>(OnPlayerHurtByBullet);
            _eventBus.Unsubscribe<EPlayerDeathSequenceStarted>(OnPlayerDeathSequenceStarted);
            _eventBus.Unsubscribe<EPlayerDeathSequenceEnded>(OnPlayerDeathSequenceEnded);
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            if (_isDying) return;

            var isCarryingObject = _grabObjects.GetCurrentGrabbedObject() != null;

            _animator.SetFloat(AnimationStrings.isCarryingObject, isCarryingObject ? 1f : 0f);
            _animator.SetBool(AnimationStrings.isGrounded, _touchingDirections.IsGrounded);
            _animator.SetBool(AnimationStrings.isOnWall, _touchingDirections.IsOnWall);
            _animator.SetBool(AnimationStrings.isCeiling, _touchingDirections.IsCeiling);

            // Keeps the run animation going (slowing via runAnimSpeedMultiplier) while the player
            // is still decelerating after releasing input, instead of cutting straight to Idle.
            _animator.SetBool(AnimationStrings.isMoving,
                _playerController.IsWalking || _playerController.IsRunning || _playerController.IsPhysicallyMoving);
            _animator.SetBool(AnimationStrings.isCrouching, _playerController.IsCrouching);
            _animator.SetBool(AnimationStrings.isDashing, _playerController.IsDashing);
            _animator.SetBool(AnimationStrings.isClimbing, _playerController.IsClimbing);
            _animator.SetBool(AnimationStrings.isWallSliding, _playerController.IsWallSliding);

            _animator.SetBool(AnimationStrings.isGoingUp, _playerController.IsGoingUp);
            _animator.SetBool(AnimationStrings.isFalling, _playerController.IsFalling());

            _animator.SetFloat(AnimationStrings.runAnimSpeedMultiplier, _playerController.RunAnimSpeedMultiplier);
            _animator.SetFloat(AnimationStrings.climbAnimSpeedMultiplier, _playerController.IsClimbingMoving ? 1f : 0f);
        }

        private void OnPlayerRestOnStatue(EPlayerRestOnStatue e)
        {
            _animator.SetBool(AnimationStrings.isPraying, true);
            _animator.SetTrigger(AnimationStrings.startStopPray);
        }

        private void OnStatueMenuClosed(EStatueMenuClosed e)
        {
            _animator.SetBool(AnimationStrings.isPraying, false);
            _animator.SetTrigger(AnimationStrings.startStopPray);
        }

        private void OnPlayerObjectThrown(EPlayerObjectThrown e) => _animator.SetTrigger(AnimationStrings.throwTrigger);
        private void OnPlayerObjectPutDown(EPlayerObjectPutDown e) => _animator.SetTrigger(AnimationStrings.throwTrigger);
        private void OnPlayerGrabbedPitObject(EPlayerGrabbedPitObject e) => _animator.SetTrigger(AnimationStrings.grabTrigger);

        // No "hurt" clip exists yet - the tag will be added later. The trigger already fires
        // correctly on every hit, ready to be wired up once the clip is in place.
        private void OnPlayerHurt(EPlayerReceiveContactDamageByEnemy e) => _animator.SetTrigger(AnimationStrings.hurtTrigger);
        private void OnPlayerHurtByBullet(EEnemyBulletHitPlayer e) => _animator.SetTrigger(AnimationStrings.hurtTrigger);

        private void OnPlayerDeathSequenceStarted(EPlayerDeathSequenceStarted e)
        {
            _isDying = true;
            _animator.SetBool(AnimationStrings.isDying, true);
        }

        private void OnPlayerDeathSequenceEnded(EPlayerDeathSequenceEnded e)
        {
            _isDying = false;
            _animator.SetBool(AnimationStrings.isDying, false);
        }

        // No real caller yet - Slam has no damage/physics logic implemented.
        public void TriggerSlam() => _animator.SetTrigger(AnimationStrings.slamTrigger);

        // Outfit swap hook: any future outfit must be an AnimatorOverrideController sharing this
        // same base controller's states/parameters, only the Motion per state changes.
        public void SetOutfit(RuntimeAnimatorController controller) => _animator.runtimeAnimatorController = controller;
    }
}
