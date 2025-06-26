using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GearPuzzleController : MonoBehaviour
{
    [System.Serializable]
    public class GearSlot
    {
        public Transform slotTransform;
        public string correctGearId;
        [HideInInspector] public Gear currentGear;
        public bool IsCorrect => currentGear != null && currentGear.gearId == correctGearId;
    }

    [Header("Settings")]
    public GearSlot[] slots;
    public float snapDistance = 1f;
    public string nextScene = "PastLibrary";

    private Gear draggedGear;
    private Vector3 dragOffset;
    private bool puzzleSolved;

    void Update()
    {
        if (puzzleSolved) return;

        HandleDragInput();

        if (Input.GetMouseButtonUp(0))
        {
            CheckPuzzleSolution();
        }
    }

    void HandleDragInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag();
        }

        if (draggedGear != null && Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            draggedGear.transform.position = new Vector3(mousePos.x + dragOffset.x, mousePos.y + dragOffset.y, 0);
        }

        if (draggedGear != null && Input.GetMouseButtonUp(0))
        {
            TrySnapToSlot();
            draggedGear = null;
        }
    }

    void TryStartDrag()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Gear"))
        {
            draggedGear = hit.collider.GetComponent<Gear>();
            dragOffset = draggedGear.transform.position - (Vector3)mousePos;

            if (draggedGear.currentSlot != null)
            {
                draggedGear.currentSlot.currentGear = null;
                draggedGear.currentSlot = null;
            }
        }
    }

    void TrySnapToSlot()
    {
        if (draggedGear == null) return;

        GearSlot closestSlot = null;
        float closestDistance = float.MaxValue;

        foreach (var slot in slots)
        {
            if (slot.currentGear != null) continue;

            float distance = Vector3.Distance(draggedGear.transform.position, slot.slotTransform.position);
            if (distance < snapDistance && distance < closestDistance)
            {
                closestDistance = distance;
                closestSlot = slot;
            }
        }

        if (closestSlot != null)
        {
            draggedGear.transform.position = closestSlot.slotTransform.position;
            closestSlot.currentGear = draggedGear;
            draggedGear.currentSlot = closestSlot;
        }
    }

    void CheckPuzzleSolution()
    {

        foreach (var slot in slots)
        {
            if (slot.currentGear == null || slot.currentGear.gearId != slot.correctGearId)
            {
                return;
            }
        }

        puzzleSolved = true;
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextScene))
        {
            StartCoroutine(HandleSceneTransition(nextScene));
        }
        else
        {
            Debug.LogError("Next scene name is not set!");
        }
    }

    private IEnumerator HandleSceneTransition(string targetScene)
    {
        if (SceneTransitionManager.Instance != null)
        {
            yield return StartCoroutine(SceneTransitionManager.Instance.FadeOutCoroutine());
        }
        else
        {
            Debug.LogWarning("SceneTransitionManager not found, loading immediately");
            yield return null;
        }

        SceneManager.LoadScene(targetScene);
    }
}