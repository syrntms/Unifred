using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Unifred
{
	public interface IHierarchyDrawer
	{
		bool IsEnable{
			get;
			set;
		}
		int GetPriority();
		void OnGUI(ref Rect r, int instanceID, Dictionary<int, object> log);
		object UpdateData(int instanceId);
	}
}
