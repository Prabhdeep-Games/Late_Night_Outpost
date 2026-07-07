using UnityEngine;
using UnityEngine.SceneManagement;

public class GroupMusicB : MonoBehaviour
{
    public static GroupMusicB Instance;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;

    [Header("Scenes using this track")]
    [SerializeField] private string[] musicScenes; // e.g. Level5, Level6, Level7...

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (musicSource == null) return;

        bool shouldPlay = false;

        foreach (string sceneName in musicScenes)
        {
            if (scene.name == sceneName)
            {
                shouldPlay = true;
                break;
            }
        }

        if (shouldPlay)
        {
            musicSource.loop = true;
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
        else
        {
            if (musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }
    }
}