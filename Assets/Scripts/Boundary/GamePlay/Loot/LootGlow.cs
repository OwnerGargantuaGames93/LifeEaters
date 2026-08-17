using Data.Entities.Loot;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Boundary.Loot
{
    [RequireComponent(typeof(Light2D))]
    public class LootGlow : MonoBehaviour
    {
        [SerializeField] private Loot loot;
        [SerializeField] private Light2D light2D;
        [SerializeField] private ParticleSystem particles;

        [Header("Colore per categoria (priorità dall'alto in basso)")]
        [SerializeField] private Color equipmentColor = new Color(0.3f, 0.8f, 1f);
        [SerializeField] private Color essenceColor = new Color(0.8f, 0.3f, 1f);
        [SerializeField] private Color keyColor = new Color(1f, 0.85f, 0.2f);
        [SerializeField] private Color lifeColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private Color consumableColor = new Color(0.3f, 1f, 0.4f);
        [SerializeField] private Color defaultColor = Color.white;

        [Header("Pulse")]
        [SerializeField] private float baseIntensity = 1.5f;
        [SerializeField] private float pulseAmplitude = 0.5f;
        [SerializeField] private float pulseSpeed = 2f;

        private void Reset()
        {
            loot = GetComponent<Loot>();
            light2D = GetComponent<Light2D>();
            particles = GetComponentInChildren<ParticleSystem>();
        }

        private void Awake()
        {
            if (loot == null) loot = GetComponent<Loot>();
            if (light2D == null) light2D = GetComponent<Light2D>();

            var color = ResolveColor(loot.data);

            light2D.color = color;
            light2D.intensity = baseIntensity;

            if (particles != null)
            {
                var main = particles.main;
                main.startColor = color;
            }
        }

        private void Update()
        {
            light2D.intensity = baseIntensity + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        }

        private Color ResolveColor(LootData data)
        {
            if (data.Equipment is { Count: > 0 }) return equipmentColor;
            if (data.Essences is { Count: > 0 }) return essenceColor;
            if (data.Keys is { Count: > 0 }) return keyColor;
            if (data.Lifes is { Count: > 0 }) return lifeColor;
            if (data.Consumables is { Count: > 0 }) return consumableColor;

            return defaultColor;
        }
    }
}
