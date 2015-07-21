using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred
{
	public class MethodListWindow : UnifredWindowController<MethodListObject>
	{

		[MenuItem("Unifred/MethodList %y")]
		public static void SearchFromSelection()
		{
			ShowWindow(new MethodList(), string.Empty);
		}

		public static void SearchFromSelection(string initial_word)
		{
			ShowWindow(new MethodList(), initial_word);
		}
	}

	public class MethodList : UnifredFeatureBase<MethodListObject>
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
			bool is_selected = Selection.gameObjects.Count() > 0;
			return is_selected? "input method name":"<color=red> select gameobject</color>";
		}

		public override bool IsMultipleSelect()
		{
			return true;
		}

		public override IEnumerable<MethodListObject> UpdateCandidate(string word)
		{
			List<MethodListObject> result = new List<MethodListObject>();

            if (string.IsNullOrEmpty(word)) {
                return result;
			}

			var selected_objects = UnityEditor.Selection.gameObjects;

			var words = word.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			if (words.Length <= 0) {
			}

			foreach (var selected in selected_objects) {
				var components = selected.GetComponents<Component>();

				foreach (var component in components) {
					var methods = component.GetType().GetMethods(
						BindingFlags.NonPublic
						| BindingFlags.Public
						| BindingFlags.Instance);

					foreach (var method in methods) {
						var item = new MethodListObject();
						item.component = component;
						item.target = selected;
						item.method = method;
						item.name = selected.name + "::" + component.GetType().Name + "::" + method.Name;

						bool is_contain = words.All(
							(word_unit) => {return item.name.IndexOf(word_unit, StringComparison.OrdinalIgnoreCase) >= 0;}
						);
						if (!is_contain) {
							continue;
						}

						result.Add(item);
					}
				}
			}

			return result;
		}	

		public override void Draw(
			string word,
			MethodListObject candidate,
			bool is_selected
		) {
            GUILayout.Label(candidate.name, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<MethodListObject> result_list)
		{
			EditorApplication.delayCall += () => {
				MethodCallWindow.ShowMethod(result_list);
			};
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}
	}

	public class MethodListObject
	{
		public string name;
		public GameObject target;
		public Component component;
		public MethodInfo method;
	};
}

