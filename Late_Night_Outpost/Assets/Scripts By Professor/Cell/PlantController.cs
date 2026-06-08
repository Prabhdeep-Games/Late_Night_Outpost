using Ludocore;
using UnityEngine;

public class PlantController : MonoBehaviour
{
    public Lifecycle lifeCycle;

    void OnEnable()
    {
        lifeCycle.OnDied += HandleOnDeath;
    }

    void OnDisable()
    {
        lifeCycle.OnDied -= HandleOnDeath;
    }
    private void HandleOnDeath()

    {
        Destroy(gameObject);
    }
}
