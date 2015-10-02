using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

// code from http://baba-s.hatenablog.com/entry/2014/02/27/000000
namespace Unifred
{
	/// <summary>
	/// object型の拡張メソッドを管理するクラス
	/// </summary>
	public static class ObjectExtensions
	{
		private const string SEPARATOR = ",";       // 区切り記号として使用する文字列
		private const string FORMAT = "{0}:{1}";    // 複合書式指定文字列
		
		/// <summary>
		/// すべての公開フィールドの情報を文字列にして返します
		/// </summary>
		public static string ToStringFields<T>(this T obj)
		{
			try {
				return string.Join(SEPARATOR, obj
					.GetType()
					.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic /*| BindingFlags.Static*/)
					.Select(c => string.Format(FORMAT, c.Name, c.GetValue(obj)))
					.ToArray()
				);
			}
			catch (Exception e) {
				return string.Empty;
			}
		}
		
		/// <summary>
		/// すべての公開プロパティの情報を文字列にして返します
		/// </summary>
		public static string ToStringProperties<T>(this T obj)
		{
			try {
				return string.Join(SEPARATOR, obj
					.GetType()
					.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic /*| BindingFlags.Static*/)
					.Where(c => c.CanRead)
					.Select(c => string.Format(FORMAT, c.Name, c.GetValue(obj, null)))
				    .ToArray()
				);
			}
			catch (Exception e) {
				return string.Empty;
			}
		}
		
		/// <summary>
		/// すべての公開フィールドと公開プロパティの情報を文字列にして返します
		/// </summary>
		public static string ToStringReflection<T>(this T obj)
		{
			return string.Join(SEPARATOR, 
				new string[] {obj.ToStringFields(), obj.ToStringProperties()}
			);
		}
	}
}