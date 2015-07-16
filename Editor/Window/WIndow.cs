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

		public void OnAwake()
		{
			if (OnAwakeAction != null) {
				OnAwakeAction();
			}
		}

		public void OnEnable()
		{
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

		public static Action OnGUIAction;
		public static Action OnDestroyAction;
		public static Action OnAwakeAction;
		public static Action OnEnableAction;
		public static Action OnDisableAction;
	}
}
