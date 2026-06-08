// ============================================================================
// SceneGraphBuilder — Architecture Visualizer
//
// Walks every loaded scene and emits a directed reference graph:
//   - GameObjects appear ONLY if they carry a user MonoBehaviour, OR if some
//     other node in the graph references them. Decorative children of 3D
//     models, empty hierarchy organizers, etc. are skipped — the graph shows
//     architecture, not scene contents.
//   - Every user-defined MonoBehaviour becomes a node.
//   - Every ScriptableObject reached through a SerializeField becomes a node.
//
// Edges are tagged with one of three kinds so the GraphView can color them:
//   - Containment      : GameObject → its own Components
//   - ObjectReference  : Component  → GameObject or another Component
//   - DataReference    : Component  → ScriptableObject
//
// User-defined = type's namespace is not under UnityEngine / UnityEditor /
// Unity.* / TMPro / System. Engine components (Transform, Renderer, …) are
// filtered out; if a SerializeField points at one, the edge is redirected to
// its GameObject so the wire still shows.
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Ludocore.Editor.ArchitectureVisualizer
{
    public enum NodeKind { GameObject, Component, ScriptableObject }

    public enum EdgeKind { Containment, ObjectReference, DataReference }

    public class GraphNode
    {
        public string Id;
        public NodeKind Kind;
        public string DisplayName;
        public string TypeName;
        public Object UnityObject;
        public string ParentGameObjectId;
        public int HierarchyDepth;
    }

    public class GraphEdge
    {
        public string FromId;
        public string ToId;
        public EdgeKind Kind;
    }

    public class SceneGraph
    {
        public readonly List<GraphNode> Nodes = new();
        public readonly List<GraphEdge> Edges = new();
    }

    public static class SceneGraphBuilder
    {
        public static SceneGraph Build()
        {
            var graph = new SceneGraph();
            var seen = new HashSet<string>();

            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                Scene scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded) continue;

                foreach (GameObject root in scene.GetRootGameObjects())
                    WalkGameObject(root, 0, graph, seen);
            }

            return graph;
        }

        private static void WalkGameObject(GameObject go, int depth, SceneGraph graph, HashSet<string> seen)
        {
            if (go == null) return;
            if ((go.hideFlags & HideFlags.HideInHierarchy) != 0) return;

            // Collect this GO's user MonoBehaviours up front.
            List<MonoBehaviour> userComponents = null;
            foreach (MonoBehaviour comp in go.GetComponents<MonoBehaviour>())
            {
                if (comp == null) continue; // missing script
                if (!IsUserType(comp.GetType())) continue;
                userComponents ??= new List<MonoBehaviour>();
                userComponents.Add(comp);
            }

            bool hasUserScripts = userComponents != null;
            string goId = IdFor(go);

            // Eager-add the GO only if it carries user code. If something later
            // references it via SerializeField, lazy-add will catch it.
            if (hasUserScripts && seen.Add(goId))
            {
                graph.Nodes.Add(new GraphNode
                {
                    Id = goId,
                    Kind = NodeKind.GameObject,
                    DisplayName = go.name,
                    UnityObject = go,
                    HierarchyDepth = depth
                });
            }

            if (hasUserScripts)
            {
                foreach (MonoBehaviour comp in userComponents)
                {
                    Type t = comp.GetType();
                    string compId = IdFor(comp);
                    if (seen.Add(compId))
                    {
                        graph.Nodes.Add(new GraphNode
                        {
                            Id = compId,
                            Kind = NodeKind.Component,
                            DisplayName = t.Name,
                            TypeName = t.FullName,
                            UnityObject = comp,
                            ParentGameObjectId = goId,
                            HierarchyDepth = depth
                        });
                    }

                    // GO owns its components.
                    graph.Edges.Add(new GraphEdge
                    {
                        FromId = goId,
                        ToId = compId,
                        Kind = EdgeKind.Containment
                    });

                    WalkSerializedReferences(comp, compId, graph, seen);
                }
            }

            // Always recurse — children may carry user scripts even if this GO does not.
            for (int i = 0; i < go.transform.childCount; i++)
                WalkGameObject(go.transform.GetChild(i).gameObject, depth + 1, graph, seen);
        }

        private static void WalkSerializedReferences(Object owner, string ownerId, SceneGraph graph, HashSet<string> seen)
        {
            using var so = new SerializedObject(owner);
            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                enterChildren = true;

                if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;

                Object target = prop.objectReferenceValue;
                if (target == null) continue;

                EdgeKind kind = target is ScriptableObject
                    ? EdgeKind.DataReference
                    : EdgeKind.ObjectReference;

                AddReferenceEdge(target, ownerId, kind, graph, seen);
            }
        }

        private static void AddReferenceEdge(Object target, string ownerId, EdgeKind kind, SceneGraph graph, HashSet<string> seen)
        {
            switch (target)
            {
                case ScriptableObject so:
                {
                    string id = IdFor(so);
                    if (seen.Add(id))
                    {
                        graph.Nodes.Add(new GraphNode
                        {
                            Id = id,
                            Kind = NodeKind.ScriptableObject,
                            DisplayName = so.name,
                            TypeName = so.GetType().Name,
                            UnityObject = so
                        });
                    }
                    graph.Edges.Add(new GraphEdge { FromId = ownerId, ToId = id, Kind = kind });
                    break;
                }
                case Component comp:
                {
                    Type t = comp.GetType();
                    if (!IsUserType(t))
                    {
                        // Engine component (Transform, Renderer, RectTransform…). The
                        // wire still matters — redirect it to the underlying GameObject
                        // so dragged-GameObject references appear in the graph.
                        if (comp.gameObject != null)
                            AddReferenceEdge(comp.gameObject, ownerId, kind, graph, seen);
                        return;
                    }

                    string id = IdFor(comp);
                    if (seen.Add(id))
                    {
                        graph.Nodes.Add(new GraphNode
                        {
                            Id = id,
                            Kind = NodeKind.Component,
                            DisplayName = t.Name,
                            TypeName = t.FullName,
                            UnityObject = comp,
                            ParentGameObjectId = comp.gameObject ? IdFor(comp.gameObject) : null
                        });
                    }
                    graph.Edges.Add(new GraphEdge { FromId = ownerId, ToId = id, Kind = kind });
                    break;
                }
                case GameObject go:
                {
                    string id = IdFor(go);
                    if (seen.Add(id))
                    {
                        graph.Nodes.Add(new GraphNode
                        {
                            Id = id,
                            Kind = NodeKind.GameObject,
                            DisplayName = go.name,
                            UnityObject = go
                        });
                    }
                    graph.Edges.Add(new GraphEdge { FromId = ownerId, ToId = id, Kind = kind });
                    break;
                }
                // Materials, Textures, AudioClips, Meshes, etc. are intentionally ignored —
                // they're authored assets, not part of the runtime architecture.
            }
        }

        private static string IdFor(Object o) => o.GetInstanceID().ToString();

        private static bool IsUserType(Type t)
        {
            string ns = t.Namespace ?? string.Empty;
            if (ns.StartsWith("UnityEngine", StringComparison.Ordinal)) return false;
            if (ns.StartsWith("UnityEditor", StringComparison.Ordinal)) return false;
            if (ns.StartsWith("Unity.",      StringComparison.Ordinal)) return false;
            if (ns.StartsWith("TMPro",       StringComparison.Ordinal)) return false;
            if (ns.StartsWith("System",      StringComparison.Ordinal)) return false;
            return true;
        }
    }
}
