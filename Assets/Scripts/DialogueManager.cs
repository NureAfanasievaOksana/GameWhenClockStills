using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private float padding = 20f;
    [SerializeField] private float minButtonWidth = 200f;
    [SerializeField] private float maxButtonWidth = 800f;
    public GameObject pocketClock;

    [Header("Blocking UI")]
    private Canvas blockingCanvas;
    private Image blockingImage;
    private GraphicRaycaster[] raycasters;
    private bool[] raycasterStates;
    private PlayerController playerController;
    private ItemTooltipController tooltipController;

    private DialoguesDatabase dialoguesDatabase;
    private GameState gameState;
    private float maxOptionWidth = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        InitializeManager();
        SetupBlockingCanvas();
        FindBlockableComponents();
    }

    private void InitializeManager()
    {
        dialoguesDatabase = DialoguesDatabase.LoadFromResources();
        if (dialoguesDatabase == null)
        {
            Debug.LogError("Failed to load dialogues database!");
            enabled = false;
            return;
        }

        if (DataManager.Instance != null)
        {
            gameState = DataManager.Instance.currentGameState;
        }

        if (dialoguePanel == null) Debug.LogError("Dialogue Panel is not assigned!");
        if (dialogueText == null) Debug.LogError("Dialogue Text is not assigned!");
        if (optionButtons == null || optionButtons.Length == 0) Debug.LogError("Option Buttons are not assigned!");
    }

    private void SetupBlockingCanvas()
    {
        GameObject canvasGO = new GameObject("DialogueBlockingCanvas");
        blockingCanvas = canvasGO.AddComponent<Canvas>();
        blockingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        blockingCanvas.sortingOrder = 999;

        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject imageGO = new GameObject("BackgroundBlocker");
        imageGO.transform.SetParent(canvasGO.transform);
        blockingImage = imageGO.AddComponent<Image>();
        blockingImage.color = new Color(0, 0, 0, 0.5f);

        RectTransform rt = blockingImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        blockingCanvas.gameObject.SetActive(false);
    }

    private void FindBlockableComponents()
    {
        playerController = FindAnyObjectByType<PlayerController>();
        tooltipController = FindAnyObjectByType<ItemTooltipController>();
        raycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        raycasterStates = raycasters.Select(r => r.enabled).ToArray();
    }

    public void StartDialogue(string dialogueId)
    {
        if (gameState == null)
        {
            if (DataManager.Instance != null)
                gameState = DataManager.Instance.currentGameState;

            if (gameState == null)
            {
                Debug.LogError("GameState is not initialized!");
                return;
            }
        }

        DialogueData dialogue = dialoguesDatabase.GetDialogueById(dialogueId);
        if (dialogue == null || !CheckDialogueConditions(dialogue.required_flag))
        {
            Debug.Log($"Dialogue '{dialogueId}' not found or condition not met.");
            return;
        }

        CalculateMaxOptionWidth(dialogue);
        SetupDialogueUI(dialogue);
        ShowBlockingLayer();
    }

    private void CalculateMaxOptionWidth(DialogueData dialogue)
    {
        maxOptionWidth = minButtonWidth;
        TextMeshProUGUI refText = optionButtons[0].GetComponentInChildren<TextMeshProUGUI>();

        GameObject tempObj = new GameObject("TempText");
        var tempText = tempObj.AddComponent<TextMeshProUGUI>();
        tempText.font = refText.font;
        tempText.fontSize = refText.fontSize;
        tempText.fontStyle = refText.fontStyle;
        tempText.textWrappingMode = TextWrappingModes.NoWrap;
        tempText.alignment = refText.alignment;

        try
        {
            foreach (var opt in dialogue.options)
            {
                tempText.text = opt.text;
                tempText.ForceMeshUpdate();
                float width = tempText.preferredWidth;
                if (width > maxOptionWidth)
                    maxOptionWidth = width;
            }

            maxOptionWidth = Mathf.Clamp(maxOptionWidth + padding * 2, minButtonWidth, maxButtonWidth);
        }
        finally
        {
            Destroy(tempObj);
        }
    }

    private void SetupDialogueUI(DialogueData dialogue)
    {
        pocketClock.SetActive(false);
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogue.start_text;

        foreach (var btn in optionButtons) btn.gameObject.SetActive(false);

        for (int i = 0; i < dialogue.options.Count && i < optionButtons.Length; i++)
        {
            SetupOptionButton(optionButtons[i], dialogue.options[i]);
        }
    }

    private void SetupOptionButton(Button button, DialogueOption option)
    {
        button.gameObject.SetActive(true);
        var txt = button.GetComponentInChildren<TextMeshProUGUI>();
        txt.text = option.text;
        txt.textWrappingMode = TextWrappingModes.NoWrap;

        button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxOptionWidth);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnOptionSelected(option));
    }

    private bool CheckDialogueConditions(string requiredFlag)
    {
        if (string.IsNullOrEmpty(requiredFlag)) return true;

        string[] parts = requiredFlag.Split(' ');
        if (parts.Length != 2) return false;

        string flagName = parts[0];
        bool expected = parts[1].ToLower() == "true";

        var field = gameState.player.progress_flags.GetType().GetField(flagName);
        if (field == null) return false;

        return (bool)field.GetValue(gameState.player.progress_flags) == expected;
    }

    private void OnOptionSelected(DialogueOption option)
    {
        bool shouldAddLabCode = false;

        if (!string.IsNullOrEmpty(option.set_flag))
        {
            string[] parts = option.set_flag.Split(' ');
            if (parts.Length == 2 && parts[0] == "get_code" && parts[1].ToLower() == "true")
            {
                shouldAddLabCode = true;
            }
        }

        SetProgressFlag(option.set_flag);

        if (shouldAddLabCode)
        {
            AddLabCodeToInventory();
        }

        if (string.IsNullOrEmpty(option.next_dialogue))
        {
            EndDialogue();
        }
        else
        {
            StartDialogue(option.next_dialogue);
        }
    }

    private void AddLabCodeToInventory()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItemToInventory("LabCode", "Код доступу до лабораторії");
            Debug.Log("LabCode added to inventory");
        }
        else
        {
            Debug.LogError("InventoryManager is not available!");
        }
    }

    private void SetProgressFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;

        string[] parts = flag.Split(' ');
        if (parts.Length != 2) return;

        string name = parts[0];
        bool value = parts[1].ToLower() == "true";

        var field = gameState.player.progress_flags.GetType().GetField(name);
        if (field != null)
        {
            field.SetValue(gameState.player.progress_flags, value);
            DataManager.Instance.SaveGameState();
        }
    }

    private void ShowBlockingLayer()
    {
        blockingCanvas.gameObject.SetActive(true);
        dialoguePanel.transform.SetParent(blockingCanvas.transform, false);

        if (playerController != null)
            playerController.enabled = false;

        if (tooltipController != null)
            tooltipController.HideTooltip();

        for (int i = 0; i < raycasters.Length; i++)
        {
            if (raycasters[i] != null)
                raycasters[i].enabled = false;
        }
    }

    private void EndDialogue()
    {
        pocketClock.SetActive(true);
        dialoguePanel.SetActive(false);
        blockingCanvas.gameObject.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;

        for (int i = 0; i < raycasters.Length; i++)
        {
            if (raycasters[i] != null)
                raycasters[i].enabled = raycasterStates[i];
        }
    }

    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeInHierarchy;
    }
}