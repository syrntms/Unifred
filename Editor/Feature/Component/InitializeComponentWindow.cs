using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred.Feature
{

	public class InitializeComponentWindow : UnifredWindowController<InitializeComponentObject>
	{
		public static void ShowWindow()
		{
			ShowWindow(new InitializeComponent(), string.Empty);
		}
	}

	public class InitializeComponent : UnifredFeatureBase<InitializeComponentObject>
	{
		private static GUIStyle textGuiStyle = new GUIStyle
		{
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = Color.white, }
		};

		public override string GetDescription()
		{
			if (Selection.activeGameObject == null) {
				return "<color=white>select game object you want to Initialize</color>";
			}
			return "input component name you wanna to Initialize";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.Multiple;
		}

		public override IEnumerable<InitializeComponentObject> UpdateCandidate(string input)
		{
			List<InitializeComponentObject> result = new List<InitializeComponentObject>();
			Selection.gameObjects
				.SelectMany(go => go.GetComponents<Component>())
				.Select(c => new InitializeComponentObject(){component = c})
				.ForEach(obj => result.Add(obj));
		
			string[] words = input.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			return result.Where(
				obj => words.All( word => 
					obj.component.GetType().ToString().IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0
					|| obj.component.gameObject.name.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0
				)
			);
		}	

		public override void Draw(
			string word,
			InitializeComponentObject candidate,
			bool is_selected
		) {
			string text = string.Format("{0}::{1}",
				candidate.component.gameObject.name,
				candidate.component.GetType().ToString()
			);
			GUILayout.Label(text, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<InitializeComponentObject> result_list)
		{
			bool isOnlyEmptyField = !Input.IsPressedCommandKey();
			result_list.ForEach( obj => Initialize(obj.component, isOnlyEmptyField) );
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}

		private static void Initialize(Component component, bool isOnlyEmptyField)
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
					if (isOnlyEmptyField && iterator.objectReferenceValue != null) {
						continue;
					}
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

	[System.Serializable]
	public class InitializeComponentObject
	{
		public Component component;
	};
}
