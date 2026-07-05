using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;
    [Tooltip("Scenes where this music should be active.")]
    public string[] musicScenes;

    void Awake()
    {
        // Singleton + persistent
        if (FindObjectsOfType<MusicManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldPlay = false;

        foreach (var s in musicScenes)
        {
            if (scene.name == s)
            {
                shouldPlay = true;
                break;
            }
        }

        if (shouldPlay)
        {
            if (!musicSource.isPlaying)
            {
                musicSource.loop = true;
                musicSource.Play();
            }
        }
        else
        {
            // If you truly want it NEVER to stop, comment this out
            // musicSource.Stop();
        }
    }
}