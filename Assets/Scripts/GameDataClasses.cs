using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractableData
{
    public string displayName;
    public string description;
    public Vector2 interactionOffset;
}

[System.Serializable]
public class InteractablesDatabase
{
    public Dictionary<string, InteractableData> interactables;
}

[Serializable]
public class Inventory
{
    public List<string> items = new List<string>();
}

[Serializable]
public class ProgressFlags
{
    public bool talked_to_assistant;
    public bool learned_about_diary;
    public bool butler_appeared;
    public bool found_time_device;
}

[Serializable]
public class PlayerState
{
    public string current_location;
    public string previous_location;
    public string current_time_period;
    public ProgressFlags progress_flags = new ProgressFlags();
}

[Serializable]
public class TimePeriodState
{
    public List<string> available_items = new List<string>();
    public List<string> npcs_present = new List<string>();
    public List<string> interactions_completed = new List<string>();
}

[Serializable]
public class LocationData
{
    public Dictionary<string, TimePeriodState> time_periods = new Dictionary<string, TimePeriodState>();
}

[Serializable]
public class DialogueTriggerFlags
{
    public bool diary_topic_unlocked;
    public bool butler_spawn_triggered;
}

[Serializable]
public class NPCDialogue
{
    public string conversation_state;
    public List<string> completed_topics = new List<string>();
    public List<string> available_topics = new List<string>();
    public DialogueTriggerFlags trigger_flags = new DialogueTriggerFlags();
}

[Serializable]
public class NPCSpawnConditions
{
    public bool talked_to_assistant;
    public bool learned_about_diary;
}

[Serializable]
public class NPCState
{
    public bool is_spawned;
    public string spawn_location;
    public NPCSpawnConditions spawn_conditions = new NPCSpawnConditions();
    public string conversation_state;
}

[Serializable]
public class GameItems
{
    public Dictionary<string, ItemData> items = new Dictionary<string, ItemData>();
}

[Serializable]
public class DialogueData
{
    public Dictionary<string, NPCDialogue> npc_dialogues = new Dictionary<string, NPCDialogue>();
}

[Serializable]
public class LocationStates
{
    public Dictionary<string, LocationData> locations = new Dictionary<string, LocationData>();
}

[Serializable]
public class GameState
{
    public PlayerState player = new PlayerState();
    public Inventory inventory = new Inventory();
}