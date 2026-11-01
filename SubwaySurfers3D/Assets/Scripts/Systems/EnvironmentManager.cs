using UnityEngine;

namespace MetroRunner.Systems
{
    public enum WeatherType
    {
        Clear,
        Rain,
        Fog
    }

    public class EnvironmentManager : MonoBehaviour
    {
        public static EnvironmentManager Instance { get; private set; }

        [Header("Lighting")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Color dayColor = new Color(1f, 0.95f, 0.85f);
        [SerializeField] private Color nightColor = new Color(0.1f, 0.15f, 0.3f);

        [Header("Weather Particles")]
        [SerializeField] private ParticleSystem rainParticles;

        public bool IsNight { get; private set; } = false;
        public WeatherType CurrentWeather { get; private set; } = WeatherType.Clear;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ToggleDayNight()
        {
            IsNight = !IsNight;
            if (sunLight != null)
            {
                sunLight.color = IsNight ? nightColor : dayColor;
                sunLight.intensity = IsNight ? 0.3f : 1.2f;
            }
            RenderSettings.fogColor = IsNight ? new Color(0.05f, 0.08f, 0.15f) : new Color(0.7f, 0.8f, 0.9f);
        }

        public void SetWeather(WeatherType weather)
        {
            CurrentWeather = weather;
            if (rainParticles != null)
            {
                if (weather == WeatherType.Rain) rainParticles.Play();
                else rainParticles.Stop();
            }

            RenderSettings.fogDensity = (weather == WeatherType.Fog) ? 0.035f : 0.012f;
        }
    }
}
