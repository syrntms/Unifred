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
			new LauncherObject() {ClassType = typeof(ProjectAndSearchWindow),		Hotkey = "p : ProjectSearch",		MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(HierarchyAndSearchWindow),	Hotkey = "h : HierarchySearch",	MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(ManipulateComponent),		Hotkey = "ci : ComponentInitialize",MethodName = "InitializeField"},
			new LauncherObject() {ClassType = typeof(MethodListWindow),			Hotkey = "cm : ComponentMethod",	MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(CopyComponentWindow),		Hotkey = "cc : ComponentCopy",		MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(DeleteComponentWindow),	Hotkey = "cd : ComponentDelete",	MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(PasteComponentWindow),		Hotkey = "cp : ComponentPaste",		MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(ValueListWindow),			Hotkey = "cv : ComponentValue",		MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(SearchHistoryWindow),		Hotkey = "u : UnifredHistory",		MethodName = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(ManipulateConsole),					Hotkey = "d : DebugClear",			MethodName = "Clear"},
			new LauncherObject() {ClassType = typeof(ManipulateInspector),		Hotkey = "i : InspectorLock",		MethodName = "SwitchLock"},
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
            GUILayout.Label(candidate.Hotkey, textGuiStyle);
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
