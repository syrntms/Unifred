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
	public class MethodCallWindow : UnifredWindowController<MethodCallObject>
	{
		public static void ShowWindow(IEnumerable<MethodListObject> list)
		{
			ShowWindow(new MethodCall(list), string.Empty);
		}
	}

	public class MethodCall : UnifredFeatureBase<MethodCallObject>
	{

		private List<MethodCallObject> list = new List<MethodCallObject>();

		public MethodCall(IEnumerable<MethodListObject> list)
		{
			this.list.Clear();
			foreach (var item in list) {
				var call_object = new MethodCallObject();
				call_object.component 	= item.component;
				call_object.method 		= item.method;
				call_object.name 		= item.name;
				call_object.target 		= item.target;
				this.list.Add(call_object);
			}
		}

		private static GUIStyle textGuiStyle = new GUIStyle {
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = Color.white, },
		};

		public override string GetDescription()
		{
			return "input parameter of methods";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.Single;
		}

		public override IEnumerable<MethodCallObject> UpdateCandidate(string word)
		{
			return list;
		}	

		public override void Draw(
			string word,
			MethodCallObject candidate,
			bool is_selected
		) {

			int current_param_index = word.Count((c) => {return c == ',';});
			var paramaters = candidate.method.GetParameters();
			var param_desc = "(";
			int param_index = 0;
			foreach (var param in paramaters) {

				var type_name = param.ParameterType.ToString();
				var last_namespace_index = type_name.LastIndexOf('.');
				var type_name_without_namespace = type_name.Substring(last_namespace_index + 1);

				string open_tag  = (param_index == current_param_index)? "<color=white>":"";
				string close_tag = (param_index == current_param_index)? "</color>":"";
				param_desc += open_tag + type_name_without_namespace + close_tag;

				param_index++;

				bool is_remain = param_index < paramaters.Count();
				if (is_remain) {
					param_desc += ",";
				}
			}
			param_desc += ")";

            GUILayout.Label(candidate.name + param_desc, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<MethodCallObject> result_list)
		{
			string[] param_list = word.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
			foreach (var item in list) {
				object[] real_param_list = _MakeParams(param_list, item.method.GetParameters());
				var ret = item.method.Invoke(item.component, real_param_list);
				Debug.Log("method return" + ret);
			}
		}

		private static Object[] _MakeParams(string[] string_params, ParameterInfo[] param_infos)
		{
			bool has_params = false;
			bool is_va_list = false;
			object[] param_list = null;
			if (param_infos.Length > 0) {
				has_params = true;
				is_va_list = param_infos[param_infos.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
				if (is_va_list) {
					Debug.LogWarning("this method has params of va_list");
				}
			}

			var param_length = param_infos.Length;
			Type[] real_types = new Type[param_length];
			for (int i = 0 ; i < param_length ; ++i) {
				var param = param_infos.ElementAt(i);
				real_types[i] = param.ParameterType.GetElementType() ?? param.ParameterType;
			}
		
			if (has_params) {
				param_list  = new object[param_length];
				for (int i = 0; i < param_length ; i++) {
					param_list[i] = ScriptUtility.MakeValue(string_params[i], real_types[i]);
				}
			}

			return param_list;
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}
	}

	public class MethodCallObject
	{
		public string name;
		public GameObject target;
		public Component component;
		public MethodInfo method;
	};
}
