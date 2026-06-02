// ============================================================================
// ArchitectureGraphView — the canvas.
//
// Receives a SceneGraph from SceneGraphBuilder, instantiates one visual node
// per GraphNode, connects them with edges per GraphEdge, and lays them out
// in three columns:
//
//   ┌──────────────┬──────────────┬──────────────┐
//   │ GameObjects  │ Components   │ ScriptableObj│
//   │ (grey)       │ (blue)       │ (green)      │
//   └──────────────┴──────────────┴──────────────┘
//
// Edges are colored by EdgeKind so the architecture lesson reads visually:
//   - Containment       (GO → its components)        : dim grey, hairline
//   - ObjectReference   (script → script / GameObject): light blue
//   - DataReference     (script → ScriptableObject)   : green (matches SO)
//
// Two fixed overlays sit on top of the graph (don't pan/zoom with content):
//   - Legend (bottom-left)         — explains the six visual concepts
//   - Empty-state label (centered) — shown when the graph has no nodes
//
// SO column is sorted by the average Y of components that reference each SO,
// so heavily-referenced SOs land near their consumers and edges stay legible.
//
// Right-click → Hide Selection. Right-click → Show All. (Show All also
// available from the window toolbar.) Read-only otherwise.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ludocore.Editor.ArchitectureVisualizer
{
    public class ArchitectureGraphView : GraphView
    {
        private const float ColumnGameObjectX = 0f;
        private const float ColumnComponentX = 340f;
        private const float ColumnScriptableObjectX = 760f;
        private const float GameObjectRowSpacing = 90f;
        private const float ComponentRowSpacing = 70f;
        private const float ScriptableObjectRowSpacing = 80f;

        /// <summary>Window subscribes here so toolbar/context-menu commands can ask for a full rebuild.</summary>
        public Action OnRequestRebuild;

        private Label _emptyState;

        public ArchitectureGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            style.flexGrow = 1;

            // Read-only: swallow Delete; no new edges may be drawn.
            deleteSelection = (_, _) => { };

            // Fixed overlays — added through hierarchy.Add so they sit OUTSIDE the
            // panning/zooming content layer and stay anchored to the window.
            hierarchy.Add(BuildLegend());
            _emptyState = BuildEmptyState();
            hierarchy.Add(_emptyState);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) => new();

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            if (selection.OfType<ArchitectureNode>().Any())
                evt.menu.AppendAction("Hide Selection", _ => HideSelection());
            evt.menu.AppendAction("Show All", _ => OnRequestRebuild?.Invoke());
        }

        public void Populate(SceneGraph graph)
        {
            ClearGraph();

            int nodeCount = graph?.Nodes.Count ?? 0;
            _emptyState.style.display = nodeCount == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            if (graph == null) return;

            var visuals = new Dictionary<string, ArchitectureNode>(graph.Nodes.Count);

            foreach (GraphNode n in graph.Nodes)
            {
                ArchitectureNode visual = n.Kind switch
                {
                    NodeKind.GameObject       => new GameObjectNode(n),
                    NodeKind.Component        => new ComponentNode(n),
                    NodeKind.ScriptableObject => new ScriptableObjectNode(n),
                    _                         => null
                };
                if (visual == null) continue;

                AddElement(visual);
                visuals[n.Id] = visual;
            }

            LayoutNodes(graph, visuals);

            foreach (GraphEdge e in graph.Edges)
            {
                if (!visuals.TryGetValue(e.FromId, out ArchitectureNode from)) continue;
                if (!visuals.TryGetValue(e.ToId,   out ArchitectureNode to))   continue;

                Edge edge = from.OutputPort.ConnectTo(to.InputPort);
                edge.capabilities &= ~Capabilities.Deletable;

                Color color = ColorForEdge(e.Kind);
                edge.edgeControl.inputColor = color;
                edge.edgeControl.outputColor = color;

                AddElement(edge);
            }
        }

        public void ApplySearch(string query)
        {
            bool empty = string.IsNullOrWhiteSpace(query);
            string needle = empty ? null : query.ToLowerInvariant();

            nodes.ForEach(n =>
            {
                if (n is not ArchitectureNode an) return;
                if (empty)
                {
                    an.style.opacity = 1f;
                    return;
                }
                bool match =
                    (an.Data.DisplayName != null && an.Data.DisplayName.ToLowerInvariant().Contains(needle)) ||
                    (an.Data.TypeName    != null && an.Data.TypeName.ToLowerInvariant().Contains(needle));
                an.style.opacity = match ? 1f : 0.18f;
            });
        }

        private void HideSelection()
        {
            var hide = selection.OfType<ArchitectureNode>().ToList();
            foreach (ArchitectureNode node in hide)
            {
                foreach (Edge edge in node.InputPort.connections.ToList())
                {
                    edge.output?.Disconnect(edge);
                    edge.input?.Disconnect(edge);
                    RemoveElement(edge);
                }
                foreach (Edge edge in node.OutputPort.connections.ToList())
                {
                    edge.output?.Disconnect(edge);
                    edge.input?.Disconnect(edge);
                    RemoveElement(edge);
                }
                RemoveElement(node);
            }
            ClearSelection();
        }

        private void ClearGraph()
        {
            List<GraphElement> all = graphElements.ToList();
            foreach (GraphElement el in all)
                RemoveElement(el);
        }

        private static Color ColorForEdge(EdgeKind kind) => kind switch
        {
            EdgeKind.Containment     => ArchitectureColors.Containment,
            EdgeKind.DataReference   => ArchitectureColors.DataReference,
            EdgeKind.ObjectReference => ArchitectureColors.ObjectReference,
            _                        => Color.white
        };

        private static void LayoutNodes(SceneGraph graph, Dictionary<string, ArchitectureNode> visuals)
        {
            // Group components by their parent GameObject so we can stack them together.
            var componentsByGo = new Dictionary<string, List<GraphNode>>();
            foreach (GraphNode n in graph.Nodes)
            {
                if (n.Kind != NodeKind.Component) continue;
                if (string.IsNullOrEmpty(n.ParentGameObjectId)) continue;

                if (!componentsByGo.TryGetValue(n.ParentGameObjectId, out List<GraphNode> list))
                {
                    list = new List<GraphNode>();
                    componentsByGo[n.ParentGameObjectId] = list;
                }
                list.Add(n);
            }

            // Columns 1 + 2: walk GameObjects in declared order, stack their components beside them.
            float y = 0f;
            foreach (GraphNode n in graph.Nodes)
            {
                if (n.Kind != NodeKind.GameObject) continue;
                if (!visuals.TryGetValue(n.Id, out ArchitectureNode goVisual)) continue;

                goVisual.SetPosition(new Rect(ColumnGameObjectX, y, 0, 0));

                if (componentsByGo.TryGetValue(n.Id, out List<GraphNode> children))
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        if (!visuals.TryGetValue(children[i].Id, out ArchitectureNode comp)) continue;
                        comp.SetPosition(new Rect(ColumnComponentX, y + i * ComponentRowSpacing, 0, 0));
                    }
                    y += Mathf.Max(GameObjectRowSpacing, children.Count * ComponentRowSpacing + 20f);
                }
                else
                {
                    y += GameObjectRowSpacing;
                }
            }

            // Components added lazily (referenced before their owner GO was walked).
            foreach (GraphNode n in graph.Nodes)
            {
                if (n.Kind != NodeKind.Component) continue;
                if (!string.IsNullOrEmpty(n.ParentGameObjectId) &&
                    componentsByGo.ContainsKey(n.ParentGameObjectId)) continue;

                if (!visuals.TryGetValue(n.Id, out ArchitectureNode comp)) continue;
                comp.SetPosition(new Rect(ColumnComponentX, y, 0, 0));
                y += ComponentRowSpacing;
            }

            // Column 3: ScriptableObjects, sorted by average Y of their referencing components
            // so heavily-referenced SOs land near their consumers (less edge criss-cross).
            var soOrdering = new List<(GraphNode node, float sortKey)>();
            foreach (GraphNode n in graph.Nodes)
            {
                if (n.Kind != NodeKind.ScriptableObject) continue;
                if (!visuals.ContainsKey(n.Id)) continue;

                float sum = 0f;
                int count = 0;
                foreach (GraphEdge e in graph.Edges)
                {
                    if (e.ToId != n.Id) continue;
                    if (!visuals.TryGetValue(e.FromId, out ArchitectureNode src)) continue;
                    if (src is not ComponentNode) continue;
                    sum += src.GetPosition().y;
                    count++;
                }
                float key = count > 0 ? sum / count : float.MaxValue; // unreferenced go last
                soOrdering.Add((n, key));
            }
            soOrdering.Sort((a, b) => a.sortKey.CompareTo(b.sortKey));

            float soY = 0f;
            foreach (var entry in soOrdering)
            {
                if (!visuals.TryGetValue(entry.node.Id, out ArchitectureNode soVisual)) continue;
                soVisual.SetPosition(new Rect(ColumnScriptableObjectX, soY, 0, 0));
                soY += ScriptableObjectRowSpacing;
            }
        }

        // ===================== OVERLAYS =====================

        private static VisualElement BuildLegend()
        {
            var legend = new VisualElement();
            legend.style.position = Position.Absolute;
            legend.style.bottom = 12;
            legend.style.left = 12;
            legend.style.paddingTop = 6;
            legend.style.paddingBottom = 6;
            legend.style.paddingLeft = 8;
            legend.style.paddingRight = 8;
            legend.style.backgroundColor = new StyleColor(new Color(0.10f, 0.10f, 0.10f, 0.88f));
            legend.style.borderTopLeftRadius = 4;
            legend.style.borderTopRightRadius = 4;
            legend.style.borderBottomLeftRadius = 4;
            legend.style.borderBottomRightRadius = 4;
            legend.pickingMode = PickingMode.Ignore;

            AddLegendRow(legend, ArchitectureColors.GameObject,       "GameObject",                 isLine: false);
            AddLegendRow(legend, ArchitectureColors.Component,        "Script (MonoBehaviour)",     isLine: false);
            AddLegendRow(legend, ArchitectureColors.ScriptableObject, "Data (ScriptableObject)",    isLine: false);

            var sep = new VisualElement();
            sep.style.height = 1;
            sep.style.marginTop = 5;
            sep.style.marginBottom = 5;
            sep.style.backgroundColor = new StyleColor(new Color(0.35f, 0.35f, 0.35f));
            legend.Add(sep);

            AddLegendRow(legend, ArchitectureColors.Containment,     "GameObject contains script", isLine: true);
            AddLegendRow(legend, ArchitectureColors.ObjectReference, "Script references object",   isLine: true);
            AddLegendRow(legend, ArchitectureColors.DataReference,   "Script reads data",          isLine: true);

            return legend;
        }

        private static void AddLegendRow(VisualElement parent, Color color, string text, bool isLine)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginTop = 1;
            row.style.marginBottom = 1;

            var swatch = new VisualElement();
            if (isLine)
            {
                swatch.style.width = 18;
                swatch.style.height = 2;
            }
            else
            {
                swatch.style.width = 14;
                swatch.style.height = 14;
                swatch.style.borderTopLeftRadius = 2;
                swatch.style.borderTopRightRadius = 2;
                swatch.style.borderBottomLeftRadius = 2;
                swatch.style.borderBottomRightRadius = 2;
            }
            swatch.style.backgroundColor = new StyleColor(color);
            swatch.style.marginRight = 8;
            row.Add(swatch);

            var label = new Label(text);
            label.style.color = new StyleColor(new Color(0.90f, 0.90f, 0.90f));
            label.style.fontSize = 11;
            row.Add(label);

            parent.Add(row);
        }

        private static Label BuildEmptyState()
        {
            var label = new Label(
                "No scripted GameObjects in this scene.\n\n" +
                "Add a MonoBehaviour or ScriptableObject reference, then click Refresh.");
            label.style.position = Position.Absolute;
            label.style.left = 0;
            label.style.right = 0;
            label.style.top = new Length(45, LengthUnit.Percent);
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.color = new StyleColor(new Color(0.65f, 0.65f, 0.65f));
            label.style.fontSize = 13;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.pickingMode = PickingMode.Ignore;
            label.style.display = DisplayStyle.None;
            return label;
        }
    }
}
