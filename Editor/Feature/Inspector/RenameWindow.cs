using System;
using System.Linq;
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
			this.list = list.Select( item => new RenameObject(){ gameObject = item, } );
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
			int start = candidate.gameObject.name.IndexOf(from, StringComparison.OrdinalIgnoreCase);
			int count = from.Length;

			if (string.IsNullOrEmpty(word)) {
				output = candidate.gameObject.name
					.Insert(start + count, "</color>")
					.Insert(start, "<color=#00AFFF>");
			}
			else {
				string oldWord = candidate.gameObject.name.Substring(start, count);
				output = candidate.gameObject.name.Replace(oldWord, "<color=#00AFFF>" + word + "</color>");
			}

            GUILayout.Label(output, textGuiStyle);
		}

		public override void Select(
			string word,
			IEnumerable<RenameObject> result_list
		) {
			foreach(var candidate in list) {
				int start = candidate.gameObject.name.IndexOf(from, StringComparison.OrdinalIgnoreCase);
				int count = from.Length;
				string oldWord = candidate.gameObject.name.Substring(start, count);
				candidate.gameObject.name = candidate.gameObject.name.Replace(oldWord, word);
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
	};
}
