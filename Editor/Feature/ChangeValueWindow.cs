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
	public class ChangeValueWindow : UnifredWindowController<ChangeValueObject>
	{
		public static void ChangeValue(ChangeValueObject data)
		{
			ChangeValue changer = new ChangeValue();
			changer.SetCandidate(data);
			ShowWindow(changer, string.Empty);
		}
	}

	public class ChangeValue : UnifredFeatureBase<ChangeValueObject>
	{
		private ChangeValueObject candidate;

		private static GUIStyle textGuiStyle = new GUIStyle {
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = Color.white, },
		};

		public override string GetDescription()
		{
			return "input value";
		}

		public override bool IsMultipleSelect()
		{
			return false;
		}

		public override IEnumerable<ChangeValueObject> UpdateCandidate(string word)
		{
			List<ChangeValueObject> result = new List<ChangeValueObject>();
			result.Add(candidate);
			return result;
		}	

		public override void Draw(
			string word,
			ChangeValueObject candidateArg,
			bool is_selected
		) {
			string displayText = string.Empty;
			if (candidateArg.field != null) {
				displayText = string.Format("format:{0}, name:{1}, value:{2}", 
					candidateArg.field.FieldType,
                    candidateArg.name,
                    candidateArg.field.GetValue(candidateArg.component).ToStringReflection()
				);
			}
			else if (candidateArg.property != null) {
				displayText = string.Format("format:{0}, name:{1}, value:{2}",
					candidateArg.property.PropertyType,
				    candidateArg.name,
				   	candidateArg.property.GetValue(candidateArg.component, null).ToStringReflection()
				);
			}
            GUILayout.Label(displayText, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<ChangeValueObject> result_list)
		{
			if (candidate.field != null) {
				candidate.field.SetValue(
					candidate.component,
					ScriptUtility.MakeValue(word, candidate.field.FieldType)
				);
			}
			else if (candidate.property != null) {
				candidate.property.SetValue(
					candidate.component,
					ScriptUtility.MakeValue(word, candidate.property.PropertyType),
					null
				);
			}
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}

		public void SetCandidate(ChangeValueObject candidate)
		{
			this.candidate = candidate;
		}
	}


	public class ChangeValueObject
	{
		public string		name;
		public GameObject	target;
		public Component	component;
		public FieldInfo	field;
		public PropertyInfo	property;
	};
}
