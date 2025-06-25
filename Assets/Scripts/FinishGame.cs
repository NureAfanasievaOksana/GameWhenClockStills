using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FinishGame : MonoBehaviour
{
    [Header("UI Elements")]
    public Button exitButton;

    void Start()
    {
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        });
    }
}