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
    public GameObject objectToDisplay;

    private ItemsDatabase database;
    private GameState gameState;

    private void Start()
    {
        database = ItemsDatabase.LoadFromResources();
        gameState = DataManager.Instance.currentGameState;

        var allItems = database.GetAllItemsIncludingDisplay(itemId);
        foreach (var item in allItems)
        {
            if (item.type == "display")
            {
                CheckAndUpdateDisplayState(item);
                break;
            }
        }
    }

    public Vector2 GetInteractionPoint()
    {
        Collider2D collider = GetComponent<Collider2D>();
        Vector2 closestPoint = collider.ClosestPoint(FindAnyObjectByType<PlayerController>().transform.position);
        return closestPoint + (Vector2)(FindAnyObjectByType<PlayerController>().transform.position - transform.position).normalized * interactionDistance;
    }

    public void Interact()
    {
        string selectedItem = InventoryManager.Instance.GetSelectedItem();

        if (selectedItem != null)
        {
            List<ItemData> possibleInteractions = database.GetAllItemsById(itemId);
            if (possibleInteractions != null)
            {
                foreach (ItemData interaction in possibleInteractions)
                {
                    if (interaction.required_items != null && interaction.required_items.Contains(selectedItem))
                    {
                        if (InventoryManager.Instance.UseSelectedItem(selectedItem))
                        {
                            ExecuteInteraction(interaction);
                            return;
                        }
                    }
                }
            }

            CommentManager.Instance.ShowMessage("Цей предмет тут не потрібен.");
            return;
        }

        List<ItemData> allInteractions = database.GetAllItemsById(itemId);
        if (allInteractions == null || allInteractions.Count == 0) return;

        ItemData validInteraction = FindValidInteraction(allInteractions);
        if (validInteraction == null)
        {
            CommentManager.Instance.ShowMessage("Я ще не знаю, що з цим робити...");
            return;
        }

        ExecuteInteraction(validInteraction);
    }

    private ItemData FindValidInteraction(List<ItemData> possibleInteractions)
    {
        foreach (ItemData interaction in possibleInteractions)
        {
            if (interaction.is_completed && !interaction.repeatable) continue;

            if (!CheckUnlockConditions(interaction.unlock_conditions)) continue;

            if (!CheckRequiredItems(interaction.required_items)) continue;

            return interaction;
        }
        return null;
    }

    private void ExecuteInteraction(ItemData item)
    {
        if (item.type == "display")
        {
            Debug.LogWarning("Display items should not be processed in ExecuteInteraction");
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
            case "door":
                HandleDoor(item);
                break;
            case "time_device":
                HandleTimeChange(item);
                break;
            case "dialogue":
                HandleDialogue(item);
                break;
            default:
                CommentManager.Instance.ShowMessage(item.description);
                break;
        }

        UpdateGameState(item);
    }

    private bool CheckUnlockConditions(List<string> conditions)
    {
        if (conditions == null || conditions.Count == 0) return true;

        foreach (string condition in conditions)
        {
            var parts = condition.Split(' ');
            if (parts.Length != 2)
            {
                Debug.LogWarning($"Invalid unlock condition format: {condition}");
                return false;
            }

            string flagName = parts[0];
            string expectedValue = parts[1];

            var field = gameState.player.progress_flags.GetType().GetField(flagName);
            if (field == null)
            {
                Debug.LogWarning($"Progress flag '{flagName}' not found");
                return false;
            }

            bool flagValue = (bool)field.GetValue(gameState.player.progress_flags);
            bool expectedBool = expectedValue.ToLower() == "true";

            if (flagValue != expectedBool)
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckRequiredItems(List<string> requiredItems)
    {
        if (requiredItems == null || requiredItems.Count == 0) return true;
        return !requiredItems.Except(gameState.inventory.items).Any();
    }

    private void HandlePickup(ItemData item)
    {
        if (string.IsNullOrEmpty(item.inventory_image))
        {
            Debug.LogError($"No inventory_image specified for item {item.item_id}");
            return;
        }

        gameState.inventory.items.Add(item.item_id);
        item.is_completed = true;

        InventoryManager.Instance.AddItemToInventory(item.inventory_image, item.description);

        UpdateGameState(item);
    }

    private void HandleInspect(ItemData item)
    {
        CommentManager.Instance.ShowMessage(item.description);
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
        gameState.player.previous_location = gameState.player.current_location;
        gameState.player.current_location = targetScene;

        yield return StartCoroutine(SceneTransitionManager.Instance.FadeOutCoroutine());

        SceneManager.LoadScene(targetScene);
    }


    private void HandleTimeChange(ItemData item)
    {
        if (!string.IsNullOrEmpty(item.target_time_period))
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                gameState.player.lastTransitionPosition = player.transform.position;
                var spriteRenderer = player.GetComponent<SpriteRenderer>();
                gameState.player.wasFlipped = spriteRenderer != null ? spriteRenderer.flipX : false;
            }

            gameState.player.current_time_period = item.target_time_period;
            gameState.player.previous_location = gameState.player.current_location;
            gameState.player.current_location = item.target_location;

            StartCoroutine(HandleSceneTransition(item.target_location));
        }
    }

    private void UpdateGameState(ItemData item)
    {
        if (!string.IsNullOrEmpty(item.progress_flag))
        {
            var flagsType = typeof(ProgressFlags);
            var field = flagsType.GetField(item.progress_flag);

            if (field != null)
            {
                field.SetValue(gameState.player.progress_flags, true);
            }
            else
            {
                Debug.LogWarning($"Progress flag '{item.progress_flag}' not found in ProgressFlags class");
            }
        }

        DataManager.Instance.SaveGameState();
    }

    public ItemData GetItemData()
    {
        return database.GetItemById(itemId);
    }

    private void HandleDialogue(ItemData item)
    {
        DialogueManager.Instance.StartDialogue(item.dialogue_id);
    }

    private void CheckAndUpdateDisplayState(ItemData item)
    {
        if (objectToDisplay != null)
        {
            bool shouldBeActive = CheckUnlockConditions(item.unlock_conditions);
            objectToDisplay.SetActive(shouldBeActive);

            Debug.Log($"Display object {objectToDisplay.name} set to {shouldBeActive} " +
                     $"based on conditions for item {item.item_id}");
        }
        else
        {
            Debug.LogWarning($"No objectToDisplay assigned for display item {item.item_id}");
        }
    }
}