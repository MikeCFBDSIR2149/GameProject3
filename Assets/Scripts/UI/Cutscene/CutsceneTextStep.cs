using TMPro;
using UnityEngine;

namespace UI.Cutscene
{
    /// <summary>
    /// Simple text step that only holds a reference to a TextMeshPro component.
    /// Enter/Exit behavior uses the base class (activates/deactivates the GameObject).
    /// </summary>
    public class CutsceneTextStep : CutsceneStep
    {
        [SerializeField] private TMP_Text contentText;

        [Tooltip("Seconds to wait between revealing each character (smaller = faster)")]
        [SerializeField, Min(0f)] private float secondsPerCharacter = 0.1f;

        // Internal state for typing coroutine
        private string _fullText = string.Empty;
        private Coroutine _typingCoroutine;
        private bool _isTypingFinished = true;

        public override void Enter()
        {
            base.Enter();

            if (contentText == null)
                return;

            // Capture the full text and start from empty
            _fullText = contentText.text ?? string.Empty;
            contentText.text = string.Empty;
            _isTypingFinished = false;

            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }

            _typingCoroutine = StartCoroutine(TypeRoutine());
        }

        /// <summary>
        /// Stop typing when the step exits. If typing was in progress, reveal the
        /// full text and stop the coroutine, then call base Exit which will
        /// deactivate the GameObject.
        /// </summary>
        public override bool Exit()
        {
            if (!_isTypingFinished)
            {
                if (_typingCoroutine != null)
                {
                    StopCoroutine(_typingCoroutine);
                    _typingCoroutine = null;
                }

                _isTypingFinished = true;

                if (contentText != null)
                {
                    contentText.text = _fullText;
                }

                return false;
            }

            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }

            if (contentText != null)
            {
                contentText.text = _fullText;
            }

            return base.Exit();
        }

        private System.Collections.IEnumerator TypeRoutine()
        {
            if (!contentText)
                yield break;
            
            float secsPerChar = secondsPerCharacter > 0f ? secondsPerCharacter : 0f;

            // Iterate through the full text, but handle TMP/HTML-like tags so they
            // are not revealed character-by-character (keeps tags intact).
            for (int i = 0; i < _fullText.Length; i++)
            {
                // If we encounter a tag start, copy until the tag end without delay
                if (_fullText[i] == '<')
                {
                    int tagEnd = _fullText.IndexOf('>', i);
                    if (tagEnd == -1) // malformed tag, just append the rest
                    {
                        contentText.text += _fullText.Substring(i);
                        break;
                    }

                    contentText.text += _fullText.Substring(i, tagEnd - i + 1);
                    i = tagEnd; // advance index to end of tag
                    continue;
                }

                contentText.text += _fullText[i];

                if (secsPerChar > 0f)
                    yield return new WaitForSecondsRealtime(secsPerChar);
                else
                    yield return null;
            }

            // Typing finished
            _isTypingFinished = true;
            _typingCoroutine = null;
        }
    }
}
