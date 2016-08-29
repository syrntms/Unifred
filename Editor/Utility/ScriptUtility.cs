using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CSharp;

namespace Unifred
{
	public static class ScriptUtility
	{

		// original http://www.codeproject.com/Articles/11939/Evaluate-C-Code-Eval-Function
		public static object Eval(string sCSCode, IEnumerable<Type> assemblyTypes = null) {

			CSharpCodeProvider c = new CSharpCodeProvider();
			ICodeCompiler icc = c.CreateCompiler();
			CompilerParameters cp = new CompilerParameters();

			cp.ReferencedAssemblies.Add("system.dll");
			cp.ReferencedAssemblies.Add(typeof(MonoBehaviour).Assembly.Location);
			if (assemblyTypes != null) {
				assemblyTypes.ForEach(loadType => cp.ReferencedAssemblies.Add(loadType.Assembly.Location));
			}

			cp.CompilerOptions = "/t:library";
			cp.GenerateInMemory = true;

			StringBuilder sb = new StringBuilder("");
			sb.Append("using System;\n" );
			sb.Append("namespace CSCodeEvaler{ \n");
			sb.Append("public class CSCodeEvaler{ \n");
			sb.Append("public object EvalCode(){\n");
			sb.Append("return "+sCSCode+"; \n");
			sb.Append("} \n");
			sb.Append("} \n");
			sb.Append("}\n");

			CompilerResults cr = icc.CompileAssemblyFromSource(cp, sb.ToString());
			if( cr.Errors.Count > 0 ){
				return null;
			}

			System.Reflection.Assembly a = cr.CompiledAssembly;
			object o = a.CreateInstance("CSCodeEvaler.CSCodeEvaler");

			Type t = o.GetType();
			MethodInfo mi = t.GetMethod("EvalCode");

			object s = mi.Invoke(o, null);
			return s;
		}
	}
}
