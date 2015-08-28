using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Unifred
{

	public class AssetOrSearchWindow : UnifredWindowController<AssetOrSearchObject>
	{
		public static void SearchFromAsset()
		{
			ShowWindow(new AssetOrSearch(), "");
		}

		public static void SearchFromAsset(string initialize_word)
		{
			ShowWindow(new AssetOrSearch(), initialize_word);
		}
	}

	public class AssetOrSearch : UnifredFeatureBase<AssetOrSearchObject>
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
			return "input asset name you want <color=white>OR</color> search";
		}

		public override bool IsMultipleSelect()
		{
			return true;
		}

		public override IEnumerable<AssetOrSearchObject> UpdateCandidate(string input)
		{
			List<AssetOrSearchObject> result = new List<AssetOrSearchObject>();
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

				AssetOrSearchObject content = new AssetOrSearchObject();
				content.path = path;
				result.Add(content);
			}
			return result;
		}	

		public override void Draw(
			string word,
			AssetOrSearchObject candidate,
			bool is_selected
		) {
			Texture icon = UnityEditorInternal.InternalEditorUtility.GetIconForFile(candidate.path);
			GUILayout.Box(icon, iconGuiStyle);
            GUILayout.Label(candidate.path, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<AssetOrSearchObject> result_list)
		{
			if (string.IsNullOrEmpty(word)) {
				EditorApplication.delayCall += () => {
					AssetAndSearchWindow.SearchFromAsset();
				};
				return;
			}

			IEnumerable<AssetOrSearchObject> restore_list = result_list.Cast<AssetOrSearchObject>();
			Selection.objects = restore_list.Select(
				(t) => {
					return AssetDatabase.LoadAssetAtPath(t.path, typeof(Object));
				}
			).ToArray();

			if (Input.IsPressedOpenKey()) {
				foreach (var obj in Selection.objects) {
					AssetDatabase.OpenAsset(obj);
				}
			}
			else {
				EditorApplication.ExecuteMenuItem("Window/Project");
			    EditorUtility.FocusProjectWindow();
			}
			_SaveHistory("AssetOrSearch", word);
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}
		#endregion

	}

	public class AssetOrSearchObject
	{
		public string path;
	};
}
