using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Unifred.Feature
{
	public class ManipulateConsole : MonoBehaviour
	{
		public static void Clear()
		{
			var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
			var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			clearMethod.Invoke(null,null);
		}
	}
}