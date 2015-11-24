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
	public class ValueListWindow : UnifredWindowController<ValueListObject>
	{
		public static void ShowWindow()
		{
			ShowWindow(new ValueList(), string.Empty);
		}
	}

	public class ValueList : UnifredFeatureBase<ValueListObject>
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
			return CandidateSelectMode.Single;
		}

		public override IEnumerable<ValueListObject> UpdateCandidate(string word)
		{
			List<ValueListObject> result = new List<ValueListObject>();
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
						var item = new ValueListObject();
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
						var item = new ValueListObject();
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
			ValueListObject candidate,
			bool is_selected
		) {
            GUILayout.Label(candidate.name, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<ValueListObject> result_list)
		{
			ValueListObject result = result_list.First();
			if (Input.IsPressedCommandKey()) {
				ChangeValue(result);
			}
			else {
				Show(result);
			}
		}

		private void Show(ValueListObject result)
		{
			object showedValue = null;
			if (result.field != null) {
				showedValue = result.field.GetValue(result.component);
			}
			else if (result.property != null) {
				showedValue = result.property.GetValue(result.component, null);
			}

			if (showedValue is String) {
				Debug.Log(showedValue);
			}
			else {
				string log = showedValue.ToStringFields();
				if (string.IsNullOrEmpty(log)) {
					Debug.Log(showedValue.ToStringProperties());
				}
				else {
					Debug.Log(log);
				}
			}
		}

		private void ChangeValue(ValueListObject result)
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


	public class ValueListObject
	{
		public string		name;
		public GameObject	target;
		public Component	component;
		public FieldInfo	field;
		public PropertyInfo	property;
	};
}
