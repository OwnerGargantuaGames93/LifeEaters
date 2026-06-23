using Data.Entities.Item;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Interactable
{
    public class Life: MonoBehaviour
    {
        private IEventBus _eventBus;

        private LifeId _lifeId;
        
        [Header("Vertical Movement")]
        [SerializeField] private float floatSpeed = 1.5f;

        [Header("Lateral Wave Motion")]
        [SerializeField] private float waveAmplitude = 2f;
        [SerializeField] private float waveFrequency = 2f;
        
        // Le vite scompaiono dopo 30 secondi
        [SerializeField] private float duration = 30f;

        private float _startTime;
        private Vector3 _startPosition;
        
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
        }

        private void Start()
        {
            _startTime = Time.time;
            _startPosition = transform.position;
        }

        private void Update()
        {
            var elapsed = Time.time - _startTime;

            // Movimento verticale costante verso l'alto
            var vertical = elapsed * floatSpeed;

            // Oscillazione orizzontale sinusoidale
            var horizontal = Mathf.Sin(elapsed * waveFrequency) * waveAmplitude;

            // Aggiorna posizione
            transform.position = _startPosition + new Vector3(horizontal, vertical, 0f);
            
            // Distruggi dopo la durata specificata
            if (elapsed >= duration)
            {
                Destroy(gameObject);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(Constants.PlayerTag))
            {
                return;
            }
            
            // Collect Life
            _eventBus.Publish(new ELifeCollected(_lifeId));
            
            Destroy(gameObject);
        }
        
        public void SetLifeId(LifeId id)
        {
            _lifeId = id;
        }
    }
    
    #region Events
    public struct ELifeCollected
    {
        public readonly LifeId Id;

        public ELifeCollected(LifeId id)
        {
            Id = id;
        }
    }
    #endregion
    
}