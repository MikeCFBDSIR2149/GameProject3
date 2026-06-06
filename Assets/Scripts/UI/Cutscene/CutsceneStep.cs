using UnityEngine;

namespace UI.Cutscene
{
	/// <summary>
	/// Base class for cutscene steps. Provides default Enter/Exit which
	/// simply toggles the GameObject active state. Override to implement
	/// custom behaviour.
	/// </summary>
	public abstract class CutsceneStep : MonoBehaviour
	{
		public virtual void Enter()
		{
			gameObject.SetActive(true);
		}

		public virtual bool Exit()
		{
			gameObject.SetActive(false);
			return true;
		}
	}
}