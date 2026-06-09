using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace StarterAssets
{
    public partial class StarterAssetsDeployMenu : ScriptableObject
    {
        // prefab paths
        private const string PlayerArmaturePrefabName = "PlayerArmature";

        /// <summary>
        /// Check the Armature, main camera, cinemachine virtual camera, camera target and references
        /// </summary>
        [MenuItem(MenuRoot + "/Reset Third Person Controller Armature", false)]
        static void ResetThirdPersonControllerArmature()
        {
            var thirdPersonControllerType = GetTypeByName("StarterAssets.ThirdPersonController") ??
                GetTypeByName("ThirdPersonController");
            if (thirdPersonControllerType == null)
            {
                Debug.LogError("Couldn't find ThirdPersonController type");
                return;
            }

            var thirdPersonControllers = Resources.FindObjectsOfTypeAll(thirdPersonControllerType)
                .OfType<Component>()
                .Where(controller => controller.gameObject.scene.isLoaded);
            var player = thirdPersonControllers.FirstOrDefault(controller =>
                controller.GetComponent<Animator>() && controller.CompareTag(PlayerTag));

            GameObject playerGameObject = null;

            // player
            if (player == null)
            {
                if (TryLocatePrefab(PlayerArmaturePrefabName, null, new[] { thirdPersonControllerType, typeof(StarterAssetsInputs) }, out GameObject prefab, out string _))
                {
                    HandleInstantiatingPrefab(prefab, out playerGameObject);
                }
                else
                {
                    Debug.LogError("Couldn't find player armature prefab");
                }
            }
            else
            {
                playerGameObject = player.gameObject;
            }

            if (playerGameObject != null)
            {
                // cameras
                CheckCameras(playerGameObject.transform, GetThirdPersonPrefabPath());
            }
        }

        [MenuItem(MenuRoot + "/Reset Third Person Controller Capsule", false)]
        static void ResetThirdPersonControllerCapsule()
        {
            var thirdPersonControllerType = GetTypeByName("StarterAssets.ThirdPersonController") ??
                GetTypeByName("ThirdPersonController");
            if (thirdPersonControllerType == null)
            {
                Debug.LogError("Couldn't find ThirdPersonController type");
                return;
            }

            var thirdPersonControllers = Resources.FindObjectsOfTypeAll(thirdPersonControllerType)
                .OfType<Component>()
                .Where(controller => controller.gameObject.scene.isLoaded);
            var player = thirdPersonControllers.FirstOrDefault(controller =>
                !controller.GetComponent<Animator>() && controller.CompareTag(PlayerTag));

            GameObject playerGameObject = null;

            // player
            if (player == null)
            {
                if (TryLocatePrefab(PlayerCapsulePrefabName, null, new[] { thirdPersonControllerType, typeof(StarterAssetsInputs) }, out GameObject prefab, out string _))
                {
                    HandleInstantiatingPrefab(prefab, out playerGameObject);
                }
                else
                {
                    Debug.LogError("Couldn't find player capsule prefab");
                }
            }
            else
            {
                playerGameObject = player.gameObject;
            }

            if (playerGameObject != null)
            {
                // cameras
                CheckCameras(playerGameObject.transform, GetThirdPersonPrefabPath());
            }
        }

        static Type GetTypeByName(string fullTypeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullTypeName))
                .FirstOrDefault(type => type != null);
        }

        static string GetThirdPersonPrefabPath()
        {
            var thirdPersonControllerType = GetTypeByName("StarterAssets.ThirdPersonController") ??
                GetTypeByName("ThirdPersonController");

            var requiredTypes = thirdPersonControllerType != null
                ? new[] { thirdPersonControllerType, typeof(StarterAssetsInputs) }
                : new[] { typeof(StarterAssetsInputs) };

            if (TryLocatePrefab(PlayerArmaturePrefabName, null, requiredTypes, out GameObject _, out string prefabPath))
            {
                var pathString = new StringBuilder();
                var currentDirectory = new FileInfo(prefabPath).Directory;
                while (currentDirectory.Name != "Packages")
                {
                    pathString.Insert(0, $"/{currentDirectory.Name}");
                    currentDirectory = currentDirectory.Parent;
                }

                pathString.Insert(0, currentDirectory.Name);
                return pathString.ToString();
            }

            return null;
        }
    }
}