// ============================================================================
// GhostPreview — Helper
//
// Spawns a non-interactive "ghost" clone of a building prefab. Used by:
//   BuildingSiteView — ghost shown when the player focuses a build site
//   PlayerBuild      — ghost that follows the cursor while a Builder is held
//
// The ghost keeps only visuals (MeshRenderer / SkinnedMeshRenderer / MeshFilter).
// Colliders are disabled and all scripts, rigidbodies, and particle systems
// are stripped so the ghost can't be interacted with, doesn't fall, doesn't
// tick, and doesn't spawn anything.
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    /// <summary>Creates stripped-down preview clones of building prefabs.</summary>
    public static class GhostPreview
    {
        /// <summary>Spawn a ghost of the prefab at the given pose. Returns the
        /// ghost GameObject (inactive). Caller owns the lifetime — Destroy it
        /// when the preview ends.</summary>
        public static GameObject Create(GameObject sourcePrefab, Vector3 position, Quaternion rotation, Material ghostMaterial, Transform parent = null)
        {
            if (!sourcePrefab) return null;

            GameObject ghost = Object.Instantiate(sourcePrefab, position, rotation, parent);
            ghost.name = sourcePrefab.name + "_Ghost";

            Strip(ghost);
            ApplyMaterial(ghost, ghostMaterial);

            ghost.SetActive(false);
            return ghost;
        }

        //==================== PRIVATE =====================
        private static void Strip(GameObject ghost)
        {
            // Disable interaction surfaces.
            foreach (Collider c in ghost.GetComponentsInChildren<Collider>(true))
                c.enabled = false;

            // Destroy anything that would tick, animate, or fall.
            foreach (Rigidbody rb in ghost.GetComponentsInChildren<Rigidbody>(true))
                Object.Destroy(rb);

            foreach (ParticleSystem ps in ghost.GetComponentsInChildren<ParticleSystem>(true))
                Object.Destroy(ps);

            foreach (MonoBehaviour mb in ghost.GetComponentsInChildren<MonoBehaviour>(true))
                Object.Destroy(mb);
        }

        private static void ApplyMaterial(GameObject ghost, Material material)
        {
            if (!material) return;

            foreach (MeshRenderer r in ghost.GetComponentsInChildren<MeshRenderer>(true))
                Paint(r, material);

            foreach (SkinnedMeshRenderer r in ghost.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                Paint(r, material);
        }

        private static void Paint(Renderer r, Material material)
        {
            Material[] mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++) mats[i] = material;
            r.sharedMaterials = mats;
        }
    }
}
