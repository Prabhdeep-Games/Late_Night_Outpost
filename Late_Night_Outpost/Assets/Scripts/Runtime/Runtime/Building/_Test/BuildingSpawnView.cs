using System;
using UnityEngine;
using DG.Tweening;

namespace Ludocore.Test
{
    /// <summary>Abstract base for building spawn animations. Lives on the spawned prefab. (TEST)</summary>
    public abstract class BuildingSpawnViewTest : MonoBehaviour
    {
        public event Action OnCompleted;

        /// <summary>Play the spawn-in animation.</summary>
        public abstract void Play();

        protected void FireCompleted()
        {
            OnCompleted?.Invoke();
        }
    }

    /// <summary>Uniform scale from zero — simple pop-in.</summary>
    public class ScaleUpSpawnViewTest : BuildingSpawnViewTest
    {
        [Header("Config")]
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private Ease ease = Ease.OutBack;

        private Vector3 _targetScale;

        private void Awake()
        {
            _targetScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }

        public override void Play()
        {
            transform.DOScale(_targetScale, duration)
                .SetEase(ease)
                .OnComplete(FireCompleted);
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }

    /// <summary>Grows from ground up along Y axis.</summary>
    public class GrowUpSpawnViewTest : BuildingSpawnViewTest
    {
        [Header("Config")]
        [SerializeField] private float duration = 0.6f;
        [SerializeField] private Ease ease = Ease.OutCubic;

        private Vector3 _targetScale;

        private void Awake()
        {
            _targetScale = transform.localScale;
            transform.localScale = new Vector3(_targetScale.x, 0f, _targetScale.z);
        }

        public override void Play()
        {
            transform.DOScale(_targetScale, duration)
                .SetEase(ease)
                .OnComplete(FireCompleted);
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}