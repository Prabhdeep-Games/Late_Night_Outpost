using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;

public class TreeGrower : MonoBehaviour
{
    [Header("Scale")]
    [Tooltip("Final uniform scale the plant grows to.")]
    [Min(0f)]
    [SerializeField] private float targetScale = 1f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;
    
    [Header("Timing")]
    [Tooltip("How long the grow animation takes in seconds.")]
    [Min(0f)]
    [SerializeField] private float duration = 1f;
    [Tooltip("Start growing automatically when the object is enabled.")]
    [FormerlySerializedAs("autoStart")]
    [SerializeField] private bool autoPlay = true;
    
    private void OnEnable()
    {
        if (autoPlay) Grow();
    }
    
    private void OnDisable()
    {
        transform.DOKill();
    }

    public void Grow()
    {
        transform.DOScale(Vector3.one * targetScale, duration).SetEase(scaleEase);
    }
}
