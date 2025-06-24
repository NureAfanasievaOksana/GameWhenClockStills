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

        // Переконайтеся, що об'єкт у корені ієрархії
        transform.SetParent(null);

        InitializeData();
    }

    void InitializeData()
    {
        currentGameState = LoadFromJSON<GameState>("Data/GameState") ?? new GameState();

        // Якщо це новий об'єкт, ініціалізуємо базові значення
        if (currentGameState.player == null)
        {
            currentGameState.player = new PlayerState
            {
                current_location = "MainHall",
                current_time_period = "present",
                progress_flags = new ProgressFlags()
            };
        }
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