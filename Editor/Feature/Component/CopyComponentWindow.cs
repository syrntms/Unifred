using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred.Feature
{

	public class CopyComponentWindow : UnifredWindowController<CopyComponentObject>
	{
		public static void ShowWindow()
		{
			ShowWindow(new CopyComponent(), string.Empty);
		}

		[MenuItem("Unifred/ClearCopiedComponent")]
		public static void ClearCopiedComponent()
		{
			EditorUserSettings.SetConfigValue("CopiedComponentByUnifred", null);
		}
	}

	public class CopyComponent : UnifredFeatureBase<CopyComponentObject>
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
				return "<color=white>select game object you wanna copy</color>";
			}
			return "input component name you wanna to copy";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.Multiple;
		}

		private Dictionary<string, System.Object> _SerializedObjectToSerializable(
			SerializedObject serializedObject
		) {
			Dictionary<string, System.Object> result = new Dictionary<string, System.Object>();
			var iterator = serializedObject.GetIterator();

			while (iterator.Next(true)) {

				switch (iterator.propertyType) {
				case SerializedPropertyType.AnimationCurve:
					result.Add(iterator.propertyPath, (SerializableAnimationCurve)iterator.animationCurveValue);
					break;
				case SerializedPropertyType.ArraySize:
					result.Add(iterator.propertyPath, iterator.intValue);
					break;
				case SerializedPropertyType.Boolean:
					result.Add(iterator.propertyPath, iterator.boolValue);
					break;
				case SerializedPropertyType.Bounds:
					result.Add(iterator.propertyPath, (SerializableBounds)iterator.boundsValue);
					break;
				case SerializedPropertyType.Color:
					result.Add(iterator.propertyPath, (SerializableColor)iterator.colorValue);
					break;
				case SerializedPropertyType.Enum:
					result.Add(iterator.propertyPath, iterator.enumValueIndex);
					break;
				case SerializedPropertyType.Float:
					result.Add(iterator.propertyPath, iterator.floatValue);
					break;
				case SerializedPropertyType.Integer:
					result.Add(iterator.propertyPath, iterator.intValue);
					break;
				case SerializedPropertyType.LayerMask:
					result.Add(iterator.propertyPath, iterator.intValue);
					break;
				case SerializedPropertyType.ObjectReference:
					result.Add(iterator.propertyPath, iterator.objectReferenceInstanceIDValue);
					break;
				case SerializedPropertyType.Quaternion:
					result.Add(iterator.propertyPath, (SerializableQuaternion)iterator.quaternionValue);
					break;
				case SerializedPropertyType.Rect:
					result.Add(iterator.propertyPath, (SerializableRect)iterator.rectValue);
					break;
				case SerializedPropertyType.String:
					result.Add(iterator.propertyPath, iterator.stringValue);
					break;
				case SerializedPropertyType.Vector2:
					result.Add(iterator.propertyPath, (SerializableVector2)iterator.vector2Value);
					break;
				case SerializedPropertyType.Vector3:
					result.Add(iterator.propertyPath, (SerializableVector3)iterator.vector3Value);
					break;
				case SerializedPropertyType.Vector4:
					result.Add(iterator.propertyPath, (SerializableVector4)iterator.vector4Value);
					break;
				case SerializedPropertyType.Character:
				case SerializedPropertyType.Generic:
				case SerializedPropertyType.Gradient:
					//cant access with value
					break;
				}
			}
			return result;
		}

		public override IEnumerable<CopyComponentObject> UpdateCandidate(string input)
		{
			List<CopyComponentObject> result = new List<CopyComponentObject>();
			GameObject go = Selection.activeGameObject;
			if (go == null) {
				return result;
			}
			go.GetComponents<Component>().ForEach(
				component => {
					CopyComponentObject obj = new CopyComponentObject();
					obj.componentType = component.GetType();
					obj.properties = _SerializedObjectToSerializable(new SerializedObject(component));
					result.Add(obj);
				}
			);

			string[] words = input.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);

			words.ForEach(word => Debug.Log(word));
			return result.Where(
				obj => words.All( word => obj.componentType.ToString().IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0 )
			);
		}	

		public override void Draw(
			string word,
			CopyComponentObject candidate,
			bool is_selected
		) {
            GUILayout.Label(candidate.componentType.ToString(), textGuiStyle);
		}

		public override void Select(string word, IEnumerable<CopyComponentObject> result_list)
		{
			var history_data = EditorUserSettings.GetConfigValue("CopiedComponentByUnifred");

			IEnumerable<CopyComponentObject> histories;
			if (string.IsNullOrEmpty(history_data)) {
				histories = new List<CopyComponentObject>();
			}
			else {
				histories = SerializeUtility.DeserializeObject<List<CopyComponentObject>>(history_data);
			}

			histories = histories.Concat(result_list);

			if (histories.Count() > Consts.COPYCOMPONENT_HISTORY_COUNT) {
				histories = histories.Skip(histories.Count() - Consts.COPYCOMPONENT_HISTORY_COUNT);
			}

			var updated_history_data = SerializeUtility.SerializeObject<List<CopyComponentObject>>(histories.ToList());
			EditorUserSettings.SetConfigValue("CopiedComponentByUnifred", updated_history_data);
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}

//		public void DebugOutput(SerializedProperty iterator) {
//			Debug.Log(iterator.animationCurveValue);
//			Debug.Log(iterator.boolValue);
//			Debug.Log(iterator.boundsValue);
//			Debug.Log(iterator.colorValue);
//			Debug.Log(iterator.doubleValue);
//			Debug.Log(iterator.enumValueIndex);
//			Debug.Log(iterator.floatValue);
//			Debug.Log(iterator.hasMultipleDifferentValues);
//			Debug.Log(iterator.intValue);
//			Debug.Log(iterator.longValue);
//			Debug.Log(iterator.objectReferenceInstanceIDValue);
//			Debug.Log(iterator.objectReferenceValue);
//			Debug.Log(iterator.quaternionValue);
//			Debug.Log(iterator.rectValue);
//			Debug.Log(iterator.stringValue);
//			Debug.Log(iterator.vector2Value);
//			Debug.Log(iterator.vector3Value);
//			Debug.Log(iterator.vector4Value);
//		}
	}

	[System.Serializable]
	public class CopyComponentObject
	{
		public Type componentType;
		public Dictionary<string, System.Object> properties;
	};
}
