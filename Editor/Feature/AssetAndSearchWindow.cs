using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Unifred
{

	public class AssetAndSearchWindow : UnifredWindowController<AssetAndSearchObject>
	{
		[MenuItem("Unifred/SearchAsset %t")]
		public static void SearchFromAsset()
		{
			ShowWindow(new AssetAndSearch(), string.Empty);
		}

		public static void SearchFromAsset(string initial_word)
		{
			ShowWindow(new AssetAndSearch(), initial_word);
		}
	}

	public class AssetAndSearch : UnifredFeatureBase<AssetAndSearchObject>
	{

		//about label 
		private static GUIStyle textGuiStyle = new GUIStyle {
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = {
				textColor = EditorStyles.label.normal.textColor,
			},
		};

		//about icon
		private static GUIStyle iconGuiStyle = new GUIStyle {};

		//set backgroudn color only
		private static GUIStyle defaultRowGuiStyle;

		//set backgroudn color only
		private static GUIStyle selectedRowGuiStyle;

		#region impl
		public override void OnInit()
		{
			defaultRowGuiStyle = new GUIStyle {
				normal = {background = TextureUtility.MakeSolidTexture(Color.clear),}
			};
			selectedRowGuiStyle = new GUIStyle {
				normal = { background = TextureUtility.MakeSolidTexture(Color.magenta + Color.gray * 1.25f),},
			};

			iconGuiStyle.fixedWidth = iconGuiStyle.fixedHeight = GetRowHeight();
		}

		public override void OnDestroy()
		{
			GameObject.DestroyImmediate(defaultRowGuiStyle.normal.background);
			GameObject.DestroyImmediate(selectedRowGuiStyle.normal.background);
		}

		public override string GetDescription()
		{
			return "input asset name you want <color=yellow>AND</color> search";
		}

		public override bool IsMultipleSelect()
		{
			return true;
		}

		public override IEnumerable<AssetAndSearchObject> UpdateCandidate(string input)
		{
			List<AssetAndSearchObject> result = new List<AssetAndSearchObject>();
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
				bool is_contain = words.All(
					(word_unit) => {return path.IndexOf(word_unit, StringComparison.OrdinalIgnoreCase) >= 0;}
				);
				if (!is_contain) {
					continue;
				}

				AssetAndSearchObject content = new AssetAndSearchObject();
				content.path = path;
				result.Add(content);
			}
			return result;
		}	

		public override void Draw(
			string word,
			IEnumerable<AssetAndSearchObject> result_list,
			IEnumerable<IntRange> selected_list,
			int offset,
			int count
		) {
			IEnumerable<int> uniq_selected_list = IntRange.Split(selected_list);
			for (int i = offset ; i < offset + count ; ++i) {
				bool is_selected = uniq_selected_list.Any((index) => {return index == i;});
				GUIStyle style = (is_selected)? selectedRowGuiStyle:defaultRowGuiStyle;

				AssetAndSearchObject result = result_list.ElementAt(i);

				Texture icon = UnityEditorInternal.InternalEditorUtility.GetIconForFile(result.path);
	            GUILayout.BeginHorizontal(style);
				GUILayout.Box(icon, iconGuiStyle);
	            GUILayout.Label(result.path, textGuiStyle);
	            GUILayout.EndHorizontal();
			}
		}

		public override void Select(string word, IEnumerable<AssetAndSearchObject> result_list)
		{
			if (string.IsNullOrEmpty(word)) {
				EditorApplication.delayCall += () => {
					AssetOrSearchWindow.SearchFromAsset();
				};
				return;
			}

			IEnumerable<AssetAndSearchObject> restore_list = result_list.Cast<AssetAndSearchObject>();
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
			_SaveHistory("AssetAndSearch", word);
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}
		#endregion
	}

	public class AssetAndSearchObject
	{
		public string path;
	};
}
