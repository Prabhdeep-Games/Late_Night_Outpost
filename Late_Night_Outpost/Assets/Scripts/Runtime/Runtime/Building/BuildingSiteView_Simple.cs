using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Ludocore
{
    /// <summary>SIMPLE (L4) version. Visual feedback for BuildingSite_Simple.
    /// Kept so the Lecture 4 scene keeps working. New scripts use BuildingSiteView + Focusable.</summary>
    public class BuildingSiteView_Simple : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("The BuildingSite_Simple logic component to observe.")]
        [SerializeField] private BuildingSite_Simple site;

        [Header("Particles")]
        [Tooltip("Sub-particle systems to toggle when the player is detected.")]
        [SerializeField] private ParticleSystem[] subParticleSystems;

        [Header("Canvas")]
        [Tooltip("Canvas containing the building name prompt.")]
        [SerializeField] private Canvas promptCanvas;

        [Tooltip("Text field displaying the building name.")]
        [SerializeField] private TextMeshProUGUI nameText;

        [Tooltip("How far the canvas slides up from its initial position.")]
        [SerializeField] private float slideDistance = 50f;

        [Tooltip("Duration of the slide animation in seconds.")]
        [SerializeField] private float slideDuration = 0.3f;

        //==================== STATE =====================
        private RectTransform _canvasRect;
        private Vector2 _hiddenPos;
        private Vector2 _shownPos;
        private Tween _slideTween;

        //==================== LIFECYCLE =====================
        private void Awake()
        {
            if (promptCanvas)
            {
                _canvasRect = promptCanvas.GetComponent<RectTransform>();
                _hiddenPos = _canvasRect.anchoredPosition;
                _shownPos = _hiddenPos + Vector2.up * slideDistance;
            }
        }

        private void OnEnable()
        {
            site.OnPlayerDetected += OnPlayerDetected;
            site.OnPlayerLost += OnPlayerLost;
        }

        private void OnDisable()
        {
            site.OnPlayerDetected -= OnPlayerDetected;
            site.OnPlayerLost -= OnPlayerLost;
        }

        private void Start()
        {
            HideSubParticles();
            HidePrompt();
        }

        //==================== HANDLERS =====================
        private void OnPlayerDetected()
        {
            ShowSubParticles();
            ShowPrompt();
        }

        private void OnPlayerLost()
        {
            HideSubParticles();
            HidePrompt();
        }

        //==================== PARTICLES =====================
        /// <summary>Activate the sub-particle systems of the marker.</summary>
        public void ShowSubParticles()
        {
            for (int i = 0; i < subParticleSystems.Length; i++)
                if (subParticleSystems[i]) subParticleSystems[i].Play();
        }

        /// <summary>Deactivate the sub-particle systems of the marker.</summary>
        public void HideSubParticles()
        {
            for (int i = 0; i < subParticleSystems.Length; i++)
                if (subParticleSystems[i]) subParticleSystems[i].Stop();
        }

        //==================== CANVAS =====================
        /// <summary>Update the name text and slide the canvas up.</summary>
        public void ShowPrompt()
        {
            if (!_canvasRect) return;

            if (nameText && site.Data)
                nameText.text = site.Data.BuildingName;

            promptCanvas.gameObject.SetActive(true);
            _slideTween?.Kill();
            _slideTween = _canvasRect.DOAnchorPos(_shownPos, slideDuration).SetEase(Ease.OutBack);
        }

        /// <summary>Slide the canvas down and hide it.</summary>
        public void HidePrompt()
        {
            if (!_canvasRect) return;

            _slideTween?.Kill();
            _slideTween = _canvasRect
                .DOAnchorPos(_hiddenPos, slideDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => promptCanvas.gameObject.SetActive(false));
        }
    }
}
