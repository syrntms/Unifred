using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred.Feature
{

	public class ManipulateComponent
	{
		public static void InitializeField()
		{
			bool is_select = Selection.gameObjects.Count() > 0;
			if (!is_select) {
				return;
			}

			foreach (var go in Selection.gameObjects) {
				go.GetComponents<Component>().ForEach( c => Initialize(c) );
			}
		}

		private static void Initialize(Component component)
		{
			var to_serialized	= new SerializedObject(component);
			var iterator		= to_serialized.GetIterator();

			while (iterator.Next(true)) {

				if (iterator.propertyType == SerializedPropertyType.Generic) {
					if (iterator.name.EndsWith("ListItems", StringComparison.OrdinalIgnoreCase)) {
						string parent_name = iterator.name.Substring(0, iterator.name.Length - "Items".Length);
						GameObject parent_go = _FindObjectIgnoreCase(component.gameObject, parent_name);
						if (parent_go == null) {
							continue;
						}
						int length = parent_go.transform.childCount;
						iterator.ClearArray();
						iterator.arraySize = length;
						for (int i = 0 ; i < length ; ++i) {
							var prop = iterator.GetArrayElementAtIndex(i);
							if (prop.propertyType != SerializedPropertyType.ObjectReference) {
								continue;
							}
							prop.objectReferenceValue = parent_go.transform.GetChild(i).gameObject;
						}
						iterator = iterator.GetArrayElementAtIndex(length - 1);
						continue;
					}
				}
					
				if (iterator.propertyType == SerializedPropertyType.ObjectReference) {
					bool isBlackList = GetBlackList().Any(path => iterator.propertyPath.Contains(path));
					if (isBlackList) {
						continue;
					}

					if (iterator.name.EndsWith("prefab", StringComparison.OrdinalIgnoreCase)) {
						iterator.objectReferenceValue = _FindAssetIgnoreCase(
							_GetFileName(iterator.name, "prefab"),
							".prefab"
						);
					}
					else {
						iterator.objectReferenceValue = _FindObjectIgnoreCase(component.gameObject, iterator.name);
					}
				}
			}
			to_serialized.ApplyModifiedProperties();
			to_serialized.UpdateIfDirtyOrScript();
		}

		private static IEnumerable<string> GetBlackList()
		{
			return new string[]{
				"m_GameObject",
				"m_Script",
				"m_Prefab",
				"m_Children",
				"m_Father",
			};
		}

		private static GameObject _FindObjectIgnoreCase(GameObject root, string name)
		{
			// find from children
			GameObject child = root.GetComponentsInChildren<Component>(true)
				.Select( c => c.gameObject )
				.Distinct()
				.Where( go => go.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0 )
				.FirstOrDefault( go => go.name.Length == name.Length );
			if (child != null) {
				return child;
			}

			// find from scene
			GameObject other = GameObjectUtility.FindAllInHierarchy()
				.Where( go => go.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0 )
				.FirstOrDefault( go => go.name.Length == name.Length );

			return other;
		}

		private static UnityEngine.Object _FindAssetIgnoreCase(string word, string ext)
		{
			string prefab_name = word + ext;
			var guid_list = AssetDatabase.FindAssets(word);
			foreach (var guid in guid_list) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var filename = path.Split(new char[]{'/'}, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
				var is_same = filename.IndexOf(prefab_name, StringComparison.OrdinalIgnoreCase) >= 0 
					&& filename.Length == prefab_name.Length;

				if (!is_same) {
					continue;
				}
				return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
			}
			return null;
		}

		private static string _GetFileName(string word, string extWithoutPeriod)
		{
			return word.Substring(0, word.Length - extWithoutPeriod.Length);
		}
	}
}
