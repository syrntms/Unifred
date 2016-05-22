using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred.Feature
{

	public class DeleteComponentWindow : UnifredWindowController<DeleteComponentObject>
	{
		public static void ShowWindow()
		{
			ShowWindow(new DeleteComponent(), string.Empty);
		}
	}

	public class DeleteComponent : UnifredFeatureBase<DeleteComponentObject>
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
				return "<color=white>select game you wanna delete component</color>";
			}
			return "input component name you wanna delete";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.Multiple;
		}

		public override IEnumerable<DeleteComponentObject> UpdateCandidate(string input)
		{
			if (string.IsNullOrEmpty(input)) {
				return Selection.gameObjects
					.SelectMany (go => go.GetComponents<Component>())
					.Select (c => new DeleteComponentObject (){ component = c });
			}

			string[] words = input.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);

			return Selection.gameObjects
				.SelectMany( go => go.GetComponents<Component>() )
				.Where( c => words.All( word => c.GetType().ToString().IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0))
				.Select( c => new DeleteComponentObject{component = c} );
		}	

		public override void Draw(
			string word,
			DeleteComponentObject candidate,
			bool is_selected
		) {
			GUILayout.Label(candidate.component.gameObject.name + ":" + candidate.component.GetType().ToString(), textGuiStyle);
		}

		public override void Select(
			string word,
			IEnumerable<DeleteComponentObject> result_list
		) {
			if (result_list.Count() <= 0) {
				return;
			}
			result_list.ForEach( c => GameObject.DestroyImmediate(c.component) );
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}
	}

	public class DeleteComponentObject
	{
		public Component component;
	};
}
