using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Unifred.Feature
{

	public class ProjectOrSearchWindow : UnifredWindowController<ProjectOrSearchObject>
	{
		public static void ShowWindow()
		{
			ShowWindow(new ProjectOrSearch(), "");
		}

		public static void ShowWindow(string initialize_word)
		{
			ShowWindow(new ProjectOrSearch(), initialize_word);
		}
	}

	public class ProjectOrSearch : UnifredFeatureBase<ProjectOrSearchObject>
	{
		//about label 
		private static GUIStyle textGuiStyle = new GUIStyle {
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = {
				textColor = Color.white,
			},
		};

		//about icon
		private static GUIStyle iconGuiStyle = new GUIStyle {};

		#region impl
		public override void OnInit()
		{
			iconGuiStyle.fixedWidth = iconGuiStyle.fixedHeight = GetRowHeight();
		}

		public override string GetDescription()
		{
			return "Input name you wanna search. "
				+ "<color=white>Space</color> = OR. "
				+ "<color=white>Ctrl</color> = FileOpen.";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.Multiple;
		}

		public override IEnumerable<ProjectOrSearchObject> UpdateCandidate(string input)
		{
			List<ProjectOrSearchObject> result = new List<ProjectOrSearchObject>();
            if (string.IsNullOrEmpty(input)) {
                return result;
			}

			string[] words = input.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			if (words.Length <= 0) {
				return result;
			}

			List<string> guid_list = new List<string>();
			foreach (var word in words) {
				var added = AssetDatabase.FindAssets(word);
				guid_list.AddRange(added);
			}
			guid_list = guid_list.Distinct().ToList();

			foreach (var guid in guid_list) {
				string path = AssetDatabase.GUIDToAssetPath(guid);;
				bool is_contain = words.Any(
					(word_unit) => {return path.IndexOf(word_unit, StringComparison.OrdinalIgnoreCase) >= 0;}
				);
				if (!is_contain) {
					continue;
				}

				ProjectOrSearchObject content = new ProjectOrSearchObject();
				content.path = path;
				result.Add(content);
			}
			return result;
		}	

		public override void Draw(
			string word,
			ProjectOrSearchObject candidate,
			bool is_selected
		) {
			Texture icon = UnityEditorInternal.InternalEditorUtility.GetIconForFile(candidate.path);
			GUILayout.Box(icon, iconGuiStyle);
            GUILayout.Label(candidate.path, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<ProjectOrSearchObject> result_list)
		{
			if (string.IsNullOrEmpty(word)) {
				EditorApplication.delayCall += () => { ProjectAndSearchWindow.ShowWindow(); };
				return;
			}

			if (Input.IsPressedCommandKey()) {
				_OpenAssets(result_list);
			}
			else {
				_SelectAssets(result_list);
			}

			EditorApplication.ExecuteMenuItem("Window/General/Project");
		    EditorUtility.FocusProjectWindow();
			_SaveHistory("AssetOrSearch", word);
		}

		private void _OpenAssets(IEnumerable<ProjectOrSearchObject> result_list)
		{
			IEnumerable<UnityEngine.Object> objects = result_list
				.Select( t => AssetDatabase.LoadAssetAtPath(t.path, typeof(Object)) )
				.ToArray();

			foreach (var obj in objects) {
				if (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab) {
					foreach (GameObject active_go in Selection.gameObjects) {
						_CreatePrefab(obj, active_go);
					}
					if (Selection.gameObjects.Count() <= 0) {
						_CreatePrefab(obj, null);
					}
				}
				else {
					AssetDatabase.OpenAsset(obj);
				}
			}
		}

		private void _CreatePrefab(UnityEngine.Object prefab, GameObject parent)
		{
			GameObject go = null;
			if (Input.IsPressedShiftKey()) {
				go = GameObject.Instantiate(prefab) as GameObject;
			}
			else {
				go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
			}
			Transform parent_transform = parent == null ? null:parent.transform;
			go.transform.SetParent(parent_transform);
			go.transform.localPosition = Vector3.zero;
			go.name = prefab.name;
		}

		private void _SelectAssets(IEnumerable<ProjectOrSearchObject> result_list)
		{
			Selection.objects = result_list
				.Select( t => AssetDatabase.LoadAssetAtPath(t.path, typeof(Object)) )
				.ToArray();
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}
		#endregion

	}

	public class ProjectOrSearchObject
	{
		public string path;
	};
}
