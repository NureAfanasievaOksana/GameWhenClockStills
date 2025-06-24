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
    public Canvas blockingCanvas; // Canvas ���� ���� ��������� �����䳿
    public Image blockingImage;   // ������������ ��� ��� ����������

    // ���������� ��� ����������
    private PlayerController playerMovement;
    private ItemTooltipController tooltipController;
    private GraphicRaycaster[] allRaycasters;
    private bool[] raycastersOriginalState;

    // Canvas �� ������ ������ ������������
    private Canvas confirmationCanvas;

    // ��������� ���� ��� ����������� ��������
    public static bool IsConfirmationActive = false;

    void Start()
    {
        // ����������� ���������� ����
        confirmationPanel.SetActive(false);
        SetupBlockingCanvas();

        // ��������� Canvas ����� ������������
        confirmationCanvas = confirmationPanel.GetComponentInParent<Canvas>();

        // ��������� ���������� ��� ����������
        FindComponentsToBlock();

        // ����������� ��������� ����
        SetupEventHandlers();
    }

    private void SetupBlockingCanvas()
    {
        if (blockingCanvas == null)
        {
            // ��������� Canvas ��� ����������, ���� �� �� �����������
            GameObject blockingCanvasGO = new GameObject("BlockingCanvas");
            blockingCanvas = blockingCanvasGO.AddComponent<Canvas>();
            blockingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            blockingCanvas.sortingOrder = 1000; // ���� ������� ������� ����������

            // ������ GraphicRaycaster ��� ������������ ����
            blockingCanvasGO.AddComponent<GraphicRaycaster>();

            // ��������� ������������ ���
            GameObject blockingImageGO = new GameObject("BlockingImage");
            blockingImageGO.transform.SetParent(blockingCanvas.transform);
            blockingImage = blockingImageGO.AddComponent<Image>();
            blockingImage.color = new Color(0, 0, 0, 0.5f); // ������������ ������

            // ��������� �� ���� �����
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
        // ��������� ��������� ���� ������
        playerMovement = FindAnyObjectByType<PlayerController>();

        // ��������� ��������� tooltip
        tooltipController = FindAnyObjectByType<ItemTooltipController>();

        // ��������� �� GraphicRaycaster ��� ���������� UI �������
        allRaycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        raycastersOriginalState = new bool[allRaycasters.Length];

        // �������� ���������� ����
        for (int i = 0; i < allRaycasters.Length; i++)
        {
            raycastersOriginalState[i] = allRaycasters[i].enabled;
        }
    }

    private void SetupEventHandlers()
    {
        // �������� ���� ������������ ��� ��������� "�����"
        exitButton.onClick.AddListener(() =>
        {
            ShowConfirmation();
        });

        // ������ "����������"
        cancelExitButton.onClick.AddListener(() =>
        {
            HideConfirmation();
        });

        // ������ "�����"
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
        // �������� ������ ������������
        confirmationPanel.SetActive(true);

        // �������� ��������� Canvas
        blockingCanvas.gameObject.SetActive(true);

        // ������������ ���������� ������� Canvas-��
        if (confirmationCanvas != null)
        {
            confirmationCanvas.sortingOrder = blockingCanvas.sortingOrder + 1;
        }

        // ������� �� �����䳿
        BlockAllInteractions();

        // ������������ ���������� ����
        IsConfirmationActive = true;
    }

    private void HideConfirmation()
    {
        // ���������� ������ ������������
        confirmationPanel.SetActive(false);

        // ���������� ��������� Canvas
        blockingCanvas.gameObject.SetActive(false);

        // ������������ �� �����䳿
        UnblockAllInteractions();

        // ������� ���������� ����
        IsConfirmationActive = false;
    }

    private void BlockAllInteractions()
    {
        // ������� ��� ������
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // ������ tooltip ���� �� ��������
        if (tooltipController != null)
        {
            tooltipController.HideTooltip();
        }

        // ������� UI �����䳿 (��� ������ confirmation panel �� blocking canvas)
        for (int i = 0; i < allRaycasters.Length; i++)
        {
            if (allRaycasters[i] != null && ShouldBlockRaycaster(allRaycasters[i]))
            {
                allRaycasters[i].enabled = false;
            }
        }

        // �� ������� ���, ��� UI ������� ���������
        // Time.timeScale = 0f; // �������������
    }

    private void UnblockAllInteractions()
    {
        // ������������ ��� ������
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // ³��������� UI �����䳿
        for (int i = 0; i < allRaycasters.Length; i++)
        {
            if (allRaycasters[i] != null)
            {
                allRaycasters[i].enabled = raycastersOriginalState[i];
            }
        }

        // ³��������� ��� (���� ��� ���������)
        // Time.timeScale = 1f; // �������������
    }

    private bool ShouldBlockRaycaster(GraphicRaycaster raycaster)
    {
        // �� ������� raycaster ���� �� �������� ��:
        // 1. Blocking Canvas
        if (raycaster.gameObject == blockingCanvas.gameObject)
            return false;

        // 2. Canvas � ������� ������������
        if (confirmationCanvas != null && raycaster.gameObject == confirmationCanvas.gameObject)
            return false;

        // 3. ������ ��'���� ����� ������������
        if (IsChildOfConfirmationPanel(raycaster.gameObject))
            return false;

        // ������� �� ����
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
        // ������� ESC �� ���� ������ ���� ������� ����� ������������
        if (IsConfirmationActive)
        {
            // ����� ������ ������� �����, ��������� ESC ��� ��������
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideConfirmation();
            }
        }
    }

    void OnDestroy()
    {
        // ³��������� ���� ��� ������� ��'����
        if (IsConfirmationActive)
        {
            UnblockAllInteractions();
            IsConfirmationActive = false;
        }
    }
}