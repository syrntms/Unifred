using System;
using UnityEditor;

namespace Unifred
{
	public class UnifredWindow : EditorWindow
	{
		public void OnGUI()
		{
			if (OnGUIAction != null) {
				OnGUIAction();
			}
		}

		public void OnDestroy()
		{
			if (OnDestroyAction != null) {
				OnDestroyAction();
			}
		}

		public void OnEnable()
		{
			UnityEngine.Debug.Log("enable");
			if (OnEnableAction != null) {
				OnEnableAction();
			}
		}

		public void OnDisable()
		{
			if (OnDisableAction != null) {
				OnDisableAction();
			}
		}

		public void Awake()
		{
			if (OnAwakeAction != null) {
				OnAwakeAction();
			}
		}

		public static Action OnGUIAction;
		public static Action OnDestroyAction;
		public static Action OnEnableAction;
		public static Action OnDisableAction;
		public static Action OnAwakeAction;
	}
}
