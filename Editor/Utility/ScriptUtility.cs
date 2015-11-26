using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CSharp;

namespace Unifred
{
	public static class ScriptUtility
	{

		public static System.Object MakeValue(string value, Type type, IEnumerable<Type> assemblyTypes = null)
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
			CodeDomProvider provider = new CSharpCodeProvider();
			var paramater = new CompilerParameters();
			paramater.GenerateInMemory = true;
			//add unity engine location
			paramater.ReferencedAssemblies.Add(typeof(MonoBehaviour).Assembly.Location);

			assemblyTypes = assemblyTypes ?? Enumerable.Empty<Type>();
			assemblyTypes.ForEach(
				assemblyType => paramater.ReferencedAssemblies.Add(assemblyType.Assembly.Location)
			);

			CompilerResults result = provider.CompileAssemblyFromSource(paramater, source);

			Assembly asm = result.CompiledAssembly;
			foreach (var err in result.Errors) {
				Debug.Log("eval paramater has some error message:" + err.ToString());
			}
			Type t = asm.GetType("ValueMaker");

			return t.InvokeMember("Eval", BindingFlags.InvokeMethod, null, null, null);
		}
	}
}
