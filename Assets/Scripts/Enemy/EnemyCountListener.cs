using UnityEngine;

namespace Enemy
{
	[DefaultExecutionOrder(-500)]
	public class EnemyCountListener : MonoBehaviour
	{
		private static EnemyCountListener _instance;

		private int _enemyCount;

		private void Awake()
		{
			Debug.Log("EnemyCountListener Awake");
			_instance = this;
		}

		public static void RegisterEnemy()
		{
			_instance._enemyCount++;
			Debug.Log($"Enemy Count: {_instance._enemyCount}");
		}

		public static void DestroyEnemy()
		{
			_instance._enemyCount--;
			Debug.Log($"Enemy Count: {_instance._enemyCount}");

			if (_instance._enemyCount <= 0)
			{
				if (GameplayManager.Instance == null)
				{
					return;
				}
				GameplayManager.Instance.RequestGameWin();
			}
		}
	}
}