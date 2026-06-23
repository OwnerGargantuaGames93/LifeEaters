using System.Collections;
using System.Collections.Generic;
using Boundary.Enemy.Interfaces;
using Boundary.Enemy.StateMachine;
using Boundary.Enemy.StateMachine.ConcreteStates;
using Boundary.GamePlay.Enemy.Behaviors.Chase;
using Boundary.GamePlay.Enemy.Behaviors.Dormant;
using Boundary.GamePlay.Enemy.Behaviors.Idle;
using Boundary.GamePlay.Enemy.Behaviors.MeleeAttack; // namespace also used by EnemyCooldownSOBase
using Boundary.GamePlay.Enemy.StateMachine.ConcreteStates;
using Boundary.GamePlay.Interactable;
using Boundary.Interactable;
using Boundary.UI.Components;
using Boundary.Utils;
using Control.Damage.UseCase;
using Control.Player;
using Data.Damage;
using Data.Entities.Enemy;
using Data.Entities.Item;
using Infra.EventBus;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Boundary.GamePlay.Enemy.Base
{
    [RequireComponent(typeof(TouchingDirections))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class BaseEnemy : IdentifiableMonoBehaviour, ITriggerCheckable
    {
        private IEventBus _eventBus;
        private UCombatSystem _combatSystem;
    
        #region Unity Editor + Data
        [field: SerializeField] private int MaxHealth { get; set; } = 1;
        [field: SerializeField] private int PointsDrop { get; set; } = 1;
        [field: SerializeField] private int JumpAttackDefense { get; set; } = 0;
        [field: SerializeField] private int ThrowAttackDefense { get; set; } = 0;
        [field: SerializeField] private List<LifeDrop> PossibleLifeDrops { get; set; }
        [field: SerializeField] private EnemyType enemyType = EnemyType.Regular;
        [field: SerializeField] private int Phase2HealthThreshold { get; set; } = 0;
        [field: SerializeField] private int EnemyPoisonResistance { get; set; } = 5;
        [field: SerializeField] private int EnemyBurnResistance { get; set; } = 5;
        [field: SerializeField] private int EnemyFrostResistance { get; set; } = 5;
        [field: SerializeField] private EnemyModel data;
        private Vector3 _initialPosition;
        private bool _inComa;
        private Coroutine _poisonCoroutine;
        #endregion
    
        #region Attacks
        [field: SerializeField] public DamageOutput contactDamageOutput;
        #endregion
        
        #region Components
        public Rigidbody2D Rb { get; private set; }
        public TouchingDirections TouchingDirections { get; private set; }
        [SerializeField] public DetectionZone cliffDetection;
        [SerializeField] public bool turnWasPerformed;
        [SerializeField] public float doubleTurnTimeBuffer;
        [SerializeField] public float doubleTurnPreventionTime = 0.5f;
        [SerializeField] public GameObject sleepPosition;
        public GameObject Player { get; private set; }
        public Animator Animator { get; private set; }
        [SerializeField] public GameObject eyes;
        [SerializeField] public GameObject ContactAttackArea;
        [SerializeField] public VisualTip visualTipPrefab;
        public Canvas Canvas { get; private set; }

        #endregion

        #region Knockback
        public bool isKnockedBack;
        #endregion

        #region Data to expose

        public Vector2 Position { get; private set; } = Vector2.zero;

        #endregion

        #region State Machina Variables

        public EnemyStateMachine StateMachine { get; private set; }
        
        public EnemyDormantState DormantState { get; private set; }

        public EnemyIdleState IdleState { get; private set; }

        public EnemyChaseState ChasingState { get; private set; }
        
        public EnemyCooldownState CooldownState { get; private set; }
        
        public EnemyRangedAttackState RangedAttackState { get; private set; }

        public EnemyState InitialState;

        #endregion

        #region Behaviours Scriptable Objects

        // For plug logics in the enemy
        [SerializeField] private EnemyDormantSOBase enemyDormantBase;
        [SerializeField] private EnemyIdleSOBase enemyIdleBase;
        [SerializeField] private EnemyChaseSOBase enemyChaseBase;
        [SerializeField] private EnemyCooldownSOBase enemyCooldownBase;
        [SerializeField] private EnemyRangedAttackSOBase enemyRangedAttackBase;
    
        // Concrete instances of the behaviours (if not, every enemy will share the same instances)
        public EnemyDormantSOBase EnemyDormantBaseInstance { get; private set; }
        public EnemyIdleSOBase EnemyIdleBaseInstance { get; private set; }
        public EnemyChaseSOBase EnemyChaseBaseInstance { get; private set; }
        public EnemyCooldownSOBase EnemyCooldownBaseInstance { get; private set; }
        public EnemyRangedAttackSOBase EnemyRangedAttackBaseInstance { get; private set; }

        #endregion

        public bool IsAggroed { get ; set ; }
        public bool IsAwake { get; set; }
        public bool InRangedAttackRange { get; set; }

        #region Movement

        public enum WalkDirectionEnum
        {
            Right,
            Left
        } 

        private WalkDirectionEnum _walkDirection;
        public WalkDirectionEnum WalkDirection
        {
            get => _walkDirection;
            set
            {
                if (value != _walkDirection)
                {
                    // flip the sprite
                    gameObject.transform.localScale = new Vector2(gameObject.transform.localScale.x * -1, gameObject.transform.localScale.y);
                    walkDirectionVector = value == WalkDirectionEnum.Right ? Vector2.right : Vector2.left;
                }

                _walkDirection = value;
            }
        }

        public Vector2 walkDirectionVector;

        #endregion

        private void Awake()
        {
            CommonAwake();
            
            _eventBus = GameContext.Instance.EventBus;
            _combatSystem = GameContext.Instance.CombatSystem;
            
            Animator = GetComponent<Animator>();

            if (enemyDormantBase)
            {
                EnemyDormantBaseInstance = Instantiate(enemyDormantBase);
            }
            
            if (enemyIdleBase)
            {
                EnemyIdleBaseInstance = Instantiate(enemyIdleBase);    
            }

            if (enemyChaseBase)
            {
                EnemyChaseBaseInstance = Instantiate(enemyChaseBase);    
            }

            if (enemyCooldownBase)
            {
                EnemyCooldownBaseInstance = Instantiate(enemyCooldownBase);
            }
            
            if (enemyRangedAttackBase)
            {
                EnemyRangedAttackBaseInstance = Instantiate(enemyRangedAttackBase);
            }

            Player = GameObject.FindGameObjectWithTag(Constants.PlayerTag);
            Canvas = GameObject.FindGameObjectWithTag(Constants.CanvasTag).GetComponent<Canvas>();
            
            StateMachine = new EnemyStateMachine();

            DormantState = new EnemyDormantState(this, StateMachine);
            IdleState = new EnemyIdleState(this, StateMachine);
            ChasingState = new EnemyChaseState(this, StateMachine);
            CooldownState = new EnemyCooldownState(this, StateMachine);
            RangedAttackState = new EnemyRangedAttackState(this, StateMachine);
            
            _initialPosition = transform.position;
            
            if (transform.localScale.x >= 0)
            {
                _walkDirection = WalkDirectionEnum.Right;
                walkDirectionVector = Vector2.right;
            }
            else
            {
                _walkDirection = WalkDirectionEnum.Left;
                walkDirectionVector = Vector2.left;
            }
        
            SubscribeToEvents();
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EEnemyHitByPitObjectEvent>(OnHitByPitObject);
            _eventBus.Subscribe<EEnemyFallInDeadBox>(OnEnemyFallInDeadBox);
            _eventBus.Subscribe<EPlayerRestOnStatue>(OnPlayerRestOnStatue);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EEnemyHitByPitObjectEvent>(OnHitByPitObject);
            _eventBus.Unsubscribe<EEnemyFallInDeadBox>(OnEnemyFallInDeadBox);
            _eventBus.Unsubscribe<EPlayerRestOnStatue>(OnPlayerRestOnStatue);
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Start()
        {
            Rb = GetComponent<Rigidbody2D>();
            TouchingDirections = GetComponent<TouchingDirections>();

            Initialize(true);
        }

        private void Update()
        {
            if (_inComa)
            {
                return;
            }
            
            if (data.CurrentStatus == EnemyStatus.Dead)
            {
                return;
            }
            
            Position = new Vector2(transform.position.x, transform.position.y);

            StateMachine.CurrentEnemyState.FrameUpdate();
        }

        private void FixedUpdate()
        {
            if (_inComa)
            {
                return;
            }
            
            if (data.CurrentStatus == EnemyStatus.Dead)
            {
                return;
            }
            
            StateMachine.CurrentEnemyState.PhysicsUpdate();
        }

        public void OnStomped()
        {
            // Here i want to disable the enemy attack hitbox to avoid the player receiving damage while bouncing on the enemy
            if (ContactAttackArea != null)
            {
                StartCoroutine(DisableContactDamageTemporarily());   
            }
            
            var damage = _combatSystem.CalculateJumpAttackDamage(data);
            
            _eventBus.Publish(new EEnemyStompedEvent(Id));
            
            SpawnDamageVisualTip(damage);
            
            HandleStatus();

            StateMachine.CurrentEnemyState.OnReceiveDamage();
        }
        
        private IEnumerator DisableContactDamageTemporarily()
        {
            ContactAttackArea.SetActive(false);
            yield return new WaitForSeconds(0.25f);
            ContactAttackArea.SetActive(true);
        }

        private void OnHitByPitObject(EEnemyHitByPitObjectEvent e)
        {
            if (data == null)
            {
                return;
            }
            
            if (e.EnemyId != data.Id)
            {
                return;
            }
            
            StateMachine.CurrentEnemyState.OnReceiveDamage();
            
            var damage = _combatSystem.CalculateThrowAttackDamage(data, e.PitObject);
            
            SpawnDamageVisualTip(damage);
            
            HandleStatus();
        }
        
        #region Event Handlers
        
        private void OnEnemyFallInDeadBox(EEnemyFallInDeadBox e)
        {
            if (e.EnemyId != data.Id)
            {
                return;
            }
            
            Kill();
        }

        private void OnPlayerRestOnStatue(EPlayerRestOnStatue e)
        {
            switch (enemyType)
            {
                case EnemyType.Regular:
                case EnemyType.Unique:
                {
                    Initialize(false);
                    break;
                }
                case EnemyType.CombatRoom:
                case EnemyType.Boss:
                default:
                    return;
            }
        }
        
        #endregion
        
        private void SetToInitialState()
        {
            data = new EnemyModel(MaxHealth, PointsDrop, JumpAttackDefense, ThrowAttackDefense, Id, PossibleLifeDrops, Phase2HealthThreshold, EnemyPoisonResistance, EnemyBurnResistance, EnemyFrostResistance);

            if (EnemyDormantBaseInstance)
            {
                EnemyDormantBaseInstance.Initialize(gameObject, this);
            }
            
            if (EnemyIdleBaseInstance)
            {
                EnemyIdleBaseInstance.Initialize(gameObject, this);
            }

            if (EnemyChaseBaseInstance)
            {
                EnemyChaseBaseInstance.Initialize(gameObject, this);    
            }
            
            if (EnemyCooldownBaseInstance)
            {
                EnemyCooldownBaseInstance.Initialize(gameObject, this);    
            }
            
            if (EnemyRangedAttackBaseInstance)
            {
                EnemyRangedAttackBaseInstance.Initialize(gameObject, this);    
            }
            
            InitialState = EnemyDormantBaseInstance != null ? DormantState : IdleState;
            StateMachine.Initialize(InitialState);
        }

        private void HandleStatus()
        {
            if (data.CurrentStatus == EnemyStatus.Dead)
            {
                // TODO: For standard and combat room enemies run the death visual logic (upside down and go below the screen)
                StateMachine.ChangeState(InitialState);
                gameObject.SetActive(false);

                DropLifeHandler();

                _eventBus.Publish(new EEnemyDied(data));
                return;
            }

            if (data.CurrentStatus == EnemyStatus.Poisoned && _poisonCoroutine == null)
            {
                _poisonCoroutine = StartCoroutine(ApplyPoisonDamageOverTime());
            }
        }

        private IEnumerator ApplyPoisonDamageOverTime()
        {
            const float tickInterval = 10f;
            const float duration = 60f;
            const int damagePerTick = 1;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;

                if (data.CurrentStatus == EnemyStatus.Dead)
                {
                    _poisonCoroutine = null;
                    yield break;
                }

                data.CurrentHealth -= damagePerTick;
                SpawnDamageVisualTip(new DamageOutput(damagePerTick, 0, 0, 0, 0));

                if (data.CurrentHealth <= 0)
                {
                    Kill();
                    yield break;
                }
            }

            data.CurrentStatus = EnemyStatus.Normal;
            data.CurrentPoisonAmount = 0;
            _poisonCoroutine = null;
        }

        private void Kill()
        {
            data.CurrentHealth = 0;
            data.CurrentStatus = EnemyStatus.Dead;
        
            HandleStatus();
        }

        private void DropLifeHandler()
        {
            var randomValue = Random.value;

            LifeId? lifeToDrop = null;
            var cumulativeProbability = 0f;
            foreach (var lifeDrop in PossibleLifeDrops)
            {
                cumulativeProbability += lifeDrop.dropChance;
                if (randomValue <= cumulativeProbability)
                {
                    lifeToDrop = lifeDrop.lifeId;
                }
            }

            if (!lifeToDrop.HasValue)
            {
                return;
            }
            
            _eventBus.Publish(new ELifeDropped(lifeToDrop.Value, transform.position));
        }

        private void SpawnDamageVisualTip(DamageOutput damage)
        {
            var worldPosition = new Vector3(transform.position.x + 0.5f, transform.position.y + 1f, transform.position.z);
            var visualTip = Instantiate(visualTipPrefab, Canvas.transform);
            
            // Set the initial world position - the visual tip will track this position in world space
            visualTip.SetInitialWorldPosition(worldPosition);
            visualTip.InitDamage(damage);
        }

        #region ITriggerCheckable
        public void SetAggroStatus(bool isAggroed)
        {
            IsAggroed = isAggroed;
        }

        public void SetIsAwake(bool isAwake)
        {
            IsAwake = isAwake;
        }
        
        public void SetInRangedAttackRange(bool inRange)
        {
            InRangedAttackRange = inRange;
        }
        
        #endregion

        #region Deal Contact Damage
        public void DealContactDamage() {
            _eventBus.Publish(new EPlayerReceiveContactDamageByEnemy(contactDamageOutput, gameObject.transform.position));
        }


        #endregion

        public bool IsDead()
        {
            return data.CurrentStatus == EnemyStatus.Dead;
        }

        public void SetComa(bool inComa)
        {
            _inComa = inComa;
        }

        public void Turn() {
            WalkDirection = WalkDirection == WalkDirectionEnum.Right ? WalkDirectionEnum.Left : BaseEnemy.WalkDirectionEnum.Right;
            turnWasPerformed = true;
        }

        public void Initialize(bool firstTime)
        {
            _poisonCoroutine = null;
            transform.position = _initialPosition;
            SetToInitialState();
            gameObject.SetActive(true);

            if (!firstTime)
            {
                _eventBus.Publish(new EEnemyRespawned(data));
            }
        }

        public EnemyPhase CurrentPhase()
        {
            return data.CurrentPhase;
        }
        
        public void ApplyJumpAttackDefenseDebuff(int debuffAmount)
        {
            data.JumpAttackDefense -= debuffAmount;
        }
        
        public void ApplyThrowAttackDefenseDebuff(int debuffAmount)
        {
            data.ThrowAttackDefense -= debuffAmount;
        }
        
        public void RemoveJumpAttackDefenseDebuff(int debuffAmount)
        {
            data.JumpAttackDefense += debuffAmount;
        }
        
        public void RemoveThrowAttackDefenseDebuff(int debuffAmount)
        {
            data.ThrowAttackDefense += debuffAmount;
        }
    }
    
    public enum EnemyType
    {
        Regular,
        CombatRoom,
        Unique,
        Boss,
    }
    
    #region Events

    public struct EEnemyStompedEvent
    {
        public readonly string EnemyId;
        
        public EEnemyStompedEvent(string enemyId)
        {
            EnemyId = enemyId;
        }
    }
    
    public struct EEnemyHitByPitObjectEvent
    {
        public readonly string EnemyId;
        public readonly PitObjectData PitObject;
        
        public EEnemyHitByPitObjectEvent(string enemyId, PitObjectData pitObject)
        {
            EnemyId = enemyId;
            PitObject = pitObject;
        }
    }
    
    public struct EEnemyDied
    {
        public readonly EnemyModel Enemy;
        
        public EEnemyDied(EnemyModel enemy)
        {
            Enemy = enemy;
        }
    }
    
    public struct EEnemyRespawned
    {
        public readonly EnemyModel Enemy;
        
        public EEnemyRespawned(EnemyModel enemy)
        {
            Enemy = enemy;
        }
    }

    public struct ELifeDropped
    {
        public readonly LifeId LifeId;
        public readonly Vector3 DropPosition;

        public ELifeDropped(LifeId lifeId, Vector3 dropPosition)
        {
            LifeId = lifeId;
            DropPosition = dropPosition;
        }
    }
    
    #endregion
}