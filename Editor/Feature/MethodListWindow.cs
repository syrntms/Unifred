using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred
{
	public class MethodListWindow : UnifredWindow<MethodListObject> {

		[MenuItem("Unifred/MethodList %y")]
		public static void SearchFromSelection() {
			ShowWindow(new MethodList());
		}
	}

	public class MethodList : IUnifred<MethodListObject> {

		private static GUIStyle textGuiStyle = new GUIStyle {
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

		public override void OnInit() {
			defaultRowGuiStyle = new GUIStyle {
				normal = {background = Util.MakeSolidTexture(Color.clear),}
			};
			selectedRowGuiStyle = new GUIStyle {
				normal = { background = Util.MakeSolidTexture(Color.magenta + Color.gray * 1.25f),},
			};
		}

		public override void OnDestroy() {
			_DestroyStyle();
		}

		public override string GetDescription() {
			bool is_selected = Selection.gameObjects.Count() > 0;
			return is_selected? "input method name":"<color=red> select gameobject</color>";
		}

		public override bool IsMultipleSelect() {
			return true;
		}

		public override IEnumerable<MethodListObject> UpdateCandidate(string word) {
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
			IEnumerable<MethodListObject> result_list,
			IEnumerable<IntRange> selected_list,
			int offset,
			int count
		) {

			IEnumerable<int> uniq_selected_list = IntRange.Split(selected_list);

			for (int i = offset ; i < offset + count ; ++i) {
				bool is_selected = uniq_selected_list.Any((index) => {return index == i;});
				GUIStyle style = (is_selected)? selectedRowGuiStyle:defaultRowGuiStyle;
				MethodListObject result = result_list.ElementAt(i);
	            GUILayout.BeginHorizontal(style);
	            GUILayout.Label(result.name, textGuiStyle);
	            GUILayout.EndHorizontal();
			}
		}

		public override void Select(string word, IEnumerable<MethodListObject> result_list) {
			EditorApplication.delayCall += () => {
				MethodCallWindow.ShowMethod(result_list);
			};
		}

		public override float GetRowHeight() {
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}

		private void _DestroyStyle() {
			GameObject.DestroyImmediate(defaultRowGuiStyle.normal.background);
			GameObject.DestroyImmediate(selectedRowGuiStyle.normal.background);
		}
	}

	public class MethodListObject {
		public string name;
		public GameObject target;
		public Component component;
		public MethodInfo method;
	};
}

