// ============================================
// Int Variable Label
// ============================================
// Binder: subscribes to an IntVariable and writes its value into a TMP label.
// Use for DayCount, ResourceCount, anything stored in an IntVariable.
// ============================================

using TMPro;
using UnityEngine;

namespace Ludocore
{
    public class IntVariableLabel : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Source")]
        [Tooltip("Variable whose value drives the label.")]
        [SerializeField] private IntVariable variable;

        [Header("Display")]
        [Tooltip("Text component to update.")]
        [SerializeField] private TMP_Text label;

        [Tooltip("Format string. {0} is the int value. Examples: 'Day {0}', 'Wood: {0}'.")]
        [SerializeField] private string format = "Day {0}";

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            variable.OnChanged += Refresh;
            Refresh(variable.Value);
        }

        private void OnDisable() => variable.OnChanged -= Refresh;

        //==================== PRIVATE =====================
        private void Refresh(int value) => label.text = string.Format(format, value);
    }
}
