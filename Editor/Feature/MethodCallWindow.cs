using System;
using System.Linq;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Unifred
{
	public class MethodCallWindow : UnifredWindowController<MethodCallObject>
	{
		public static void ShowMethod(IEnumerable<MethodListObject> list)
		{
			MethodCall.SetList(list);
			ShowWindow(new MethodCall(), string.Empty);
		}
	}

	public class MethodCall : UnifredFeatureBase<MethodCallObject>
	{

		private static GUIStyle textGuiStyle = new GUIStyle {
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = Color.white, },
		};

		public static void SetList(IEnumerable<MethodListObject> list)
		{
			MethodCall.list.Clear();
			foreach (var item in list) {
				var call_object = new MethodCallObject();
				call_object.component 	= item.component;
				call_object.method 		= item.method;
				call_object.name 		= item.name;
				call_object.target 		= item.target;
				MethodCall.list.Add(call_object);
			}
		}

		public override string GetDescription()
		{
			return "input parameter of methods";
		}

		public override bool IsMultipleSelect()
		{
			return false;
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
					param_list[i] = _MakeValue(string_params[i], real_types[i]);
//					Debug.Log(
//						"@input : " 	+ string_params[i] +
//						"@type : " 		+ real_types[i].ToString() +
//						"@result : " 	+ param_list[i].ToString()
//						);
				}
			}

			return param_list;
		}

		private static List<MethodCallObject> list = new List<MethodCallObject>();

		private static Object _MakeValue(string value, Type type)
		{
			string source = @"
				using UnityEngine;
				public class ValueMaker
				{
				    public static " + type.ToString() + @" Eval()
				    {
						return " + value + @";
				    }
				}";
			var provider = new CSharpCodeProvider();
			var paramater = new CompilerParameters();
			paramater.GenerateInMemory = true;
			//add unity engine location
			paramater.ReferencedAssemblies.Add(typeof(MonoBehaviour).Assembly.Location);

			ICodeCompiler compiler = provider.CreateCompiler();

			CompilerResults result = compiler.CompileAssemblyFromSource(paramater, source);

			Assembly asm = result.CompiledAssembly;
			foreach (var err in result.Errors) {
				Debug.Log("eval paramater has some error message:" + err.ToString());
			}
			Type t = asm.GetType("ValueMaker");

			return t.InvokeMember("Eval", BindingFlags.InvokeMethod, null, null, null);
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
