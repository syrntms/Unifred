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
			bool isSelect = Selection.gameObjects.Count() > 0;
			if (!isSelect) {
				return;
			}

			foreach (var go in Selection.gameObjects) {
				go.GetComponents<Component>().ForEach( c => Initialize(c) );
			}
		}

		private static void Initialize(Component component)
		{
			var toSerialized	= new SerializedObject(component);
			var iterator		= toSerialized.GetIterator();

			while (iterator.Next(true))
			{
				if (iterator.propertyType != SerializedPropertyType.ObjectReference) {
					continue;
				}

				bool isBlackList = GetBlackList().Any(path => iterator.propertyPath.Contains(path));
				if (isBlackList) {
					continue;
				}

				iterator.objectReferenceValue = _FindObjectIgnoreCase(component.gameObject, iterator.name);
			}
			toSerialized.ApplyModifiedProperties();
			toSerialized.UpdateIfDirtyOrScript();
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

			GameObject child = root.GetComponentsInChildren<Component>(true)
				.Select( c => c.gameObject )
				.Distinct()
				.Where( go => go.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0 )
				.FirstOrDefault();
			if (child != null) {
				return child;
			}

			GameObject other = GameObjectUtility.FindAllInHierarchy()
				.Where( go => go.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0 )
				.FirstOrDefault();
			return other;
		}
	}
}
