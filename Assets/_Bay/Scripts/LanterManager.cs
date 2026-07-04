using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LanternManager : MonoBehaviour
{
    [Header("Lantern Settings")]
    [SerializeField] private float maxFuel = 20f;
    [SerializeField] private float currentFuel;
    [SerializeField] private float decayRate = 0.5f; // Fuel per second
    [SerializeField] private float decayMultiplier = 1f; // Modified by fog, items

    [Header("Light Components")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private Light lanternSpotlight;
    [SerializeField] private GameObject lanternMesh;
    [SerializeField] private ParticleSystem lanternParticles;

    [Header("Light Settings")]
    [SerializeField] private float minLightIntensity = 0.5f;
    [SerializeField] private float maxLightIntensity = 3f;
    [SerializeField] private float minSpotAngle = 20f;
    [SerializeField] private float maxSpotAngle = 60f;
    [SerializeField] private float minRange = 5f;
    [SerializeField] private float maxRange = 15f;

    [Header("UI References")]
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private Image fuelFillImage;
    [SerializeField] private Gradient fuelGradient;
    [SerializeField] private GameObject lowFuelWarning;
    [SerializeField] private TMP_Text fuelText;

    [Header("Item Effects")]
    private float temporaryDecayReduction = 1f;
    private float temporaryRangeBoost = 1f;
    private float effectTimer = 0f;
    private bool isEffectActive = false;

    [Header("Audio")]
    [SerializeField] private AudioSource lanternAudio;
    [SerializeField] private AudioClip lowFuelSound;
    [SerializeField] private AudioClip refuelSound;
    [SerializeField] private AudioClip lanternFlickerSound;

    [Header("States")]
    private bool isLanternOn = true;
    private bool isInSafeZone = false;
    private bool isLowFuelWarningActive = false;
    private float flickerTimer = 0f;
    private float flickerInterval = 0.1f;

    // Public properties
    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public float FuelPercentage => currentFuel / maxFuel;
    public bool IsInSafeZone => isInSafeZone;

    // Singleton
    public static LanternManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        currentFuel = maxFuel;
        UpdateUI();
        UpdateLightIntensity();
        InvokeRepeating(nameof(DecayFuel), 1f, 1f);
    }

    void Update()
    {
        if (isLanternOn)
        {
            HandleFlicker();
            HandleLowFuelWarning();
        }

        UpdateLightIntensity();
        UpdateUI();

        // Check for item effect expiration
        if (isEffectActive)
        {
            effectTimer -= Time.deltaTime;
            if (effectTimer <= 0f)
            {
                ClearEffects();
            }
        }
    }

    #region Fuel Management

    void DecayFuel()
    {
        if (!isLanternOn || isInSafeZone) return;

        float effectiveDecay = decayRate * decayMultiplier * temporaryDecayReduction;
        currentFuel -= effectiveDecay;
        currentFuel = Mathf.Clamp(currentFuel, 0, maxFuel);

        if (currentFuel <= 0)
        {
            TurnOffLantern();
        }

        // Update audio pitch based on fuel level
        if (lanternAudio != null)
        {
            float pitch = Mathf.Lerp(0.5f, 1f, FuelPercentage);
            lanternAudio.pitch = pitch;
        }
    }

    public bool TrySpendFuel(float amount)
    {
        if (currentFuel >= amount)
        {
            currentFuel -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public void Refuel(float amount)
    {
        currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
        UpdateUI();

        if (!isLanternOn)
        {
            TurnOnLantern();
        }

        if (refuelSound != null)
        {
            AudioSource.PlayClipAtPoint(refuelSound, transform.position);
        }

        // Visual feedback
        if (lanternParticles != null)
        {
            lanternParticles.Play();
        }
    }

    #endregion

    #region Lantern Controls

    public void TurnOnLantern()
    {
        if (currentFuel <= 0) return;

        isLanternOn = true;
        lanternLight.enabled = true;
        lanternSpotlight.enabled = true;
        lanternMesh.SetActive(true);
        if (lanternParticles != null) lanternParticles.Play();
    }

    public void TurnOffLantern()
    {
        isLanternOn = false;
        lanternLight.enabled = false;
        lanternSpotlight.enabled = false;
        lanternMesh.SetActive(false);
        if (lanternParticles != null) lanternParticles.Stop();
    }

    public void ToggleLantern()
    {
        if (isLanternOn)
            TurnOffLantern();
        else
            TurnOnLantern();
    }

    #endregion

    #region Light Intensity

    void UpdateLightIntensity()
    {
        if (!isLanternOn) return;

        float fuelFactor = FuelPercentage;
        float intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, fuelFactor) * temporaryRangeBoost;

        lanternLight.intensity = intensity;
        lanternSpotlight.intensity = intensity * 0.5f;

        // Adjust spot angle and range based on fuel
        float spotAngle = Mathf.Lerp(minSpotAngle, maxSpotAngle, fuelFactor);
        float range = Mathf.Lerp(minRange, maxRange, fuelFactor) * temporaryRangeBoost;

        lanternSpotlight.spotAngle = spotAngle;
        lanternLight.range = range;
        lanternSpotlight.range = range * 1.2f;

        // Color temperature shifts slightly when low on fuel
        Color lightColor = Color.Lerp(new Color(1f, 0.6f, 0.2f), new Color(1f, 0.9f, 0.6f), fuelFactor);
        lanternLight.color = lightColor;
        lanternSpotlight.color = lightColor;
    }

    void HandleFlicker()
    {
        if (!isLanternOn) return;

        flickerTimer += Time.deltaTime;
        if (flickerTimer >= flickerInterval)
        {
            flickerTimer = 0f;
            flickerInterval = Random.Range(0.05f, 0.2f);

            // Slight random flicker
            float flicker = Random.Range(0.85f, 1.15f);
            lanternLight.intensity *= flicker;

            // More flicker when low on fuel
            if (FuelPercentage < 0.3f)
            {
                flicker = Random.Range(0.7f, 1.3f);
                lanternLight.intensity *= flicker;
            }
        }
    }

    void HandleLowFuelWarning()
    {
        bool isLowFuel = FuelPercentage < 0.2f && isLanternOn;

        //if (isLowFuel && !isLowFuelWarningActive)
        //{
        //    isLowFuelWarningActive = true;
        //    lowFuelWarning.SetActive(true);
        //    if (lowFuelSound != null)
        //    {
        //        lanternAudio.PlayOneShot(lowFuelSound);
        //    }
        //}
        //else if (!isLowFuel && isLowFuelWarningActive)
        //{
        //    isLowFuelWarningActive = false;
        //    lowFuelWarning.SetActive(false);
        //}
    }

    #endregion

    #region Item Effects

    public void ApplyDecayReduction(float reductionMultiplier, float duration)
    {
        temporaryDecayReduction = reductionMultiplier;
        effectTimer = duration;
        isEffectActive = true;

        // Example: Tnalak Cloth reduces decay by 30%
        // reductionMultiplier = 0.7f for 30% reduction
    }

    public void ApplyRangeBoost(float boostMultiplier, float duration)
    {
        temporaryRangeBoost = boostMultiplier;
        effectTimer = duration;
        isEffectActive = true;
    }

    void ClearEffects()
    {
        temporaryDecayReduction = 1f;
        temporaryRangeBoost = 1f;
        isEffectActive = false;
        effectTimer = 0f;
    }

    #endregion

    #region Safe Zone

    public void EnterSafeZone()
    {
        isInSafeZone = true;

        // Visual feedback
        if (lanternParticles != null)
        {
            lanternParticles.startColor = new Color(1f, 0.8f, 0.4f);
        }

        UIManager.Instance.ShowMessage("Entered Safe Zone!");
    }

    public void ExitSafeZone()
    {
        isInSafeZone = false;

        if (lanternParticles != null)
        {
            lanternParticles.startColor = new Color(1f, 0.6f, 0.2f);
        }

        UIManager.Instance.ShowMessage("Exited Safe Zone!");
    }

    #endregion

    #region Fog Effects

    public void ApplyFogEffect()
    {
        decayMultiplier = 2f; // Decay doubles in fog
    }

    public void RemoveFogEffect()
    {
        decayMultiplier = 1f;
    }

    #endregion

    #region UI

    void UpdateUI()
    {
        //if (fuelSlider != null)
        //{
        //    fuelSlider.value = FuelPercentage;
        //}

        //if (fuelFillImage != null)
        //{
        //    fuelFillImage.color = fuelGradient.Evaluate(FuelPercentage);
        //}

        //if (fuelText != null)
        //{
        //    fuelText.text = $"{Mathf.CeilToInt(currentFuel)} / {maxFuel}";
        //}

        UIManager.Instance.UpdateFuelDisplay(FuelPercentage, currentFuel, maxFuel);
    }

    public void ShowLowFuelWarning(bool show)
    {
        if (lowFuelWarning != null)
        {
            lowFuelWarning.SetActive(show);
        }
    }

    #endregion

    #region Debug

    [ContextMenu("Refuel Full")]
    void DebugRefuelFull()
    {
        Refuel(maxFuel);
    }

    [ContextMenu("Drain Fuel")]
    void DebugDrainFuel()
    {
        currentFuel = 0;
        UpdateUI();
        TurnOffLantern();
    }

    #endregion
}