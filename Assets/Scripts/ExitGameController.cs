using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExitGameController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject confirmationPanel;
    public Button exitButton;
    public Button confirmExitButton;
    public Button cancelExitButton;

    [Header("Blocking Settings")]
    public Canvas blockingCanvas;
    public Image blockingImage;

    private PlayerController playerMovement;
    private ItemTooltipController tooltipController;
    private GraphicRaycaster[] allRaycasters;
    private bool[] raycastersOriginalState;

    private Canvas confirmationCanvas;

    public static bool IsConfirmationActive = false;

    void Start()
    {
        confirmationPanel.SetActive(false);
        SetupBlockingCanvas();

        confirmationCanvas = confirmationPanel.GetComponentInParent<Canvas>();

        FindComponentsToBlock();

        SetupEventHandlers();
    }

    private void SetupBlockingCanvas()
    {
        if (blockingCanvas == null)
        {
            GameObject blockingCanvasGO = new GameObject("BlockingCanvas");
            blockingCanvas = blockingCanvasGO.AddComponent<Canvas>();
            blockingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            blockingCanvas.sortingOrder = 1000;

            blockingCanvasGO.AddComponent<GraphicRaycaster>();

            GameObject blockingImageGO = new GameObject("BlockingImage");
            blockingImageGO.transform.SetParent(blockingCanvas.transform);
            blockingImage = blockingImageGO.AddComponent<Image>();
            blockingImage.color = new Color(0, 0, 0, 0.5f);

            RectTransform blockingRect = blockingImage.GetComponent<RectTransform>();
            blockingRect.anchorMin = Vector2.zero;
            blockingRect.anchorMax = Vector2.one;
            blockingRect.offsetMin = Vector2.zero;
            blockingRect.offsetMax = Vector2.zero;
        }

        blockingCanvas.gameObject.SetActive(false);
    }

    private void FindComponentsToBlock()
    {
        playerMovement = FindAnyObjectByType<PlayerController>();

        tooltipController = FindAnyObjectByType<ItemTooltipController>();

        allRaycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        raycastersOriginalState = new bool[allRaycasters.Length];

        for (int i = 0; i < allRaycasters.Length; i++)
        {
            raycastersOriginalState[i] = allRaycasters[i].enabled;
        }
    }

    private void SetupEventHandlers()
    {
        exitButton.onClick.AddListener(() =>
        {
            ShowConfirmation();
        });

        cancelExitButton.onClick.AddListener(() =>
        {
            HideConfirmation();
        });

        confirmExitButton.onClick.AddListener(() =>
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        });
    }

    private void ShowConfirmation()
    {
        confirmationPanel.SetActive(true);
        blockingCanvas.gameObject.SetActive(true);

        if (confirmationCanvas != null)
        {
            confirmationCanvas.sortingOrder = blockingCanvas.sortingOrder + 1;
        }

        BlockAllInteractions();

        IsConfirmationActive = true;
    }

    private void HideConfirmation()
    {
        confirmationPanel.SetActive(false);
        blockingCanvas.gameObject.SetActive(false);

        UnblockAllInteractions();

        IsConfirmationActive = false;
    }

    private void BlockAllInteractions()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (tooltipController != null)
        {
            tooltipController.HideTooltip();
        }

        for (int i = 0; i < allRaycasters.Length; i++)
        {
            if (allRaycasters[i] != null && ShouldBlockRaycaster(allRaycasters[i]))
            {
                allRaycasters[i].enabled = false;
            }
        }
    }

    private void UnblockAllInteractions()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        for (int i = 0; i < allRaycasters.Length; i++)
        {
            if (allRaycasters[i] != null)
            {
                allRaycasters[i].enabled = raycastersOriginalState[i];
            }
        }
    }

    private bool ShouldBlockRaycaster(GraphicRaycaster raycaster)
    {
        if (raycaster.gameObject == blockingCanvas.gameObject)
            return false;

        if (confirmationCanvas != null && raycaster.gameObject == confirmationCanvas.gameObject)
            return false;

        if (IsChildOfConfirmationPanel(raycaster.gameObject))
            return false;

        return true;
    }

    private bool IsChildOfConfirmationPanel(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.gameObject == confirmationPanel)
                return true;
            current = current.parent;
        }
        return false;
    }

    void Update()
    {
        if (IsConfirmationActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideConfirmation();
            }
        }
    }

    void OnDestroy()
    {
        if (IsConfirmationActive)
        {
            UnblockAllInteractions();
            IsConfirmationActive = false;
        }
    }
}