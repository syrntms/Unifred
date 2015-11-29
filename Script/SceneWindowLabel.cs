using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Unifred
{
	public class SceneWindowLabel : MonoBehaviour
	{
		public void Add(Component c, FieldInfo fieldInfo)
		{
			list.Add(new SceneWindowLabelObject(){component = c, field = fieldInfo,});
		}

		public void Add(Component c, PropertyInfo propertyInfo)
		{
			list.Add(new SceneWindowLabelObject(){component = c, property = propertyInfo,});
		}

		public void Clear()
		{
			list.Clear();
		}

		public string GetLabel()
		{
			StringBuilder builder = new StringBuilder();
			foreach (var obj in list) {
				if (builder.Length != 0) {
					builder.AppendLine();
				}
				if (obj.field != null) {
					builder.AppendFormat("<color=#00AFFFFF>{0}:</color> <color=#CC6600FF>{1}</color>",  obj.field.Name, obj.field.GetValue(obj.component));
				}
				else if (obj.property != null) 
				{
					builder.AppendFormat("<color=#00AFFFFF>{0}:</color> <color=#CC6600FF>{1}</color>",  obj.property.Name, obj.property.GetValue(obj.component, null));
				}
			}
			return builder.ToString();
		}
		private List<SceneWindowLabelObject> list = new List<SceneWindowLabelObject>();
	}

	public class SceneWindowLabelObject
	{
		public Component component;
		public FieldInfo field;
		public PropertyInfo property;
	}
}