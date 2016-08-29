using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace Unifred
{

	public class ManipulateInspector : MonoBehaviour {

		public static void SwitchLock()
		{
			var window = Resources.FindObjectsOfTypeAll(typeof(EditorWindow))
				.Select( w => w as EditorWindow)
				.Where( w => w.GetType().Name == "InspectorWindow" )
				.FirstOrDefault();

			if (window == null) {
				return;
			}

			PropertyInfo propertyInfo = window.GetType().GetProperty("isLocked");
			bool isLocked = (bool)propertyInfo.GetValue(window, null);
			propertyInfo.SetValue(window, !isLocked, null);
			window.Repaint();
		}
	}

}