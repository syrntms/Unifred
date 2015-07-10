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
		public Action OnGUIAction;
		public Action OnDestroyAction;
	}
}
