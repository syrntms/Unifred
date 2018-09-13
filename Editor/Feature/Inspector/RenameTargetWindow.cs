using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred.Feature
{
	public class RenameTargetWindow : UnifredWindowController<RenameTargetObject>
	{
		public static void ShowWindow()
		{
			ShowWindow(new RenameTarget(), string.Empty);
		}
	}

	public class RenameTarget : UnifredFeatureBase<RenameTargetObject>
	{
		public RenameTarget()
		{
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

		public override IEnumerable<RenameTargetObject> UpdateCandidate(string word)
		{
			List<RenameTargetObject> result = new List<RenameTargetObject>();
            if (string.IsNullOrEmpty(word)) {
                return result;
			}

			IEnumerable<GameObject> gameobjects = Selection.gameObjects;
			if (gameobjects == null || gameobjects.Count() == 0) {
				gameobjects = GameObjectUtility.FindAllInHierarchy();
			}

			foreach (var gameobject in gameobjects) {
				try{
					bool is_match = Regex.IsMatch(gameobject.name, word);
					if (!is_match) {
						continue;
					}
					RenameTargetObject content = new RenameTargetObject() {
						gameObject = gameobject,
					};
					result.Add(content);
				}
				catch(ArgumentException){
					// "("など特殊文字を含むパターンを入力途中でパースエラーが発生するため
					continue;
				}
			}
			return result;
		}	

		public override void Draw(
			string word,
			RenameTargetObject candidate,
			bool is_selected
		) {
			GUILayout.Label(candidate.gameObject.name, textGuiStyle);
		}

		public override void Select(
			string word,
			IEnumerable<RenameTargetObject> result_list
		) {
			if (result_list.Count() <= 0) {
				return;
			}

			IEnumerable<GameObject> param = result_list.Select(item => item.gameObject);
			EditorApplication.delayCall += () => {
				RenameWindow.ShowWindow(param, word);
			};
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}
	}

	public class RenameTargetObject
	{
		public GameObject gameObject;
	};
}
