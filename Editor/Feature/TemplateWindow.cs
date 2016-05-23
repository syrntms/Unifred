//using System;
//using System.Linq;
//using System.Reflection;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;
//using Object = System.Object;
//
//namespace Unifred.Feature
//{
//	public class TemplateWindow : UnifredWindowController<TemplateObject>
//	{
//		[MenuItem("Unifred/Template %t")]
//		public static void ShowWindow()
//		{
//			ShowWindow(new TemplateFeature(), string.Empty);
//		}
//	}
//
//	public class TemplateFeature : UnifredFeatureBase<TemplateObject>
//	{
//		private static GUIStyle textStyle = new GUIStyle {
//			richText = true,
//			fontSize = 12,
//			margin = new RectOffset(5, 5, 5, 5),
//			alignment = TextAnchor.MiddleLeft,
//			normal = { textColor = Color.white, },
//		};
//
//		public override string GetDescription()
//		{
//			/*******************************
//			// custom here
//			// input description message.
//			*******************************/
//			return "input description here";
//		}
//
//		public override CandidateSelectMode GetSelectMode()
//		{
//			/*******************************
//			// custom here
//			// choose candidate select mode.
//			*******************************/
//			return CandidateSelectMode.Single;
//		}
//
//		public override IEnumerable<TemplateObject> UpdateCandidate(string word)
//		{
//			List<TemplateObject> result = new List<TemplateObject>();
//			result.Add(new TemplateObject());
//
//			/*******************************
//			// custom here
//			// create candidate list by user input word
//			*******************************/
//
//			return result;
//		}	
//
//		public override void Draw(
//			string word,
//			TemplateObject candidate,
//			bool is_selected
//		) {
//			/*******************************
//			// custom here
//			// create label, image, and so on with supplied candidate
//			*******************************/
//
//            GUILayout.Label("create label by candidate");
//		}
//
//		public override void Select(string word, IEnumerable<TemplateObject> result_list)
//		{
//			/*******************************
//			// custom here
//			// do something by result_list user selected
//			*******************************/
//		}
//
//		public override float GetRowHeight()
//		{
//			return textStyle.CalcSize(new GUIContent("sample")).y
//				+ textStyle.margin.bottom + textStyle.margin.top;
//		}
//
//	}
//
//	public class TemplateObject
//	{
//		/*******************************
//		// custom here.
//		// data scheme of each method.
//		*******************************/
//	};
//}
