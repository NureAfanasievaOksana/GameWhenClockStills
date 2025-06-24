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
    public Canvas blockingCanvas; // Canvas який буде блокувати взаємодії
    public Image blockingImage;   // Напівпрозорий фон для блокування

    // Компоненти для блокування
    private PlayerController playerMovement;
    private ItemTooltipController tooltipController;
    private GraphicRaycaster[] allRaycasters;
    private bool[] raycastersOriginalState;

    // Canvas що містить панель підтвердження
    private Canvas confirmationCanvas;

    // Статичний флаг для глобального контролю
    public static bool IsConfirmationActive = false;

    void Start()
    {
        // Налаштовуємо початковий стан
        confirmationPanel.SetActive(false);
        SetupBlockingCanvas();

        // Знаходимо Canvas панелі підтвердження
        confirmationCanvas = confirmationPanel.GetComponentInParent<Canvas>();

        // Знаходимо компоненти для блокування
        FindComponentsToBlock();

        // Налаштовуємо обробники подій
        SetupEventHandlers();
    }

    private void SetupBlockingCanvas()
    {
        if (blockingCanvas == null)
        {
            // Створюємо Canvas для блокування, якщо він не призначений
            GameObject blockingCanvasGO = new GameObject("BlockingCanvas");
            blockingCanvas = blockingCanvasGO.AddComponent<Canvas>();
            blockingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            blockingCanvas.sortingOrder = 1000; // Дуже високий порядок сортування

            // Додаємо GraphicRaycaster для перехоплення кліків
            blockingCanvasGO.AddComponent<GraphicRaycaster>();

            // Створюємо напівпрозорий фон
            GameObject blockingImageGO = new GameObject("BlockingImage");
            blockingImageGO.transform.SetParent(blockingCanvas.transform);
            blockingImage = blockingImageGO.AddComponent<Image>();
            blockingImage.color = new Color(0, 0, 0, 0.5f); // Напівпрозорий чорний

            // Розтягуємо на весь екран
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
        // Знаходимо компонент руху гравця
        playerMovement = FindAnyObjectByType<PlayerController>();

        // Знаходимо контролер tooltip
        tooltipController = FindAnyObjectByType<ItemTooltipController>();

        // Знаходимо всі GraphicRaycaster для блокування UI взаємодій
        allRaycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        raycastersOriginalState = new bool[allRaycasters.Length];

        // Зберігаємо початковий стан
        for (int i = 0; i < allRaycasters.Length; i++)
        {
            raycastersOriginalState[i] = allRaycasters[i].enabled;
        }
    }

    private void SetupEventHandlers()
    {
        // Показати вікно підтвердження при натисканні "вийти"
        exitButton.onClick.AddListener(() =>
        {
            ShowConfirmation();
        });

        // Кнопка "Залишитися"
        cancelExitButton.onClick.AddListener(() =>
        {
            HideConfirmation();
        });

        // Кнопка "Вийти"
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
        // Активуємо панель підтвердження
        confirmationPanel.SetActive(true);

        // Активуємо блокуючий Canvas
        blockingCanvas.gameObject.SetActive(true);

        // Встановлюємо правильний порядок Canvas-ів
        if (confirmationCanvas != null)
        {
            confirmationCanvas.sortingOrder = blockingCanvas.sortingOrder + 1;
        }

        // Блокуємо всі взаємодії
        BlockAllInteractions();

        // Встановлюємо глобальний флаг
        IsConfirmationActive = true;
    }

    private void HideConfirmation()
    {
        // Деактивуємо панель підтвердження
        confirmationPanel.SetActive(false);

        // Деактивуємо блокуючий Canvas
        blockingCanvas.gameObject.SetActive(false);

        // Розблоковуємо всі взаємодії
        UnblockAllInteractions();

        // Скидаємо глобальний флаг
        IsConfirmationActive = false;
    }

    private void BlockAllInteractions()
    {
        // Блокуємо рух гравця
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Ховаємо tooltip якщо він активний
        if (tooltipController != null)
        {
            tooltipController.HideTooltip();
        }

        // Блокуємо UI взаємодії (крім нашого confirmation panel та blocking canvas)
        for (int i = 0; i < allRaycasters.Length; i++)
        {
            if (allRaycasters[i] != null && ShouldBlockRaycaster(allRaycasters[i]))
            {
                allRaycasters[i].enabled = false;
            }
        }

        // НЕ блокуємо час, щоб UI анімації працювали
        // Time.timeScale = 0f; // Закоментовано
    }

    private void UnblockAllInteractions()
    {
        // Розблоковуємо рух гравця
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Відновлюємо UI взаємодії
        for (int i = 0; i < allRaycasters.Length; i++)
        {
            if (allRaycasters[i] != null)
            {
                allRaycasters[i].enabled = raycastersOriginalState[i];
            }
        }

        // Відновлюємо час (якщо був зупинений)
        // Time.timeScale = 1f; // Закоментовано
    }

    private bool ShouldBlockRaycaster(GraphicRaycaster raycaster)
    {
        // НЕ блокуємо raycaster якщо він належить до:
        // 1. Blocking Canvas
        if (raycaster.gameObject == blockingCanvas.gameObject)
            return false;

        // 2. Canvas з панеллю підтвердження
        if (confirmationCanvas != null && raycaster.gameObject == confirmationCanvas.gameObject)
            return false;

        // 3. Дочірні об'єкти панелі підтвердження
        if (IsChildOfConfirmationPanel(raycaster.gameObject))
            return false;

        // Блокуємо всі інші
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
        // Блокуємо ESC та інші клавіші коли активна форма підтвердження
        if (IsConfirmationActive)
        {
            // Можна додати обробку клавіш, наприклад ESC для закриття
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideConfirmation();
            }
        }
    }

    void OnDestroy()
    {
        // Відновлюємо стан при знищенні об'єкта
        if (IsConfirmationActive)
        {
            UnblockAllInteractions();
            IsConfirmationActive = false;
        }
    }
}