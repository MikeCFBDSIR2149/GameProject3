using TMPro;
using UnityEngine;

namespace UI.Cutscene
{
    /// <summary>
    /// Simple text step that only holds a reference to a TextMeshPro component.
    /// Enter/Exit behaviour uses the base class (activates/deactivates the GameObject).
    /// </summary>
    public class CutsceneTextStep : CutsceneStep
    {
        [SerializeField] private TMP_Text contentText;
    }
}
