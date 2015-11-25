using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Reflection;

public class ManipulateInspector : MonoBehaviour {

	public static void SwitchLock()
	{
		var window = Resources.FindObjectsOfTypeAll<EditorWindow>()
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
