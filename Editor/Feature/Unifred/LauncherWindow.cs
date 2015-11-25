using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Unifred.Feature
{
	public class LauncherWindow : UnifredWindowController<LauncherObject>
	{
		[MenuItem("Unifred/LaunchFeature %g")]
		public static void ShowWindow()
		{
			ShowWindow(new LauncherFeature(), string.Empty);
		}
	}

	public class LauncherFeature : UnifredFeatureBase<LauncherObject>
	{
		public IEnumerable<LauncherObject>	LaunchedFeatureObjects = new List<LauncherObject>(){
			new LauncherObject() {ClassType = typeof(AssetAndSearchWindow),		Hotkey = "ps",	MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(HierarchyAndSearchWindow),	Hotkey = "hs",	MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(MethodListWindow),			Hotkey = "cm",	MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(CopyComponentWindow),		Hotkey = "cc",	MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(DeleteComponentWindow),	Hotkey = "cd",	MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(PasteComponentWindow),		Hotkey = "cp",	MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(ValueListWindow),			Hotkey = "cv",	MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(SearchHistoryWindow),		Hotkey = "uh",	MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(Console),					Hotkey = "dc",	MethodName = "Clear"},
			new LauncherObject() {ClassType = typeof(ManipulateComponent),		Hotkey = "ci",	MethodName = "InitializeField"},
			new LauncherObject() {ClassType = typeof(ManipulateInspector),		Hotkey = "il",	MethodName = "SwitchLock"},
		};

		private static GUIStyle textGuiStyle = new GUIStyle {
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = Color.white, },
		};

		public override string GetDescription()
		{
			return "input hotkey";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.SingleImmediately;
		}

		public override IEnumerable<LauncherObject> UpdateCandidate(string word)
		{
			List<LauncherObject> result = new List<LauncherObject>();
			bool isCandidateAll = string.IsNullOrEmpty(word);

			LaunchedFeatureObjects
				.Where(x => isCandidateAll || x.Hotkey.StartsWith(word))
				.ForEach(x => result.Add(x));
			return result;
		}	

		public override void Draw(
			string word,
			LauncherObject candidate,
			bool is_selected
		) {
            GUILayout.Label(candidate.Hotkey + ":" + candidate.ClassType.Name, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<LauncherObject> result_list)
		{
			LauncherObject result = result_list.First();
			MethodInfo[] methods = result.ClassType.GetMethods();
			MethodInfo method = methods
				.Where(x => x.GetParameters().Count() == 0 && x.Name == result.MethodName)
				.FirstOrDefault();
			if (method == null) {
				return;
			}
			EditorApplication.delayCall += () => {
				method.Invoke(null, null);
			};
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}
	}

	public class LauncherObject
	{
		public Type		ClassType;
		public string	Hotkey;
		public string	MethodName;
	};
}
