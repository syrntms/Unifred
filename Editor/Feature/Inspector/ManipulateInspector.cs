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

		public static void DrawIcon()
		{
			Selection.gameObjects.ForEach(
				go => _DrawIcon(go, 1)
			);
		}

		private static void _DrawIcon(GameObject gameObject, int idx)
		{
			var largeIcons = _GetTextures("sv_label_", string.Empty, 0, 8);
			var icon = largeIcons[idx];
			var egu = typeof(EditorGUIUtility);
			var flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
			var args = new object[] { gameObject, icon.image };
			var setIcon = egu.GetMethod("SetIconForObject", flags, null, new Type[]{typeof(UnityEngine.Object), typeof(Texture2D)}, null);
			setIcon.Invoke(null, args);
		}

		private static GUIContent[] _GetTextures(string baseName, string postFix, int startIndex, int count)
		{
			GUIContent[] array = new GUIContent[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = EditorGUIUtility.IconContent(baseName + (startIndex + i) + postFix);
			}
			return array;
		}
	}

}