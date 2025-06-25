using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public GameState currentGameState;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        transform.SetParent(null);

        InitializeData();
    }

    void InitializeData()
    {
        currentGameState = LoadFromJSON<GameState>("Data/GameState") ?? new GameState();

        if (currentGameState.player == null)
        {
            currentGameState.player = new PlayerState
            {
                current_location = "PresentCorridor",
                current_time_period = "present",
                progress_flags = new ProgressFlags()
            };
        }
        else if (currentGameState.player.progress_flags == null)
        {
            currentGameState.player.progress_flags = new ProgressFlags();
        }

        currentGameState.player.progress_flags.ResetAllFlags();
    }

    private T LoadFromJSON<T>(string path) where T : new()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        return jsonFile != null ? JsonUtility.FromJson<T>(jsonFile.text) : new T();
    }

    public void SaveGameState()
    {
        string json = JsonUtility.ToJson(currentGameState);
        File.WriteAllText(Application.persistentDataPath + "/GameState.json", json);
    }
}