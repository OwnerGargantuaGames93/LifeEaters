using System.Collections;
using Data.Damage;
using TMPro;
using UnityEngine;

namespace Boundary.UI.Components
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VisualTip: MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float moveUpSpeed = 50f;
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private Color hpDamageColor = Color.red;
        [SerializeField] private Color hpHealColor = Color.green;
        [SerializeField] private Color energyGainColor = Color.black;
        [SerializeField] private Color pointsGainColor = Color.white;
        [SerializeField] private Color criticalHitColor = Color.yellow;
        
        private TextMeshProUGUI _textMesh;
        private RectTransform _rectTransform;
        private Canvas _canvas;
        private UnityEngine.Camera _camera;
        private Vector3 _worldPosition;
        private Color _color;
        
        private void Awake()
        {
            _textMesh = GetComponent<TextMeshProUGUI>();
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            _camera = UnityEngine.Camera.main;
        }
        
        public void SetInitialWorldPosition(Vector3 worldPosition)
        {
            _worldPosition = worldPosition;
            UpdateCanvasPosition();
        }
        
        private void UpdateCanvasPosition()
        {
            if (_camera == null || _canvas == null) return;
            
            var screenPosition = _camera.WorldToScreenPoint(_worldPosition);
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                screenPosition,
                _camera,
                out canvasPosition
            );
            
            _rectTransform.anchoredPosition = canvasPosition;
        }
        
        public void InitDamage(DamageOutput damageOutput)
        {
            _textMesh.text = "-" + damageOutput.healthDamage;
            _textMesh.color = damageOutput.criticalHit ? criticalHitColor : hpDamageColor;
            _color = _textMesh.color;

            StartCoroutine(Animate());
        }
        
        public void InitHeal(int healAmount)
        {
            _textMesh.text = "+" + healAmount;
            _textMesh.color = hpHealColor;
            _color = _textMesh.color;

            StartCoroutine(Animate());
        }
        
        public void InitEnergyGain(int energyAmount)
        {
            _textMesh.text = energyAmount > 0 ? "+" + energyAmount : energyAmount.ToString();
            _textMesh.color = energyGainColor;
            _color = _textMesh.color;

            StartCoroutine(Animate());
        }
        
        public void InitPointsGain(int pointsAmount)
        {
            _textMesh.text = "+" + pointsAmount;
            _textMesh.color = pointsGainColor;
            _color = _textMesh.color;

            StartCoroutine(StaticAnimate());
        }

        private IEnumerator Animate()
        {
            float t = 0;

            while (t < lifetime)
            {
                t += Time.deltaTime;

                // Move up in world space
                _worldPosition += Vector3.up * (moveUpSpeed * Time.deltaTime);
                
                // Update canvas position based on new world position
                UpdateCanvasPosition();

                // Fade out
                var alpha = Mathf.Lerp(1, 0, t / lifetime);
                _textMesh.color = new Color(_color.r, _color.g, _color.b, alpha);

                yield return null;
            }

            Destroy(gameObject);
        }
        
        private IEnumerator StaticAnimate()
        {
            float t = 0;

            while (t < lifetime)
            {
                t += Time.deltaTime;
                
                // Update canvas position in case camera moves
                UpdateCanvasPosition();
                
                // Fade out
                var alpha = Mathf.Lerp(1, 0, t / lifetime);
                _textMesh.color = new Color(_color.r, _color.g, _color.b, alpha);

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}