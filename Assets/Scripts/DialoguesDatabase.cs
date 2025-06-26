using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueOption
{
    public string text;
    public string next_dialogue;
    public string set_flag;
}

[System.Serializable]
public class DialogueData
{
    public string npc_id;
    public string dialogue_id;
    public string start_text;
    public List<DialogueOption> options;
    public string required_flag;
}

[System.Serializable]
public class DialoguesDatabase
{
    public List<DialogueData> dialogues;

    public static DialoguesDatabase LoadFromResources()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/DialoguesDatabase");
        return JsonUtility.FromJson<DialoguesDatabase>(jsonFile.text);
    }

    public DialogueData GetDialogueById(string id)
    {
        return dialogues.Find(dialogue => dialogue.dialogue_id == id);
    }

    public List<DialogueData> GetDialoguesByNpc(string npcId)
    {
        return dialogues.FindAll(dialogue => dialogue.npc_id == npcId);
    }
}