using System;
using UnityEngine;

namespace Enemy
{
	[DefaultExecutionOrder(-500)]
	public class EnemyCountListener : MonoBehaviour
	{
		private static EnemyCountListener _instance;

		private int _enemyCount;
		private static bool _allowTrigger;

		private void Awake()
		{
			Debug.Log("EnemyCountListener Awake");
			_instance = this;
			_allowTrigger = true;
		}

		public static void RegisterEnemy()
		{
			_instance._enemyCount++;
			// Debug.Log($"Enemy Count: {_instance._enemyCount}");
		}

		public static void DestroyEnemy()
		{
			_instance._enemyCount--;
			// Debug.Log($"Enemy Count: {_instance._enemyCount}");

			if (_instance._enemyCount <= 0)
			{
				if (!_allowTrigger) return;
				if (GameplayManager.Instance == null || GameplayManager.Instance.doNotTriggerListener)
				{
					return;
				}
				GameplayManager.Instance.RequestGameWin();
			}
		}

		private void OnDestroy()
		{
			_allowTrigger = false;
		}
	}
}