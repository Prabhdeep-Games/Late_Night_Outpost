// ============================================================================
// ArchitectureVisualizerWindow — Tools > Architecture Visualizer
//
// EditorWindow that hosts an ArchitectureGraphView. Toolbar offers a Refresh
// button and a search field that dims non-matching nodes. Auto-rebuilds when
// the loaded scene set changes so students don't have to remember to refresh.
//
// Read-only by design: students can pan, zoom, drag nodes for layout, click a
// node to ping the underlying object — but nothing in here modifies game state.
// ============================================================================

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Ludocore.Editor.ArchitectureVisualizer
{
    public class ArchitectureVisualizerWindow : EditorWindow
    {
        private ArchitectureGraphView _graphView;
        private ToolbarSearchField _searchField;

        [MenuItem("Tools/Architecture Visualizer")]
        public static void Open()
        {
            ArchitectureVisualizerWindow w = GetWindow<ArchitectureVisualizerWindow>();
            w.titleContent = new GUIContent("Architecture Visualizer");
            w.minSize = new Vector2(400, 300);
            w.Show();
        }

        private void OnEnable()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneClosed;
        }

        private void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var toolbar = new Toolbar();

            var refresh = new ToolbarButton(Rebuild) { text = "Refresh" };
            toolbar.Add(refresh);

            var showAll = new ToolbarButton(Rebuild) { text = "Show All" };
            toolbar.Add(showAll);

            var hint = new Label("Click node to ping  ·  Right-click for hide / show all");
            hint.style.alignSelf = Align.Center;
            hint.style.marginLeft = 12;
            hint.style.color = new StyleColor(new Color(0.55f, 0.55f, 0.55f));
            hint.style.fontSize = 10;
            toolbar.Add(hint);

            _searchField = new ToolbarSearchField();
            _searchField.style.flexGrow = 1;
            _searchField.style.marginLeft = 8;
            _searchField.RegisterValueChangedCallback(evt => _graphView?.ApplySearch(evt.newValue));
            toolbar.Add(_searchField);

            root.Add(toolbar);

            _graphView = new ArchitectureGraphView { name = "architecture-graph" };
            _graphView.style.flexGrow = 1;
            _graphView.OnRequestRebuild = Rebuild;
            root.Add(_graphView);

            Rebuild();
        }

        private void Rebuild()
        {
            if (_graphView == null) return;

            SceneGraph graph = SceneGraphBuilder.Build();
            _graphView.Populate(graph);

            if (_searchField != null && !string.IsNullOrEmpty(_searchField.value))
                _graphView.ApplySearch(_searchField.value);

            _graphView.FrameAll();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode) => Rebuild();
        private void OnSceneClosed(Scene scene) => Rebuild();
    }
}
