using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Vector2 defaultSpawnPosition = new Vector2(-5f, 0f);
    [SerializeField] private bool defaultFlipPlayer = false;

    private void Start()
    {
        PositionPlayer();
    }

    private void PositionPlayer()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("Player not found in scene!");
            return;
        }

        GameState gameState = DataManager.Instance?.currentGameState ?? CreateDefaultGameState();
        Vector2 spawnPosition = defaultSpawnPosition;
        bool shouldFlip = defaultFlipPlayer;

        // Шукаємо точку появи
        if (!string.IsNullOrEmpty(gameState.player.previous_location))
        {
            foreach (var point in FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (point.fromLocation == gameState.player.previous_location)
                {
                    spawnPosition = point.transform.position;
                    shouldFlip = point.flipPlayer;

                    // Використовуємо rotation точки появи
                    player.transform.rotation = point.transform.rotation;
                    break;
                }
            }
        }

        // Встановлюємо позицію та напрямок
        player.transform.position = spawnPosition;

        // Якщо потрібно віддзеркалити спрайт
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            playerSprite.flipX = shouldFlip;
        }
    }

    private GameState CreateDefaultGameState()
    {
        return new GameState
        {
            player = new PlayerState
            {
                current_location = SceneManager.GetActiveScene().name,
                previous_location = "",
                progress_flags = new ProgressFlags()
            },
            inventory = new Inventory()
        };
    }
}