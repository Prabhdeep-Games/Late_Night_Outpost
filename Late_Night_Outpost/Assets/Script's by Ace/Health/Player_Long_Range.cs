using UnityEngine;

namespace Ludocore
{
    public class PlayerMeleeAttackLong : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private LocalSpawner projectileSpawner;
        [SerializeField] private KeyCode attackKey = KeyCode.Mouse1;

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

            // When the spawner emits a projectile, subscribe to its hit event
            if (projectileSpawner && projectileSpawner.LastSpawned)
            {
                // optional: if your projectile has its own script exposing an OnHit event,
                // you would hook it here; see note below.
            }
        }

        private void Update()
        {
            if (!Input.GetKeyDown(attackKey)) return;
            FireProjectile();
        }

        private void FireProjectile()
        {
            if (!projectileSpawner) return;

            // Spawns the projectile at local offset, facing forward
            projectileSpawner.Spawn();
        }

        // Call this from the projectile when it actually hits something
        public void OnProjectileHit(Vector3 hitPoint, Collider target, Vector3 direction)
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
                hitParticles.transform.forward = direction;
                hitParticles.Play();
            }

            // Optional: knockback if target has Rigidbody
            if (target && target.attachedRigidbody)
            {
                target.attachedRigidbody.AddForce(direction * 3f, ForceMode.VelocityChange);
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