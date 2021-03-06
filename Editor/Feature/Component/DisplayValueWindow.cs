using System;
using System.Linq;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Unifred.Feature
{
	public class DisplayValueWindow : UnifredWindowController<DisplayValueObject>
	{
		public static void ShowWindow()
		{
			ShowWindow(new DisplayValue(), string.Empty);
		}
	}

	public class DisplayValue : UnifredFeatureBase<DisplayValueObject>
	{

		private static GUIStyle textGuiStyle = new GUIStyle {
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = Color.white, },
		};

		public override string GetDescription()
		{
			bool is_selected = Selection.gameObjects.Count() > 0;
			return is_selected? "input value name":"<color=white>select gameobject</color>";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.Multiple;
		}

		public override IEnumerable<DisplayValueObject> UpdateCandidate(string word)
		{
			List<DisplayValueObject> result = new List<DisplayValueObject>();
            if (string.IsNullOrEmpty(word)) {
                return result;
			}

			var selected_objects = UnityEditor.Selection.gameObjects;
			if (selected_objects.Count() <= 0) {
				return result;
			}

			var words = word.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			if (words.Length <= 0) {
				return result;
			}

			foreach (var selected in selected_objects) {

				var components = selected.GetComponents<Component>();
				foreach (var component in components) {

					// properties
					var properties = component.GetType().GetProperties(
						BindingFlags.Public
						| BindingFlags.NonPublic
						| BindingFlags.Instance
						| BindingFlags.Static
					);

					foreach (var property in properties) {
						var item = new DisplayValueObject();
						item.name		= property.Name;
						item.component	= component;
						item.property	= property;
						item.target		= selected;
						item.field		= null;

						bool is_contain = words.All(
							(word_unit) => {return item.name.IndexOf(word_unit, StringComparison.OrdinalIgnoreCase) >= 0;}
						);
						if (!is_contain) {
							continue;
						}
						result.Add(item);
					}

					// fields
					var fields = component.GetType().GetFields(
						BindingFlags.Public
						| BindingFlags.NonPublic
						| BindingFlags.Instance
						| BindingFlags.Static
					);

					foreach (var field in fields) {
						var item = new DisplayValueObject();
						item.name		= field.Name;
						item.component	= component;
						item.field		= field;
						item.target		= selected;
						item.property	= null;

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
			DisplayValueObject candidate,
			bool is_selected
		) {
            GUILayout.Label(candidate.name, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<DisplayValueObject> result_list)
		{
			foreach (var result in result_list) {
				Show(result);
			}
		}

		private void Show(DisplayValueObject result)
		{
			var labelComponent = result.target.GetComponent<SceneWindowLabel>();
			if (labelComponent == null) {
				labelComponent = result.target.AddComponent<SceneWindowLabel>();
			}

			if (result.field != null) {
				labelComponent.Add(result.component,  result.field);
			}
			else if (result.property != null) {
				labelComponent.Add(result.component,  result.property);
			}
		}

		private void ChangeValue(DisplayValueObject result)
		{
			EditorApplication.delayCall += () => {
				ChangeValueObject changeValueObject = new ChangeValueObject();
				changeValueObject.component	= result.component;
				changeValueObject.target	= result.target;
				changeValueObject.field		= result.field;
				changeValueObject.property	= result.property;
				changeValueObject.name		= result.name;
				ChangeValueWindow.ShowWindow(changeValueObject);
			};
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}
	}


	public class DisplayValueObject
	{
		public string		name;
		public GameObject	target;
		public Component	component;
		public FieldInfo	field;
		public PropertyInfo	property;
	};
}
