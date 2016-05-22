using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred.Feature
{

	public class SearchHistoryWindow : UnifredWindowController<SearchHistoryObject>
	{
		public static void ShowWindow()
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
			normal = { textColor = Color.white, }
		};

		public override string GetDescription()
		{
			return "input word you wanna search";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.Single;
		}

		private IEnumerable<SearchHistoryObject> _GetAllHistory()
		{
			var histories = _GetHistory();
			return histories.Select(
				history => new SearchHistoryObject() {
					mode	= _GetModeFromHistoryKey(history.Key),
					input	= history.Value
				}
			);
		}

		private SearchHistoryObject.SearchMode _GetModeFromHistoryKey(string key) 
		{
			switch (key) {
			case "AssetAndSearch":
				return SearchHistoryObject.SearchMode.AssetAndSearch;
			case "AssetOrSearch":
				return SearchHistoryObject.SearchMode.AssetOrSearch;
			case "HierarchyAndSearch":
				return SearchHistoryObject.SearchMode.HierarchyAndSearch;
			case "HierarchyOrSearch":
				return SearchHistoryObject.SearchMode.HierarchyOrSearch;
			default:
				throw new Exception("error history mode[key = " + key + "]");
			}
		}

		public override IEnumerable<SearchHistoryObject> UpdateCandidate(string input)
		{
			IEnumerable<SearchHistoryObject> result = _GetAllHistory().Reverse();
            if (string.IsNullOrEmpty(input)) {
                return result;
			}
			var words = input.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			return result.Where(
				(item) => words.All(
					(word) => item.input.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0
				)
			);
		}	

		public override void Draw(
			string word,
			SearchHistoryObject candidate,
			bool is_selected
		) {
			string color;
			switch (candidate.mode){
			case SearchHistoryObject.SearchMode.AssetAndSearch:
			case SearchHistoryObject.SearchMode.AssetOrSearch:
				color = "yellow";
				break;
			case SearchHistoryObject.SearchMode.HierarchyOrSearch:
			case SearchHistoryObject.SearchMode.HierarchyAndSearch:
				color = "green";
				break;
			default:
				color = "white";
				break;
			}
			var candidate_label = string.Format("<color={0}>{1}</color> {2}", color, candidate.mode, candidate.input);
            GUILayout.Label(candidate_label, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<SearchHistoryObject> result_list)
		{
			SearchHistoryObject result = result_list.First();
			switch (result.mode) {
			case SearchHistoryObject.SearchMode.AssetOrSearch:
				EditorApplication.delayCall += () => {
					ProjectOrSearchWindow.ShowWindow(result.input);
				};
				break;
			case SearchHistoryObject.SearchMode.AssetAndSearch:
				EditorApplication.delayCall += () => {
					ProjectAndSearchWindow.ShowWindow(result.input);
				};
				break;
			case SearchHistoryObject.SearchMode.HierarchyOrSearch:
				EditorApplication.delayCall += () => {
					HierarchyOrSearchWindow.ShowWindow(result.input);
				};
				break;
			case SearchHistoryObject.SearchMode.HierarchyAndSearch:
				EditorApplication.delayCall += () => {
					HierarchyAndSearchWindow.ShowWindow(result.input);
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
		public enum SearchMode
		{
			AssetAndSearch,
			AssetOrSearch,
			HierarchyAndSearch,
			HierarchyOrSearch,
		}
		public SearchMode mode;
		public string input;
	};
}
