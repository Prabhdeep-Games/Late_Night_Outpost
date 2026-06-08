// ============================================
// Runtime Set <T>
// ============================================
// A ScriptableObject that holds a runtime list of items registered by
// entities at play time. Entities self-register in OnEnable, unregister
// in OnDisable (or OnDestroy). Other systems iterate the set or subscribe
// to changes — no FindObjectsOfType, no static lists.
//
// USAGE — define a concrete registry asset type with one line:
//
//   [CreateAssetMenu(menuName = "Ludocore/Registries/Building Registry")]
//   public class BuildingRegistry : RuntimeSet<Building> { }
//
// Each Building references the registry asset and registers itself:
//
//   [SerializeField] private BuildingRegistry registry;
//   private void OnEnable()  => registry.TryAdd(this);
//   private void OnDisable() => registry.Remove(this);
//
// Other systems read the count, iterate, or subscribe:
//
//   text.text = $"Buildings: {registry.Count}";
//   registry.OnItemAdded += b => Debug.Log($"Built {b.name}");
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ludocore
{
    //==================== NON-GENERIC BASE =====================
    // Exists so a single custom inspector can target every concrete subclass
    // regardless of T. Students never reference this directly.
    public abstract class RuntimeSetBase : ScriptableObject
    {
        public abstract int Count { get; }
        public abstract IReadOnlyList<Object> ItemsAsObjects { get; }
        public abstract void Clear();
#if UNITY_EDITOR
        public abstract IReadOnlyList<Object> EditorListeners { get; }
#endif
    }

    //==================== GENERIC RUNTIME SET =====================
    public abstract class RuntimeSet<T> : RuntimeSetBase where T : Object
    {
        //==================== LIFECYCLE =====================
        [Header("Lifecycle")]
        [Tooltip("ON: list clears at play start. OFF: list persists (rare; for tooling).")]
        [SerializeField] private bool clearOnPlay = true;

        //==================== DEBUG =====================
        [Header("Debug")]
        [Tooltip("Log to console on Add/Remove. Per-asset toggle.")]
        [SerializeField] private bool debugLog = false;

        //==================== STATE =====================
        private readonly List<T> _items = new();
        private readonly HashSet<T> _index = new(); // O(1) Contains

        //==================== EVENT BACKERS =====================
        private Action<T> _onItemAdded;
        private Action<T> _onItemRemoved;
        private Action _onCleared;
        private Action _onModified;

        //==================== OUTPUTS =====================
        /// <summary>Raised when an item is added.</summary>
        public event Action<T> OnItemAdded
        {
            add { _onItemAdded += value; Track(value); }
            remove { _onItemAdded -= value; Untrack(value); }
        }

        /// <summary>Raised when an item is removed.</summary>
        public event Action<T> OnItemRemoved
        {
            add { _onItemRemoved += value; Track(value); }
            remove { _onItemRemoved -= value; Untrack(value); }
        }

        /// <summary>Raised when the set is cleared.</summary>
        public event Action OnCleared
        {
            add { _onCleared += value; Track(value); }
            remove { _onCleared -= value; Untrack(value); }
        }

        /// <summary>Raised after any add, remove, or clear. Generic "something changed" hook.</summary>
        public event Action Modified
        {
            add { _onModified += value; Track(value); }
            remove { _onModified -= value; Untrack(value); }
        }

        //==================== PUBLIC API =====================
        public override int Count => _items.Count;
        public bool IsEmpty => _items.Count == 0;
        public IReadOnlyList<T> Items => _items;

        public T this[int index] => _items[index];
        public T First => _items.Count > 0 ? _items[0] : null;
        public T Last => _items.Count > 0 ? _items[_items.Count - 1] : null;

        public bool Contains(T item) => item != null && _index.Contains(item);

        /// <summary>Add an item. Returns false if null or already present (idempotent).</summary>
        public bool TryAdd(T item)
        {
            if (item == null || _index.Contains(item)) return false;
            _items.Add(item);
            _index.Add(item);
            if (debugLog)
                Debug.Log($"<color=#f75369>[Registry]</color> {name} + {item.name} (count={_items.Count})", this);
            _onItemAdded?.Invoke(item);
            _onModified?.Invoke();
            return true;
        }

        /// <summary>Remove an item. Returns false if not present.</summary>
        public bool Remove(T item)
        {
            if (item == null || !_index.Remove(item)) return false;
            _items.Remove(item);
            if (debugLog)
                Debug.Log($"<color=#f75369>[Registry]</color> {name} - {item.name} (count={_items.Count})", this);
            _onItemRemoved?.Invoke(item);
            _onModified?.Invoke();
            return true;
        }

        public override void Clear()
        {
            if (_items.Count == 0) return;
            _items.Clear();
            _index.Clear();
            if (debugLog)
                Debug.Log($"<color=#f75369>[Registry]</color> {name} cleared", this);
            _onCleared?.Invoke();
            _onModified?.Invoke();
        }

        /// <summary>Iterate backwards so listeners can remove themselves safely during dispatch.</summary>
        public void ForEach(Action<T> action)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
                action(_items[i]);
        }

        //==================== UNITY LIFECYCLE =====================
        private void Awake()
        {
            // Prevents Unity from unloading the SO when nothing temporarily references it.
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        private void OnEnable()
        {
            // Clear stale subscribers and state from previous play session.
            _onItemAdded = null;
            _onItemRemoved = null;
            _onCleared = null;
            _onModified = null;
#if UNITY_EDITOR
            _editorListeners.Clear();
#endif
            if (clearOnPlay)
            {
                _items.Clear();
                _index.Clear();
            }
        }

        //==================== EDITOR =====================
#if UNITY_EDITOR
        private readonly List<Object> _editorListeners = new();
        public override IReadOnlyList<Object> EditorListeners => _editorListeners;
#endif

        // IReadOnlyList<T> where T : Object is covariant to IReadOnlyList<Object>.
        public override IReadOnlyList<Object> ItemsAsObjects => _items;

        //==================== LISTENER TRACKING =====================
        private void Track(Delegate d)
        {
#if UNITY_EDITOR
            if (d?.Target is Object o && !_editorListeners.Contains(o))
                _editorListeners.Add(o);
#endif
        }

        private void Untrack(Delegate d)
        {
#if UNITY_EDITOR
            if (d?.Target is Object o) _editorListeners.Remove(o);
#endif
        }
    }
}
