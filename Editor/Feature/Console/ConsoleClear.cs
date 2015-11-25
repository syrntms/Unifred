using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Unifred
{
	public class ConsoleClear : MonoBehaviour
	{
		public static void ClearConsole()
		{
			var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
			var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			clearMethod.Invoke(null,null);
		}
	}
}