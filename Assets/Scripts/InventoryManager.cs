using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private List<RectTransform> inventorySlots = new List<RectTransform>();
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    private List<GameObject> inventoryItems = new List<GameObject>();
    private List<string> savedItemNames = new List<string>();
    private Transform inventoryItemsContainer;
    private int selectedSlotIndex = -1;
    private bool hasInventoryInScene = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeInventory()
    {
        CheckForInventoryInScene();

        if (!hasInventoryInScene) return;

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventoryItems.Add(null);
            savedItemNames.Add(null);

            SetupSlotButton(i);
        }
    }

    private void CheckForInventoryInScene()
    {
        hasInventoryInScene = GameObject.Find("InventorySlotsContainer") != null;

        if (hasInventoryInScene)
        {
            inventoryItemsContainer = GameObject.Find("InventoryItems")?.transform;
        }
    }

    private void SetupSlotButton(int slotIndex)
    {
        Button slotButton = inventorySlots[slotIndex].GetComponent<Button>();
        if (slotButton == null)
        {
            slotButton = inventorySlots[slotIndex].gameObject.AddComponent<Button>();
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(() => ToggleSlotSelection(slotIndex));
    }

    public void ToggleSlotSelection(int slotIndex)
    {
        if (inventoryItems[slotIndex] == null) return;

        if (selectedSlotIndex == slotIndex)
        {
            DeselectAllSlots();
            return;
        }

        if (selectedSlotIndex >= 0 && selectedSlotIndex < inventoryItems.Count)
        {
            if (inventoryItems[selectedSlotIndex] != null)
            {
                inventoryItems[selectedSlotIndex].GetComponent<Image>().color = normalColor;
            }
        }

        selectedSlotIndex = slotIndex;
        inventoryItems[selectedSlotIndex].GetComponent<Image>().color = selectedColor;
    }

    public void DeselectAllSlots()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < inventoryItems.Count)
        {
            if (inventoryItems[selectedSlotIndex] != null)
            {
                inventoryItems[selectedSlotIndex].GetComponent<Image>().color = normalColor;
            }
        }
        selectedSlotIndex = -1;
    }

    public string GetSelectedItem()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < savedItemNames.Count)
        {
            return savedItemNames[selectedSlotIndex];
        }
        return null;
    }

    public bool UseSelectedItem(string requiredItemId)
    {
        if (selectedSlotIndex == -1) return false;

        string selectedItem = GetSelectedItem();
        if (selectedItem == requiredItemId)
        {
            RemoveItemFromInventory(selectedSlotIndex);
            DeselectAllSlots();
            return true;
        }

        return false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshReferences();
        StartCoroutine(RestoreInventoryWithDelay());
    }

    private IEnumerator RestoreInventoryWithDelay()
    {
        yield return null;
        if (hasInventoryInScene)
        {
            RestoreInventory();
        }
    }

    private void RefreshReferences()
    {
        CheckForInventoryInScene();

        if (!hasInventoryInScene) return;

        GameObject slotsContainer = GameObject.Find("InventorySlotsContainer");
        if (slotsContainer != null)
        {
            List<RectTransform> newSlots = new List<RectTransform>();
            foreach (Transform child in slotsContainer.transform)
            {
                var rect = child.GetComponent<RectTransform>();
                if (rect != null) newSlots.Add(rect);
            }

            inventorySlots = newSlots;

            while (inventoryItems.Count < inventorySlots.Count)
            {
                inventoryItems.Add(null);
                savedItemNames.Add(null);
            }
            while (inventoryItems.Count > inventorySlots.Count)
            {
                inventoryItems.RemoveAt(inventoryItems.Count - 1);
                savedItemNames.RemoveAt(savedItemNames.Count - 1);
            }

            for (int i = 0; i < inventorySlots.Count; i++)
            {
                SetupSlotButton(i);
            }
        }

        inventoryItemsContainer = GameObject.Find("InventoryItems")?.transform;
    }

    private void RestoreInventory()
    {
        if (inventorySlots == null || inventoryItems == null) return;

        for (int i = 0; i < Mathf.Min(inventorySlots.Count, savedItemNames.Count); i++)
        {
            var slot = inventorySlots[i];

            if (slot == null || slot.Equals(null)) continue;

            if (slot.childCount > 0)
            {
                for (int c = slot.childCount - 1; c >= 0; c--)
                {
                    var child = slot.GetChild(c);
                    if (child != null && !child.Equals(null))
                        Destroy(child.gameObject);
                }
            }

            inventoryItems[i] = null;

            if (!string.IsNullOrEmpty(savedItemNames[i]) && inventoryItemsContainer != null)
            {
                Transform item = inventoryItemsContainer.Find(savedItemNames[i]);
                if (item != null)
                {
                    GameObject newItem = Instantiate(item.gameObject, slot);
                    newItem.SetActive(true);

                    RectTransform rt = newItem.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = Vector2.zero;
                        rt.sizeDelta = new Vector2(100, 100);
                        rt.localScale = Vector3.one;
                    }

                    Image img = newItem.GetComponent<Image>();
                    if (img == null)
                    {
                        img = newItem.AddComponent<Image>();
                    }
                    img.color = normalColor;

                    inventoryItems[i] = newItem;
                }
            }
        }
    }

    public void AddItemToInventory(string itemName, string itemDescription)
    {
        if (!hasInventoryInScene) return;

        if (inventoryItemsContainer == null)
            inventoryItemsContainer = GameObject.Find("InventoryItems").transform;

        Transform itemTransform = inventoryItemsContainer?.Find(itemName);
        if (itemTransform == null)
        {
            Debug.LogError($"Item {itemName} not found!");
            return;
        }

        for (int i = 0; i < Mathf.Min(inventorySlots.Count, savedItemNames.Count); i++)
        {
            if (string.IsNullOrEmpty(savedItemNames[i]))
            {
                GameObject newItem = Instantiate(itemTransform.gameObject, inventorySlots[i]);
                newItem.SetActive(true);

                RectTransform rt = newItem.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(100, 100);
                rt.localScale = Vector3.one;

                Image img = newItem.GetComponent<Image>();
                if (img == null)
                {
                    img = newItem.AddComponent<Image>();
                }
                img.color = normalColor;

                inventoryItems[i] = newItem;
                savedItemNames[i] = itemName;

                if (CommentManager.Instance != null)
                {
                    CommentManager.Instance.ShowMessage(itemDescription);
                }
                return;
            }
        }

        if (CommentManager.Instance != null)
        {
            CommentManager.Instance.ShowMessage("Інвентар повний!");
        }
    }

    public void RemoveItemFromInventory(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventoryItems.Count && inventoryItems[slotIndex] != null)
        {
            Destroy(inventoryItems[slotIndex]);
            inventoryItems[slotIndex] = null;
            savedItemNames[slotIndex] = null;
            DeselectAllSlots();
        }
    }
}