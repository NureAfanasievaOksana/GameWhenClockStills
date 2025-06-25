using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltipController : MonoBehaviour
{
    public GameObject tooltipPanel;
    public TMP_Text tooltipText;
    public Image backgroundImage;
    public Vector2 padding = new Vector2(10, 50);
    public float edgeMargin = 10f;

    private RectTransform backgroundRect;
    private Camera mainCamera;
    private Canvas parentCanvas;
    private float scaleFactor = 1f;

    void Start()
    {
        mainCamera = Camera.main;
        tooltipPanel.SetActive(false);
        backgroundRect = backgroundImage.GetComponent<RectTransform>();

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            scaleFactor = parentCanvas.scaleFactor;
        }
    }

    void Update()
    {
        if (!tooltipPanel.activeSelf) return;

        Vector2 mousePos = Input.mousePosition;
        Vector2 tooltipSize = backgroundRect.sizeDelta * scaleFactor;

        Vector2 offset = new Vector2(15f, -15f);

        bool exceedsRight = mousePos.x + tooltipSize.x + offset.x > Screen.width - 150f;
        bool exceedsLeft = mousePos.x + offset.x < 0;
        bool exceedsBottom = mousePos.y + offset.y - tooltipSize.y < 0;
        bool exceedsTop = mousePos.y + offset.y > Screen.height;

        if (exceedsRight)
        {
            offset.x = -tooltipSize.x - 185f;
        }
        else if (exceedsLeft)
        {
            offset.x = edgeMargin - mousePos.x;
        }

        if (exceedsBottom)
        {
            offset.y = tooltipSize.y + edgeMargin;
        }
        else if (exceedsTop)
        {
            offset.y = -edgeMargin;
        }

        Vector2 finalPosition = mousePos + offset / scaleFactor;
        tooltipPanel.transform.position = finalPosition;
    }

    public void ShowTooltip(string message)
    {
        if (ExitGameController.IsConfirmationActive)
        {
            return;
        }

        tooltipText.textWrappingMode = TextWrappingModes.Normal;
        tooltipText.text = message;

        Canvas.ForceUpdateCanvases();

        float maxWidth = Mathf.Min(200f, Screen.width * 0.3f);
        Vector2 textSize = tooltipText.GetPreferredValues(message, maxWidth, 0f);

        tooltipText.rectTransform.sizeDelta = new Vector2(maxWidth, textSize.y);
        backgroundRect.sizeDelta = new Vector2(
            maxWidth + padding.x,
            textSize.y + padding.y
        );

        tooltipPanel.SetActive(true);
        Update();
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}