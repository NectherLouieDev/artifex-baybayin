using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction _portalInputAction;
    [SerializeField] private InputAction _backInputAction;

    [Header("Main UI References")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private float messageDisplayTime = 3f;
    private Coroutine messageCoroutine;

    [Header("Audio Compass")]
    [SerializeField] private RectTransform compassArrow;
    [SerializeField] private Image compassIntensityBar;
    [SerializeField] private Image compassPulseRing;
    [SerializeField] private GameObject compassPanel;
    [SerializeField] private float pulseSpeed = 1f;
    private bool isCompassActive = false;
    private float currentIntensity = 0f;
    private float pulseTimer = 0f;

    [Header("Fuel Display")]
    [SerializeField] private GameObject fuelDisplay;
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private Image fuelFillImage;
    [SerializeField] private Gradient fuelGradient;
    [SerializeField] private TMP_Text fuelText;
    [SerializeField] private GameObject lowFuelWarning;

    [Header("Quest Display")]
    [SerializeField] private GameObject questDisplay;
    [SerializeField] private TMP_Text questText;

    [Header("Interaction")]
    [SerializeField] private TMP_Text interactPromptText;
    [SerializeField] private GameObject interactPanel;

    [Header("Discovery Screen")]
    [SerializeField] private GameObject discoveryScreenPanel;
    [SerializeField] private TMP_Text discoveryArtifactName;
    [SerializeField] private Image discoveryArtifactImage;
    [SerializeField] private TMP_Text discoveryLoreText;
    [SerializeField] private TMP_Text discoveryHistoricalFact;
    [SerializeField] private Button discoveryPortalButton;
    [SerializeField] private Button discoveryContinueButton;

    [Header("Item Screen")]
    [SerializeField] private GameObject itemScreenPanel;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemDiscoverdName;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemCulturalText;
    [SerializeField] private TMP_Text itemDetailText;
    [SerializeField] private TMP_Text itemEffectText;
    [SerializeField] private Button itemPortalButton;

    [Header("Portal Notification")]
    [SerializeField] private GameObject portalNotificationPanel;
    [SerializeField] private TMP_Text portalNotificationText;
    [SerializeField] private Image portalNotificationIcon;
    [SerializeField] private float portalNotificationDuration = 4f;
    private Coroutine portalNotificationCoroutine;

    [Header("Item Tooltip")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipNameText;
    [SerializeField] private TMP_Text tooltipOriginText;
    [SerializeField] private TMP_Text tooltipDescriptionText;
    [SerializeField] private TMP_Text tooltipEffectText;
    [SerializeField] private Button tooltipUseButton;
    [SerializeField] private Button tooltipDropButton;

    [Header("Inventory")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Image[] itemSlotImages;
    [SerializeField] private TMP_Text[] itemSlotCountTexts;
    [SerializeField] private TMP_Text emberlightSlotCountText;

    [Header("Game Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Debug")]
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private bool showDebug = false;

    public bool InventoryPanelActiveSelf { get { return inventoryPanel.activeSelf; } }

    // Singleton
    public static UIManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Hide all panels initially
        fuelDisplay.SetActive(false);
        questDisplay.SetActive(false);
        pauseMenuPanel.SetActive(false);
        victoryPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        discoveryScreenPanel.SetActive(false);
        itemScreenPanel.SetActive(false);
        portalNotificationPanel.SetActive(false);
        tooltipPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        interactPanel.SetActive(false);
        messagePanel.SetActive(false);
        compassPanel.SetActive(false);

        // Set up button listeners
        if (discoveryPortalButton != null)
            discoveryPortalButton.onClick.AddListener(OnPortalButtonPressed);

        if (discoveryContinueButton != null)
            discoveryContinueButton.onClick.AddListener(OnContinueButtonPressed);

        if (tooltipUseButton != null)
            tooltipUseButton.onClick.AddListener(OnTooltipUsePressed);

        if (tooltipDropButton != null)
            tooltipDropButton.onClick.AddListener(OnTooltipDropPressed);

        // Item screen
        _portalInputAction.performed -= PortalInputAction_performed;
        _backInputAction.performed -= BackInputAction_performed;

        _portalInputAction.performed += PortalInputAction_performed;
        _backInputAction.performed += BackInputAction_performed;
    }

    private void OnDisable()
    {
        _portalInputAction.performed -= PortalInputAction_performed;
        _backInputAction.performed -= BackInputAction_performed;
    }

    private void BackInputAction_performed(InputAction.CallbackContext obj)
    {
        // Close Item Popup
        OnItemContinueButtonPressed();
    }

    private void PortalInputAction_performed(InputAction.CallbackContext obj)
    {
        ShowMessage("Opening Portal Site");
    }

    void Update()
    {
        // Update compass pulse
        if (isCompassActive)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.PingPong(pulseTimer, 1f);

            if (compassPulseRing != null)
            {
                compassPulseRing.fillAmount = pulse * currentIntensity;
                compassPulseRing.color = new Color(1f, 0.8f, 0.2f, pulse * 0.8f);
            }
        }

        // Toggle pause with Escape
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    TogglePauseMenu();
        //}
    }

    #region Messages

    public void ShowMessage(string message)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(DisplayMessage(message));
    }

    IEnumerator DisplayMessage(string message)
    {
        messagePanel.SetActive(true);
        messageText.text = "<wiggle>" + message;

        yield return new WaitForSeconds(messageDisplayTime);

        messagePanel.SetActive(false);
    }

    public void ShowQuickMessage(string message, float duration = 2f)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(DisplayQuickMessage(message, duration));
    }

    IEnumerator DisplayQuickMessage(string message, float duration)
    {
        messagePanel.SetActive(true);
        messageText.text = message;

        yield return new WaitForSeconds(duration);

        messagePanel.SetActive(false);
    }

    #endregion

    #region Audio Compass

    public void UpdateCompass(float intensity)
    {
        currentIntensity = Mathf.Clamp01(intensity);

        if (compassIntensityBar != null)
        {
            compassIntensityBar.fillAmount = currentIntensity;
        }

        if (!isCompassActive && currentIntensity > 0.1f)
        {
            isCompassActive = true;
            compassPanel.SetActive(true);
        }
        else if (isCompassActive && currentIntensity < 0.05f)
        {
            isCompassActive = false;
            compassPanel.SetActive(false);
        }
    }

    public void SetCompassDirection(Vector3 directionToArtifact)
    {
        if (compassArrow == null) return;

        // Convert direction to angle
        float angle = Mathf.Atan2(directionToArtifact.x, directionToArtifact.z) * Mathf.Rad2Deg;
        compassArrow.rotation = Quaternion.Euler(0, 0, -angle);
    }

    #endregion

    #region Fuel Display

    public void ShowFuelDisplay()
    {
        fuelDisplay.SetActive(true);
    }

    public void UpdateFuelDisplay(float fuelPercentage, float currentFuel, float maxFuel)
    {
        if (fuelSlider != null)
            fuelSlider.value = fuelPercentage;

        if (fuelFillImage != null)
            fuelFillImage.color = fuelGradient.Evaluate(fuelPercentage);

        if (fuelText != null)
            fuelText.text = $"{Mathf.CeilToInt(currentFuel)} / {maxFuel}";

        if (lowFuelWarning != null)
            lowFuelWarning.SetActive(fuelPercentage < 0.2f);
    }

    #endregion

    #region Quest Display

    public void ShowQuestDisplay()
    {
        questDisplay.SetActive(true);
        ToggleInventory();
    }

    public void UpdateQuestText(int memoryStonesActivated, int totalMemoryStones)
    {
        string title = "Activate Memory Stones ";
        questText.text = $"{title} ({memoryStonesActivated}/{totalMemoryStones})";
    }

    #endregion

    #region Interaction

    public void ShowInteractPrompt(string prompt, bool show)
    {
        interactPanel.SetActive(show);
        if (show && interactPromptText != null)
            interactPromptText.text = prompt;
    }

    #endregion

    #region Discovery Screen

    public void ShowDiscoveryScreen(string artifactName, Sprite artifactImage, string loreText, string historicalFact)
    {
        discoveryScreenPanel.SetActive(true);

        if (discoveryArtifactName != null)
            discoveryArtifactName.text = artifactName;

        if (discoveryArtifactImage != null)
            discoveryArtifactImage.sprite = artifactImage;

        if (discoveryLoreText != null)
            discoveryLoreText.text = loreText;

        if (discoveryHistoricalFact != null)
            discoveryHistoricalFact.text = historicalFact;

        // Show cursor
        Cursor.visible = true;

        // Pause game
        Time.timeScale = 0f;
    }

    void OnPortalButtonPressed()
    {
        // Call Museum API
        MuseumAPIClient.Instance.SaveArtifact();

        // Show portal notification
        ShowPortalNotification("✅ Artifact Saved to Digital Museum!");

        // Disable button after click
        discoveryPortalButton.interactable = false;
        discoveryPortalButton.GetComponentInChildren<Text>().text = "✓ Saved";
    }

    void OnContinueButtonPressed()
    {
        discoveryScreenPanel.SetActive(false);

        // Resume game
        Time.timeScale = 1f;
        Cursor.visible = false;

        // Show completion message
        ShowMessage("You have recovered the Baybayin Stone!");
    }

    public void ShowItemScreen(InventoryItem item)
    {
        itemScreenPanel.SetActive(true);

        _portalInputAction.Enable();
        _backInputAction.Enable();

        itemName.text = item.itemName;
        itemDiscoverdName.text = item.itemName;
        itemImage.sprite = item.icon;
        itemCulturalText.text = item.culturalOrigin;
        itemDetailText.text = item.description;
        itemEffectText.text = item.effectDescription;

        // Show cursor
        Cursor.visible = true;

        // Pause game
        Time.timeScale = 0f;
    }

    void OnItemContinueButtonPressed()
    {
        _portalInputAction.Disable();
        _backInputAction.Disable();

        itemScreenPanel.SetActive(false);

        // Resume game
        Time.timeScale = 1f;
        Cursor.visible = false;

        // Show completion message
        //ShowMessage("You have recovered the Baybayin Stone!\nThe legacy of Tawalisi lives on.");
    }

    #endregion

    #region Portal Notification

    public void ShowPortalNotification(string message)
    {
        if (portalNotificationCoroutine != null)
            StopCoroutine(portalNotificationCoroutine);

        portalNotificationCoroutine = StartCoroutine(DisplayPortalNotification(message));
    }

    IEnumerator DisplayPortalNotification(string message)
    {
        portalNotificationPanel.SetActive(true);
        portalNotificationText.text = message;

        // Play animation
        portalNotificationPanel.transform.localScale = Vector3.zero;
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1f, elapsed / duration);
            portalNotificationPanel.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        portalNotificationPanel.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(portalNotificationDuration);

        // Close animation
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, elapsed / duration);
            portalNotificationPanel.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        portalNotificationPanel.SetActive(false);
        portalNotificationPanel.transform.localScale = Vector3.one;
    }

    #endregion

    #region Inventory UI

    public void UpdateItemSlotCounter(int slotIndex, int count)
    {
        itemSlotCountTexts[slotIndex].gameObject.SetActive(count > 0);
        itemSlotCountTexts[slotIndex].text = count.ToString("D2");
    }

    public void UpdateInventoryUI(InventoryItem[] items, int emberlightAmount)
    {
        // Update item slots
        for (int i = 0; i < itemSlotImages.Length; i++)
        {
            if (i < items.Length && items[i] != null)
            {
                itemSlotImages[i].gameObject.SetActive(true);
                itemSlotImages[i].sprite = items[i].icon;

                itemSlotImages[i].color = items[i].isActive ? Color.white : Color.darkKhaki;

                //if (items[i].isStackable && items[i].currentStack > 1)
                //{
                //    itemSlotCountTexts[i].text = items[i].currentStack.ToString();
                //    itemSlotCountTexts[i].gameObject.SetActive(true);
                //}
                //else
                //{
                //    itemSlotCountTexts[i].gameObject.SetActive(false);
                //}
            }
            else
            {
                itemSlotImages[i].sprite = null;
                itemSlotImages[i].color = new Color(0, 0, 0, 0);
                itemSlotCountTexts[i].gameObject.SetActive(false);
            }
        }

        // Update emberlight slot
        if (emberlightAmount > 0)
        {
            emberlightSlotCountText.text = emberlightAmount.ToString("D2");
            emberlightSlotCountText.gameObject.SetActive(true);
        }
        else
        {
            emberlightSlotCountText.gameObject.SetActive(false);
        }
    }

    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    #endregion

    #region Item Tooltip

    public void ShowTooltip(InventoryItem item, int slotIndex)
    {
        tooltipPanel.SetActive(true);
        tooltipNameText.text = item.itemName;
        tooltipOriginText.text = "Origin: " + item.culturalOrigin;
        tooltipDescriptionText.text = item.description;
        tooltipEffectText.text = "⚡ " + item.effectDescription;

        // Store slot index for use/drop buttons
        tooltipPanel.GetComponent<TooltipData>().slotIndex = slotIndex;
    }

    public void CloseTooltip()
    {
        tooltipPanel.SetActive(false);
    }

    void OnTooltipUsePressed()
    {
        TooltipData data = tooltipPanel.GetComponent<TooltipData>();
        if (data != null)
        {
            InventoryManager.Instance.UseItem(data.slotIndex);
            CloseTooltip();
        }
    }

    void OnTooltipDropPressed()
    {
        TooltipData data = tooltipPanel.GetComponent<TooltipData>();
        if (data != null)
        {
            InventoryManager.Instance.DropItem(data.slotIndex);
            CloseTooltip();
        }
    }

    #endregion

    #region Pause Menu

    public void TogglePauseMenu()
    {
        bool isPaused = pauseMenuPanel.activeSelf;
        pauseMenuPanel.SetActive(!isPaused);

        if (!isPaused)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
        }
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    #region Debug

    public void SetDebugText(string text)
    {
        if (showDebug && debugText != null)
        {
            debugText.text = text;
            debugText.gameObject.SetActive(true);
        }
    }

    #endregion
}

// Helper component for tooltip
public class TooltipData : MonoBehaviour
{
    public int slotIndex;
}