using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Tutorial
{
    public class TPlayerMotorUI : UIBase
    {
        private bool _checkmark;
        public GameObject checkmarkObject;

        public override void UpdateUI(object data)
        {
            _checkmark = data is true;
            SetCheckmark(_checkmark);
        }

        private void SetCheckmark(bool check)
        {
            checkmarkObject.SetActive(check);
        }
    }
}
