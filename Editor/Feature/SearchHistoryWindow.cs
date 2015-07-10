using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unifred.Utility;

namespace Unifred.Feature
{

	public class SearchHistoryWindow : UnifredWindowController<SearchHistoryObject>
	{
		[MenuItem("Unifred/SearchHistory %^")]
		public static void SearchFromHistory()
		{
			ShowWindow(new SearchHistory(), string.Empty);
		}
	}

	public class SearchHistory : UnifredFeatureBase<SearchHistoryObject>
	{
		private static GUIStyle textGuiStyle = new GUIStyle
		{
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = EditorStyles.label.normal.textColor }
		};

		//set backgroudn color only
		private static GUIStyle defaultRowGuiStyle;

		//set backgroudn color only
		private static GUIStyle selectedRowGuiStyle;

		public override string GetDescription()
		{
			return "input word you wanna search";
		}

		public override bool IsMultipleSelect()
		{
			return false;
		}

		public override void OnInit()
		{
			defaultRowGuiStyle = new GUIStyle {
				normal = {background = TextureUtility.MakeSolidTexture(Color.clear),}
			};
			selectedRowGuiStyle = new GUIStyle {
				normal = { background = TextureUtility.MakeSolidTexture(Color.magenta + Color.gray * 1.25f),},
			};
		}

		public override void OnDestroy()
		{
			GameObject.DestroyImmediate(defaultRowGuiStyle.normal.background);
			GameObject.DestroyImmediate(selectedRowGuiStyle.normal.background);
		}

		private IEnumerable<SearchHistoryObject> _GetAllHistory()
		{
			string encoded = EditorUserSettings.GetConfigValue("SearchHistory");
			if (string.IsNullOrEmpty(encoded)) {
				return new List<SearchHistoryObject>();
			}
			return SerializeUtility.DeserializeObject<List<SearchHistoryObject>>(encoded);
		}

		public override IEnumerable<SearchHistoryObject> UpdateCandidate(string input)
		{
			IEnumerable<SearchHistoryObject> result = _GetAllHistory();
            if (string.IsNullOrEmpty(input)) {
                return result;
			}
			var words = input.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

			return result.Where(
				(item) => words.All(
					(word) => item.input.IndexOf(input) >= 0
				)
			);
		}	

		public override void Draw(
			string word,
			IEnumerable<SearchHistoryObject> result_list,
			IEnumerable<IntRange> selected_list,
			int offset,
			int count
		) {

			IEnumerable<int> uniq_selected_list = IntRange.Split(selected_list);

			for (int i = offset ; i < offset + count ; ++i) {
				bool is_selected = uniq_selected_list.Any((index) => {return index == i;});
				GUIStyle style = (is_selected)? selectedRowGuiStyle:defaultRowGuiStyle;
				SearchHistoryObject result = result_list.ElementAt(i);
	            GUILayout.BeginHorizontal(style);
				string color;
				switch (result.mode){
				case SearchHistoryObject.SearchMode.AssetAndSearch:
					color = "red";
					break;
				case SearchHistoryObject.SearchMode.AssetOrSearch:
					color = "blue";
					break;
				case SearchHistoryObject.SearchMode.HierarchyAndSearch:
					color = "yellow";
					break;
				case SearchHistoryObject.SearchMode.HierarchyOrSearch:
					color = "green";
					break;
				default:
					color = "black";
					break;
				}
				var candidate_label = string.Format("<color={0}>{1}</color> {2}", color, result.mode, result.input);
	            GUILayout.Label(candidate_label, textGuiStyle);
	            GUILayout.EndHorizontal();
			}
		}

		public override void Select(string word, IEnumerable<SearchHistoryObject> result_list)
		{
			SearchHistoryObject result = result_list.First();
			switch (result.mode) {
			case SearchHistoryObject.SearchMode.AssetOrSearch:
				EditorApplication.delayCall += () => {
					AssetOrSearchWindow.SearchFromAsset();
				};
				break;
			case SearchHistoryObject.SearchMode.AssetAndSearch:
				EditorApplication.delayCall += () => {
					AssetAndSearchWindow.SearchFromAsset();
				};
				break;
			case SearchHistoryObject.SearchMode.HierarchyOrSearch:
				EditorApplication.delayCall += () => {
					HierarchyOrSearchWindow.SearchFromHierarchy();
				};
				break;
			case SearchHistoryObject.SearchMode.HierarchyAndSearch:
				EditorApplication.delayCall += () => {
					HierarchyAndSearchWindow.SearchFromHierarchy();
				};
				break;
			}
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}

	}

	public class SearchHistoryObject
	{
		public enum SearchMode{
			AssetAndSearch,
			AssetOrSearch,
			HierarchyAndSearch,
			HierarchyOrSearch,
		}
		public SearchMode mode;
		public string input;
	};
}
