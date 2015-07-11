using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred.Feature
{

	public class PasteComponentWindow : UnifredWindowController<PasteComponentObject>
	{
		[MenuItem("Unifred/PasteComponent %]")]
		public static void PasteComponent()
		{
			ShowWindow(new PasteComponent(), string.Empty);
		}
	}

	public class PasteComponent : UnifredFeatureBase<PasteComponentObject>
	{
		private static GUIStyle textGuiStyle = new GUIStyle
		{
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = EditorStyles.label.normal.textColor }
		};

		public override string GetDescription()
		{
			if (Selection.activeGameObject == null) {
				return "<color=red>select game you want to paste component</color>";
			}
			return "input component name you wanna to copy";
		}

		public override bool IsMultipleSelect()
		{
			return true;
		}

		public override IEnumerable<PasteComponentObject> UpdateCandidate(string input)
		{
			List<PasteComponentObject> result = new List<PasteComponentObject>();
			string data = EditorUserSettings.GetConfigValue("CopiedComponentByUnifred");
			if (string.IsNullOrEmpty(data)) {
				return result;
			}

			string[] words = input.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);

			var copyObjects = SerializeUtility.DeserializeObject<List<CopyComponentObject>>(data);
			return copyObjects
				.Where( obj => words.All( word => obj.componentType.ToString().IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0) )
				.Select(
					obj => new PasteComponentObject {
						componentType = obj.componentType,
						properties = obj.properties,
					}
				)
				.Reverse();
		}	

		public override void Draw(
			string word,
			PasteComponentObject candidate,
			bool is_selected
		) {
            GUILayout.Label(candidate.componentType.ToString(), textGuiStyle);
		}

		public override void Select(
			string word,
			IEnumerable<PasteComponentObject> result_list
		) {
			if (result_list.Count() <= 0) {
				return;
			}

			result_list.ForEach(
				pasteObject => {
					var component = Selection.activeGameObject.GetComponent(pasteObject.componentType);
					if (component == null) {
						component = Selection.activeGameObject.AddComponent(pasteObject.componentType);
					}
					_PasteSerialized(component, pasteObject.properties);
				}
			);
		}

		private void _PasteSerialized(
			Component component,
			Dictionary<string, System.Object> properties
		) {
			var toSerialized = new SerializedObject(component);
			var iterator = toSerialized.GetIterator();

			while (iterator.Next(true)) {

				switch (iterator.propertyType) {
				case SerializedPropertyType.AnimationCurve:
					iterator.animationCurveValue = (AnimationCurve)(SerializableAnimationCurve)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.ArraySize:
					iterator.intValue = (int)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.Boolean:
					iterator.boolValue = (bool)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.Bounds:
					iterator.boundsValue = (Bounds)(SerializableBounds)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.Color:
					iterator.colorValue = (Color)(SerializableColor)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.Enum:
					iterator.enumValueIndex = (int)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.Float:
					iterator.floatValue = (float)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.Integer:
					// error occured.
					// [CheckConsistency: GameObject does not reference component MonoBehaviour. Fixing.]
					// and often unity down.
					if (iterator.propertyPath.Contains("m_GameObject.m_FileID")) {
						break;
					}
					iterator.intValue = (int)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.LayerMask:
					iterator.intValue = (int)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.ObjectReference:
					// error occured.
					// [CheckConsistency: GameObject does not reference component MonoBehaviour. Fixing.]
					// and often unity down.
					if (iterator.propertyPath.Contains("m_GameObject")) {
						break;
					}
					var id = (int)properties[iterator.propertyPath];
					iterator.objectReferenceInstanceIDValue = id;
					break;
				case SerializedPropertyType.Quaternion:
					iterator.quaternionValue = (Quaternion)(SerializableQuaternion)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.Rect:
					iterator.rectValue = (Rect)(SerializableRect)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.String:
					iterator.stringValue = (string)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.Vector2:
					iterator.vector2Value = (Vector2)(SerializableVector2)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.Vector3:
					iterator.vector3Value = (Vector3)(SerializableVector3)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.Vector4:
					iterator.vector4Value = (Vector4)(SerializableVector4)properties[iterator.propertyPath];
					break;
				case SerializedPropertyType.Character:
				case SerializedPropertyType.Generic:
				case SerializedPropertyType.Gradient:
					break;
				}
			}
			toSerialized.ApplyModifiedProperties();
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}

	}

	public class PasteComponentObject
	{
		public Type componentType;
		public Dictionary<string, System.Object> properties;
	};
}
