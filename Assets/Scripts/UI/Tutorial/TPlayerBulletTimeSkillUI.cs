using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Tutorial
{
    public class TPlayerBulletTimeSkillUI : UIBase
    {
        private (bool, bool) _checkmark;
        public GameObject checkmarkObject1;
        public GameObject checkmarkObject2;

        public override void UpdateUI(object data)
        {
            _checkmark = data as (bool, bool)? ?? (false, false);
            SetCheckmark(_checkmark);
        }

        private void SetCheckmark((bool, bool) check)
        {
            checkmarkObject1.SetActive(check.Item1);
            checkmarkObject2.SetActive(check.Item2);
        }
    }
}
