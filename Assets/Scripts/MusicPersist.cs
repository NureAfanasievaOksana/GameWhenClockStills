using UnityEngine;

public class MusicPersist : MonoBehaviour
{
    private static MusicPersist instance;
    public AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
}
