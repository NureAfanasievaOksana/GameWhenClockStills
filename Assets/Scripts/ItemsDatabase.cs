using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UnlockCondition
{
    public string flagName;
    public bool requiredValue;
}

[System.Serializable]
public class ItemData
{
    public string item_id;
    public string name;
    public string description;
    public string type;
    public bool is_completed;
    public bool repeatable;
    public List<string> required_items;
    public List<string> unlock_conditions;
    public string progress_flag;
    public string target_location;
    public string target_time_period;
    public string inventory_image;
}

[System.Serializable]
public class ItemsDatabase
{
    public List<ItemData> items;

    public static ItemsDatabase LoadFromResources()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/ItemsDatabase");
        return JsonUtility.FromJson<ItemsDatabase>(jsonFile.text);
    }

    public ItemData GetItemById(string id)
    {
        return items.Find(item => item.item_id == id);
    }

    public List<ItemData> GetAllItemsById(string id)
    {
        return items.FindAll(item => item.item_id == id);
    }
}