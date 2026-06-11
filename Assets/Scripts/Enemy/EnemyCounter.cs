using System;
using UnityEngine;

namespace Enemy
{
    public class EnemyCounter : MonoBehaviour
    {
        private void OnEnable()
        {
            EnemyCountListener.RegisterEnemy();
        }

        private void OnDisable()
        {
            EnemyCountListener.DestroyEnemy();
        }
    }
}
