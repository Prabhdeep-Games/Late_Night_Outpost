// ============================================
// Building Data (TEST)
// ============================================
// PURPOSE: Defines a building type and its upgrade levels.
// USAGE: Create via Assets > Create > Ludocore/Test/Building Data.
//        Assign to BuildingSiteTest component on a building site.
//        One asset per building type — Hearth, Workshop, Beacon each get their own.
// NOTE: Test version — will be rebuilt during lecture.
// ============================================

using System;
using UnityEngine;

namespace Ludocore.Test
{
    [CreateAssetMenu(fileName = "NewBuildingData", menuName = "Ludocore/Test/Building Data")]
    public class BuildingDataTest : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name of the building.")]
        [SerializeField] private string buildingName;

        [Header("Levels")]
        [Tooltip("Sequential build/upgrade levels. Level 0 is the first construction.")]
        [SerializeField] private BuildingLevelTest[] levels;

        public string BuildingName => buildingName;
        public int LevelCount => levels.Length;

        public BuildingLevelTest GetLevel(int index) => levels[index];
    }

    [Serializable]
    public struct BuildingLevelTest
    {
        [Tooltip("Prefab spawned when this level is reached.")]
        public GameObject Prefab;

        // Resource costs added later
    }
}
