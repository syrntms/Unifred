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
			new LauncherObject() {ClassType = typeof(ProjectAndSearchWindow)   , Key = "p" , Explain = "Search file or open from project"       , Method = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(HierarchyAndSearchWindow) , Key = "h" , Explain = "Search object from hierarchy"           , Method = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(RenameTargetWindow)       , Key = "r" , Explain = "Search object that have renamed word"   , Method = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(InitializeComponentWindow), Key = "ci", Explain = "Initialize serialized value by name"    , Method = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(MethodListWindow)         , Key = "cm", Explain = "Call method of component"               , Method = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(CopyComponentWindow)      , Key = "cc", Explain = "Copy serialized value within component" , Method = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(DeleteComponentWindow)    , Key = "cd", Explain = "Delete component"                       , Method = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(PasteComponentWindow)     , Key = "cp", Explain = "Paste component from history you copied", Method = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(DisplayValueWindow)       , Key = "dv", Explain = "Display component value"                , Method = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(SearchHistoryWindow)      , Key = "u" , Explain = "Launch by history"          			, Method = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(ManipulateConsole)        , Key = "dc", Explain = "Clear console log"                      , Method = "Clear"}     ,
			new LauncherObject() {ClassType = typeof(ManipulateInspector)      , Key = "i",  Explain = "Lock inspector"                         , Method = "SwitchLock"},
			new LauncherObject() {ClassType = typeof(IconSetupWindow)          , Key = "g",  Explain = "Add icon to gameobject in sceneview"    , Method = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(HierarchyDrawerActivateWindow), Key = "ahd",  Explain = "Activation HierarchyDrawer"    , Method = "ShowWindow"},
			new LauncherObject() {ClassType = typeof(ProjectDrawerActivateWindow), Key = "apd",  Explain = "Activation ProjectDrawer"    , Method = "ShowWindow"},
		};

		private static GUIStyle keyStyle = new GUIStyle {
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			fixedWidth = 50,
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = Color.white, },
		};

		private static GUIStyle explainStyle = new GUIStyle {
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
				.Where(x => isCandidateAll || x.Key.StartsWith(word))
				.ForEach(x => result.Add(x));
			return result;
		}	

		public override void Draw(
			string word,
			LauncherObject candidate,
			bool is_selected
		) {
            GUILayout.Label("key: " + candidate.Key,	keyStyle);
			GUILayout.Label(candidate.Explain,			explainStyle);
		}

		public override void Select(string word, IEnumerable<LauncherObject> result_list)
		{
			LauncherObject result = result_list.First();
			MethodInfo[] methods = result.ClassType.GetMethods();
			MethodInfo method = methods
				.Where(x => x.GetParameters().Count() == 0 && x.Name == result.Method)
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
			return keyStyle.CalcSize(new GUIContent("sample")).y
				+ keyStyle.margin.bottom + keyStyle.margin.top;
		}

	}

	public class LauncherObject
	{
		public Type		ClassType;
		public string	Key;
		public string	Explain;
		public string	Method;
	};
}
