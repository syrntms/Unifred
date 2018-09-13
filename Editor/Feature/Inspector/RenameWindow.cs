using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred.Feature
{
	public class RenameWindow : UnifredWindowController<RenameObject>
	{
		public static void ShowWindow(IEnumerable<GameObject> list, string word)
		{
			ShowWindow(new Rename(list, word), string.Empty);
		}
	}

	public class Rename : UnifredFeatureBase<RenameObject>
	{
		private IEnumerable<RenameObject> list;
		private string from;
		public Rename(IEnumerable<GameObject> list, string word)
		{
			this.list = list.Select( (item, index) => new RenameObject(){ gameObject = item, order = index} );
			from = word;
		}

		private static GUIStyle textGuiStyle = new GUIStyle {
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = Color.white }
		};

		public override string GetDescription()
		{
			return "input name you want rename";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.Multiple;
		}

		public override IEnumerable<RenameObject> UpdateCandidate(string line)
		{
			return list;
		}	

		public override void Draw(
			string word,
			RenameObject candidate,
			bool is_selected
		) {
			string output = string.Empty;
			var matchCollection = Regex.Matches(candidate.gameObject.name, from).Cast<Match>().OrderByDescending(t => t.Index);

			if (string.IsNullOrEmpty(word)) {
				output = candidate.gameObject.name;
				foreach (var match in matchCollection) {
					var before = output.Substring(0, match.Index);
					var after = output.Substring(match.Index + match.Length);
					output = before + "<color=#00AFFF>"+ match.Value + "</color>" + after;
				}
			} else {
				output = candidate.gameObject.name;
				foreach (var match in matchCollection) {
					var before = output.Substring(0, match.Index);
					var after = output.Substring(match.Index + match.Length);
					output = before + "<color=#00AFFF>"+ word + "</color>" + after;
				}
				try{
					string.Format(output, candidate.order);
				}
				catch(FormatException) {
					// 入力途中ではパースできない公文になっていてエラーになるケースが考えられる
				}
			}

            GUILayout.Label(output, textGuiStyle);
		}

		public override void Select(
			string word,
			IEnumerable<RenameObject> result_list
		) {
			foreach(var candidate in list) {
				var matchCollection = Regex.Matches(candidate.gameObject.name, from).Cast<Match>().OrderByDescending(t => t.Index);
				string output = string.Empty;

				if (string.IsNullOrEmpty(word)) {
					foreach (var match in matchCollection) {
						var before = output.Substring(0, match.Index);
						var after = output.Substring(match.Index + match.Length);
						output = before + after;
					}
				} else {
					output = candidate.gameObject.name;
					foreach (var match in matchCollection) {
						var before = output.Substring(0, match.Index);
						var after = output.Substring(match.Index + match.Length);
						output = string.Format(before + word + after, candidate.order);
					}
				}
				candidate.gameObject.name = output;
			}
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}
	}

	public class RenameObject
	{
		public GameObject gameObject;
		public int order;
	};
}
