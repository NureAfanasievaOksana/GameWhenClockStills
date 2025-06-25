using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CommentManager : MonoBehaviour
{
    public static CommentManager Instance;
    public GameObject pocketClock;

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button closeButton;

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        closeButton.onClick.AddListener(HideDialogue);
    }

    public void ShowMessage(string description)
    {
        pocketClock.SetActive(false);
        dialogueText.text = description;
        dialoguePanel.SetActive(true);
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        pocketClock.SetActive(true);
    }
}
