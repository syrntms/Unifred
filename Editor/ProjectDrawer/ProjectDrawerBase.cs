using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Unifred
{
	public abstract class ProjectDrawerBase
	{
		public void SetEnable(bool value)
		{
			EditorUserSettings.SetConfigValue(GetType().ToString(), value.ToString());
		}

		public bool GetEnabled()
		{
			var enableFlag = EditorUserSettings.GetConfigValue(GetType().ToString());
			if (enableFlag == null) {
				return false;
			}
			else {
				return bool.Parse(enableFlag);
			}
		}

		protected abstract int ScaleX { get; }
		protected virtual Rect CalcRect(Rect r) {
			r.height = 16;
			r.width = ScaleX;
			r.x -= (ScaleX - 4);
			return r;
		}
		public abstract int GetPriority();
		public abstract void OnGUI(ref Rect r, string guid);
	}
}
