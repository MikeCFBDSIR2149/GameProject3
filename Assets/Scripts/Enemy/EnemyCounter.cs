using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemy
{
    public class EnemyCounter : MonoBehaviour
    {
        [SerializeField] private bool triggerListener = true;

        private void OnEnable()
        {
            EnemyCountListener.RegisterEnemy();
        }

        private void OnDisable()
        {
            EnemyCountListener.DestroyEnemy(triggerListener);
        }
    }
}
