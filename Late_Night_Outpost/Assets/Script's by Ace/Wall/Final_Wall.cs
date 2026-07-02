using UnityEngine;
using UnityEngine.SceneManagement;   // needed for loading scenes

public class SceneChangeOnCollision : MonoBehaviour
{
    [Header("Scene settings")]
    [Tooltip("Exact name of the scene to load (must be in Build Settings).")]
    [SerializeField] private string sceneToLoad;

    [Header("Player filter")]
    [Tooltip("Only objects with this tag can trigger the scene change.")]
    [SerializeField] private string playerTag = "Player";

    // Trigger-based: use if the wall's collider is marked IsTrigger = true
    private void OnTriggerEnter(Collider other)
    {
        // Only react to the player
        if (!other.CompareTag(playerTag)) return;

        LoadScene();
    }

    // Collision-based: use if the wall's collider is NOT a trigger
    private void OnCollisionEnter(Collision collision)
    {
        // Only react to the player
        if (!collision.gameObject.CompareTag(playerTag)) return;

        LoadScene();
    }

    // Common scene loading logic
    private void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("SceneChangeOnCollision: sceneToLoad is empty.");
            return;
        }

        // Make sure the scene is added to Build Settings
        SceneManager.LoadScene(sceneToLoad);
    }
}