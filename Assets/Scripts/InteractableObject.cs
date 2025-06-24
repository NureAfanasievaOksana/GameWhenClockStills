using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class InteractableObject : MonoBehaviour
{
    public string itemId;
    public float interactionDistance = 1f;

    private ItemsDatabase database;
    private GameState gameState;

    private void Start()
    {
        database = ItemsDatabase.LoadFromResources();
        gameState = DataManager.Instance.currentGameState;
    }

    public Vector2 GetInteractionPoint()
    {
        Collider2D collider = GetComponent<Collider2D>();
        Vector2 closestPoint = collider.ClosestPoint(FindAnyObjectByType<PlayerController>().transform.position);
        return closestPoint + (Vector2)(FindAnyObjectByType<PlayerController>().transform.position - transform.position).normalized * interactionDistance;
    }

    public void Interact()
    {
        ItemData item = database.GetItemById(itemId);
        if (item == null) return;

        // �������� ����� �����䳿 (�������� ��������� ������)
        if (item.is_completed && !item.repeatable)
        {
            DialogueManager.Instance.ShowMessage("�� ������� ��� ���������");
            return;
        }

        // �������� ���� ������������� (�������� ��������� ������)
        if (!CheckUnlockConditions(item.unlock_conditions))
        {
            DialogueManager.Instance.ShowMessage("� �� �� ����, �� � ��� ������...");
            return;
        }

        // �������� ���������� �������� (�������� ��������� ������)
        if (!CheckRequiredItems(item.required_items))
        {
            string missingItems = string.Join(", ", item.required_items.Except(gameState.inventory.items));
            DialogueManager.Instance.ShowMessage($"��� �������: {missingItems}");
            return;
        }

        switch (item.type)
        {
            case "pickup":
                HandlePickup(item);
                break;

            case "inspect":
                HandleInspect(item);
                break;

            case "use":
                HandleUse(item);
                break;

            case "open":
                HandleOpen(item);
                break;

            case "door":
                HandleDoor(item);
                break;

            case "time_device":
                HandleTimeChange(item);
                break;

            default:
                DialogueManager.Instance.ShowMessage(item.description);
                break;
        }

        UpdateGameState(item);
    }

    private bool CheckUnlockConditions(List<string> conditions)
    {
        if (conditions == null || conditions.Count == 0) return true;

        foreach (string condition in conditions)
        {
            var property = gameState.player.progress_flags.GetType().GetField(condition);
            if (property == null || !(bool)property.GetValue(gameState.player.progress_flags))
                return false;
        }
        return true;
    }

    private bool CheckRequiredItems(List<string> requiredItems)
    {
        if (requiredItems == null || requiredItems.Count == 0) return true;
        return !requiredItems.Except(gameState.inventory.items).Any();
    }

    //�������� ��������� ������
    private void HandlePickup(ItemData item)
    {
        gameState.inventory.items.Add(item.item_id);
        item.is_completed = true;
        DialogueManager.Instance.ShowMessage($"� ���� {item.name}");
        gameObject.SetActive(false);
    }

    private void HandleInspect(ItemData item)
    {
        DialogueManager.Instance.ShowMessage(item.description);
    }

    //�������� ��������� ������
    private void HandleUse(ItemData item)
    {
        DialogueManager.Instance.ShowMessage($"����������� {item.name}");
        item.is_completed = true;
    }

    private void HandleOpen(ItemData item)
    {
        DialogueManager.Instance.ShowMessage($"³������ {item.name}");
        item.is_completed = true;
        // ��� ����� ������ ������� ��������
    }

    private void HandleDoor(ItemData item)
    {
        if (!string.IsNullOrEmpty(item.target_location))
        {
            StartCoroutine(HandleSceneTransition(item.target_location));
        }
    }

    private IEnumerator HandleSceneTransition(string targetScene)
    {
        // �������� ����
        gameState.player.previous_location = gameState.player.current_location;
        gameState.player.current_location = targetScene;

        // ��������� FadeOut � ������ ���� ����������
        yield return StartCoroutine(SceneTransitionManager.Instance.FadeOutCoroutine());

        // ���� ���� ���������� - ����������� �����
        SceneManager.LoadScene(targetScene);
    }


    private void HandleTimeChange(ItemData item)
    {
        if (!string.IsNullOrEmpty(item.target_time_period))
        {
            gameState.player.current_time_period = item.target_time_period;
            DialogueManager.Instance.ShowMessage($"��� ������� �� {item.target_time_period}");
            // ��� ����� ������ ����� ���� ����
        }
    }

    private void UpdateGameState(ItemData item)
    {
        if (!string.IsNullOrEmpty(item.progress_flag))
        {
            var flag = gameState.player.progress_flags.GetType().GetField(item.progress_flag);
            if (flag != null) flag.SetValue(gameState.player.progress_flags, true);
        }

        DataManager.Instance.SaveGameState();
    }
}