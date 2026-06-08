using UnityEngine;

namespace Ludocore
{
    public class PlayerMeleeAttack : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private float damage = 20f;
        [SerializeField] private float range = 2f;
        [SerializeField] private float radius = 0.5f;
        [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;

        [Header("Feedback")]
        [SerializeField] private Renderer hitFlashRenderer;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.05f;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private ParticleSystem hitParticles;

        private Color _originalColor;
        private bool _hasOriginalColor;

        private void Awake()
        {
            if (hitFlashRenderer && !_hasOriginalColor)
            {
                _originalColor = hitFlashRenderer.material.color;
                _hasOriginalColor = true;
            }
        }

        private void Update()
        {
            if (!Input.GetKeyDown(attackKey)) return;
            DoAttack();
        }

        private void DoAttack()
        {
            // Simple sphere cast from player forward
            Vector3 origin = transform.position + Vector3.up * 1f;
            RaycastHit[] hits = Physics.SphereCastAll(origin, radius, transform.forward, range);

            foreach (var hit in hits)
            {
                if (hit.collider.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage);
                    PlayHitFeedback(hit.point, hit.collider);
                    break; // hit one thing per swing
                }
            }
        }

        private void PlayHitFeedback(Vector3 hitPoint, Collider target)
        {
            // Flash
            if (hitFlashRenderer)
            {
                StopAllCoroutines();
                StartCoroutine(FlashRoutine());
            }

            // Sound
            if (audioSource && hitSound)
            {
                audioSource.PlayOneShot(hitSound);
            }

            // Particles
            if (hitParticles)
            {
                hitParticles.transform.position = hitPoint;
                hitParticles.transform.forward = transform.forward;
                hitParticles.Play();
            }

            // Optional: tiny knockback if target has Rigidbody
            if (target.attachedRigidbody)
            {
                target.attachedRigidbody.AddForce(transform.forward * 3f, ForceMode.VelocityChange);
            }
        }

        private System.Collections.IEnumerator FlashRoutine()
        {
            if (!hitFlashRenderer || !_hasOriginalColor) yield break;

            hitFlashRenderer.material.color = flashColor;
            float t = 0f;
            while (t < flashDuration)
            {
                t += Time.deltaTime;
                yield return null;
            }
            hitFlashRenderer.material.color = _originalColor;
        }
    }
}