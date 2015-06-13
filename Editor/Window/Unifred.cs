using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred
{

	public class IntRange {
		public int from = 0;
		public int to = 0;

		public static IEnumerable<int> Split(IEnumerable<IntRange> range_list) {
			List<int> result = new List<int>();
			foreach (var range in range_list) {
				int min = Mathf.Min(range.from, range.to);
				int max = Mathf.Max(range.from, range.to);
				for (int i = min ; i <= max ; ++i) {
					result.Add(i);
				}
			}
			return result.Distinct();
		}

		public static IEnumerable<IntRange> Build(IEnumerable<int> source) {
			List<int> sorted = source.OrderBy((item) => {return item;}).ToList();
			List<IntRange> result = new List<IntRange>();

			IntRange latest = null;
			foreach (var item in sorted) {

				if (latest != null) {
					if (latest.to + 1 == item) {
						latest.to = item;
						continue;
					}
				}

				latest = new IntRange();
				latest.from = latest.to = item;
				result.Add(latest);
			}
			return result;
		}
	}

	public class UnifredWindowBase : EditorWindow {
		public void OnGUI() {if(OnGUIAction != null)OnGUIAction();}
		public void OnDestroy() {if(OnDestroyAction != null)OnDestroyAction();}
		public Action OnGUIAction;
		public Action OnDestroyAction;
	}

	public abstract class IUnifred<T> {
		#region edit here of impl class, if you want new feature
		public virtual string GetDescription(){return "";}
		public virtual string GetGuiSkinPrefabPath(){return "";}
		public virtual bool IsMultipleSelect(){return false;}
		public virtual Vector2 GetWindowSize(){return new Vector2(600, 400);}
		public virtual void OnDestroy(){}
		public virtual void OnInit(){}

		public abstract IEnumerable<T> UpdateCandidate(string word);
		public abstract void Draw(string word, IEnumerable<T> obj_list, IEnumerable<IntRange> selected_list, int offset, int count);
		public abstract void Select(string search_word, IEnumerable<T> obj_list);
		public abstract float GetRowHeight();
		#endregion
	}
	
	public class UnifredWindow<T> {

		private static IUnifred<T> instance;

		private static bool isInitializeWindowPosition;
		private static string searchWord = null;
		private static string prevSearchWord = null;
		private static GUIStyle searchUIStyle = null;
		private static GUIStyle descUIStyle = null;
		private static List<T>  resultList = new List<T>();
		private static List<IntRange> selectedList = new List<IntRange>();
		private static Vector2 scrollPosition = Vector2.zero;
		private static bool isScrollToSelected = false;
		private static bool isExecuteFromMouse = false;

		private static GUIStyle scrollbarHorizontal;
		private static GUIStyle scrollbarVertical;

		protected static void ShowWindow(IUnifred<T> instance) {
			UnifredWindowBase window = EditorWindow.GetWindow<UnifredWindowBase>();
			window.Close();

			UnifredWindow<T>.instance = instance;
			window = EditorWindow.CreateInstance<UnifredWindowBase>();
			_Initialize();
			window.OnGUIAction     = () => {OnGUI();};
			window.OnDestroyAction = () => {OnDestroy();};
			window.ShowAsDropDown(
				new Rect(0, 0, 0, 0),
				Vector2.one
			);
		}

		public static void OnDestroy() {
			instance.OnDestroy();
		}

		public static void OnGUI() {
			var window = EditorWindow.GetWindow<UnifredWindowBase>();

			if (searchUIStyle == null) {
				_InitUnifredWindowStyle();
			}

			if (!isInitializeWindowPosition) {
				window.wantsMouseMove = true;
				_ResizeWindow();
				isInitializeWindowPosition = true;
			}

			if (Input.IsPressedStopKey()) {
				window.Close();
				return;
			}

			if (Input.IsPressedDoneKey() || isExecuteFromMouse) {
				_OnPressEnter();
				window.Close();
				return;
			}

			_UpdateSelectedByKeyboard();

			if (Event.current.isMouse) {
				_UpdateSelectedByMouse();
			}

			_LimitSelect();

			if (prevSearchWord != searchWord) {
				selectedList.Clear();
			}

			prevSearchWord = searchWord;

			_DisplaySearchTextField();

			_UpdateCandidate();

			_DisplayCandidate();
		}

		private static void _LimitSelect() {
			IntRange current_selected = (selectedList.Count == 0)? null:selectedList.Last();
			if (!instance.IsMultipleSelect()) {
				if (selectedList.Count > 1) {
					selectedList.Clear();
					selectedList.Add(current_selected);
				}
				if (current_selected != null) {
					current_selected.from = current_selected.to;
				}
			}
			if (current_selected != null) {
				current_selected.from = Mathf.Clamp(current_selected.from, 0, resultList.Count() - 1);
				current_selected.to   = Mathf.Clamp(current_selected.to,   0, resultList.Count() - 1);
			}
		}

		private static float _GetRowStartHeight() {
			GUIStyle[] styles = {searchUIStyle, EditorStyles.label};

			float offset = 0f;
			foreach (var style in styles) {
				offset += style.CalcSize(new GUIContent(searchWord)).y
					+ style.margin.top + style.margin.bottom;
					//margin is excluded from calcsize
					//padding, fontsize is included 
					//border is ignored
			}
			return offset;
		}

		private static int _GetIndexOnMouseClicked() {
			float mouse_y = Event.current.mousePosition.y;
			float scroll_y = scrollPosition.y;
			float offset = _GetRowStartHeight();
			var index = Mathf.Floor((mouse_y + scroll_y - offset) / instance.GetRowHeight());
			return (int)index;
		}

		private static int _GetMaxRowCount() {
			return (int)((instance.GetWindowSize().y - _GetRowStartHeight()) / instance.GetRowHeight());
		}

		private static void _UpdateSelectedByMouse() {

			int index = _GetIndexOnMouseClicked();
			bool isRowClicked = index >= 0;

			if (!Input.IsMouseDown() || !isRowClicked) {
				return;
			}

			if (Input.IsPressedExpandKey() && instance.IsMultipleSelect()) {
				_UpdateSelectedByMouseDownWithExpand();
			}
			else {
				_UpdateSelectedByMouseDown();
			}
		}

		private static void _UpdateSelectedByMouseDownWithExpand() {
			IntRange current_selected = (selectedList.Count() == 0)? null:selectedList.Last();
			if (current_selected == null) {
				current_selected = new IntRange();
				current_selected.from = current_selected.to = 0;
				selectedList.Add(current_selected);
			}
			current_selected.to = _GetIndexOnMouseClicked();
		}

		private static void _UpdateSelectedByMouseDownWithToggle() {
			//TODO impl
		}

		private static void _UpdateSelectedByMouseDown() {
			int index = _GetIndexOnMouseClicked();
			var list = IntRange.Split(selectedList).ToList();
			bool is_contains = list.Contains(index);

			if (is_contains) {
				list.Remove(index);
				selectedList = IntRange.Build(list).ToList();
			}
			else {
				IntRange range = new IntRange();
				range.from = range.to = index;
				selectedList.Add(range);
			}
		}

		private static void _UpdateSelectedByKeyboardWithExpand() {
			IntRange current_selected = (selectedList.Count() == 0)? null:selectedList.Last();
			if (Input.IsPressedDownKey()) {
				if (current_selected == null) {
					selectedList.Add(current_selected = new IntRange());
				}
				current_selected.to++;
				isScrollToSelected = true;
			}
			if (Input.IsPressedUpKey()) {
				if (current_selected == null) {
					selectedList.Add(current_selected = new IntRange());
				}
				current_selected.to--;
				isScrollToSelected = true;
			}
		}

		private static void _UpdateSelectedByKeyboardWithoutExpand() {
			IntRange current_selected = (selectedList.Count() == 0)? null:selectedList.Last();

			if (Input.IsPressedDownKey()) {
				selectedList.Clear();
				bool is_first = current_selected == null;
				current_selected = current_selected ?? new IntRange();
				selectedList.Add(current_selected);
				isScrollToSelected = true;
				if (!is_first) {
					current_selected.from = ++current_selected.to;
				}
			}
			if (Input.IsPressedUpKey()) {
				selectedList.Clear();
				current_selected = current_selected ?? new IntRange();
				selectedList.Add(current_selected);
				isScrollToSelected = true;
				current_selected.from = --current_selected.to;
			}
		}

		private static void _UpdateSelectedByRoundGoKey() {

			int result_count = resultList.Count();
			if (result_count == 0) {
				return;
			}

			IntRange current_selected = (selectedList.Count() == 0)? null:selectedList.Last();
			selectedList.Clear();
			bool is_first = current_selected == null;
			current_selected = current_selected ?? new IntRange();
			selectedList.Add(current_selected);

			if (Input.IsPressedReverseKey()) {
				int next = --current_selected.to;
				next = (next < 0)? (next + result_count):next;
				current_selected.from = current_selected.to = next;
			}
			else {
				int next = (is_first)? 0:++current_selected.to;
				current_selected.from = current_selected.to = next % result_count;
			}
		}

		private static void _UpdateSelectedByCommandSelectAll() {
			IntRange current_selected = (selectedList.Count() == 0)? null:selectedList.Last();
			current_selected = (selectedList.Count() == 0)? null:selectedList.Last();
			if (current_selected == null) {
				selectedList.Add(current_selected = new IntRange());
				current_selected.from = 0;
				current_selected.to   = resultList.Count() - 1;
			}
			else {
				int min = Mathf.Min(current_selected.from, current_selected.to);
				int max = Mathf.Max(current_selected.from, current_selected.to);
				if (min == 0 && max == resultList.Count() - 1) {
					selectedList.Clear();
				}
				else {
					current_selected.from = 0;
					current_selected.to   = resultList.Count() - 1;
				}
			}
			Event.current.Use();
		}

		private static void _UpdateSelectedByKeyboard() {
			if (Input.IsPressedExpandKey()) {
				_UpdateSelectedByKeyboardWithExpand();
			}
			else {
				_UpdateSelectedByKeyboardWithoutExpand();
			}

			if (Input.IsPressedRoundGoKey()) {
				_UpdateSelectedByRoundGoKey();
				isScrollToSelected = true;
			}

			if (Event.current.type == EventType.ValidateCommand
			    && Event.current.commandName == "SelectAll"
		    ){
				_UpdateSelectedByCommandSelectAll();
			}
		}

        private static void _UpdateCandidate() {
            if (prevSearchWord != searchWord) {
                resultList = instance.UpdateCandidate(searchWord).ToList();
			}
        }

		private static void _DisplayCandidate() {

			if (isScrollToSelected) {
				scrollPosition = _CulcScrollPosition();
				isScrollToSelected = false;
			}

			scrollPosition = EditorGUILayout.BeginScrollView(
				scrollPosition,
			    scrollbarHorizontal,
				scrollbarVertical,
				null
			);

			float row_height = instance.GetRowHeight();
			float viewable_size = instance.GetWindowSize().y - _GetRowStartHeight();
			int offset = Mathf.Max(0, Mathf.FloorToInt(scrollPosition.y / row_height));
			int last   = Mathf.Min(resultList.Count(), (int)((scrollPosition.y + viewable_size + row_height) / row_height));
			int count  = last - offset;
			float rest = (resultList.Count() - count + 1) * row_height - scrollPosition.y;
	
			GUILayout.Space(scrollPosition.y);
			instance.Draw(searchWord, resultList, selectedList, offset, count);
			GUILayout.Space(rest);

//			Debug.Log(rest);

			EditorGUILayout.EndScrollView();
		}

		private static Vector2 _CulcScrollPosition() {
			Vector2 result = scrollPosition;

			float select_area_height = instance.GetWindowSize().y - _GetRowStartHeight();
			int scroll_start_index = Mathf.CeilToInt(scrollPosition.y / instance.GetRowHeight());
			int scroll_end_index   = scroll_start_index + Mathf.FloorToInt(select_area_height / instance.GetRowHeight()) - 1;
			//adjustment

			IntRange current_selected = (selectedList.Count() == 0)? null:selectedList.Last();
			int latest_index = current_selected == null? 0:current_selected.to;

			if (latest_index <= scroll_start_index) {
				int diff = scroll_start_index - latest_index + 1;
				result.y -= diff * instance.GetRowHeight();
				float min = 0f;
				result.y = Mathf.Max(result.y, min);
			}
			else if(scroll_end_index <= latest_index) {
				int diff = latest_index - scroll_end_index + 1;
				result.y += diff * instance.GetRowHeight();
				float max = (resultList.Count + 1) * instance.GetRowHeight() - select_area_height;
				result.y = Mathf.Min(result.y, max);
			}
			return result;
		}
		
		private static void _ResizeWindow() {
			Vector2 size = instance.GetWindowSize();
			UnifredWindowBase window = EditorWindow.GetWindow<UnifredWindowBase>();
			window.minSize = window.maxSize = size;
			window.position = Util.MakeMouseBasedPosition(size.x, size.y, window.position.position);
		}
		
		private static void _DisplaySearchTextField() {
			int selected_count = IntRange.Split(selectedList).Count();
			string candidate_count = "(" + selected_count + "/" + resultList.Count + ")  ";

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(candidate_count + instance.GetDescription(), descUIStyle);
			EditorGUILayout.Space();
			isExecuteFromMouse = GUILayout.Button("execute");
			GUILayout.EndHorizontal();

			const string controlName = "search_word";
			GUI.SetNextControlName(controlName);
            searchWord = GUILayout.TextField((searchWord ?? ""), searchUIStyle, GUILayout.ExpandWidth(true));
            GUI.FocusControl(controlName);
			_ForceCaretToEndOfTextField();
		}
		
		private static void _ForceCaretToEndOfTextField() {
			var editor = (TextEditor) GUIUtility.GetStateObject(typeof (TextEditor), GUIUtility.keyboardControl);            
			editor.pos = searchWord.Length;
			editor.selectPos = searchWord.Length;
		}
		
		private static void _OnPressEnter() {
			List<T> selected_list = new List<T>();
			foreach (IntRange select in selectedList) {
				int min = Mathf.Min(select.from, select.to);
				int max = Mathf.Max(select.from, select.to);
				for (int i = min ; i <= max ; ++i) {
					selected_list.Add(resultList.ElementAt(i));
				}
			}
			instance.Select(searchWord, selected_list);
		}

		private static void _InitUnifredWindowStyle() {
			string skin_prefab_path = instance.GetGuiSkinPrefabPath();
			if (!string.IsNullOrEmpty(skin_prefab_path)) {
				GUISkin ui_skin = Resources.LoadAssetAtPath(instance.GetGuiSkinPrefabPath(), typeof(GUISkin)) as GUISkin;
				searchUIStyle = ui_skin.box;
				scrollbarVertical = ui_skin.verticalScrollbar;
				scrollbarHorizontal = ui_skin.horizontalScrollbar;
				descUIStyle = EditorStyles.label;
			}
			else {
				searchUIStyle = new GUIStyle(){
					normal = {textColor = EditorGUIUtility.isProSkin? Color.white:Color.black,},
				};
				descUIStyle = EditorStyles.label;
				descUIStyle.richText = true;
				scrollbarVertical   = new GUIStyle(GUI.skin.verticalScrollbar);
				scrollbarHorizontal = GUIStyle.none;
			}
		}

		private static void _Initialize() {
			isInitializeWindowPosition = false;
			searchWord = null;
			prevSearchWord = null;
			searchUIStyle = null;
			resultList.Clear();
			selectedList.Clear();
			instance.OnInit();
		}
	}

	public class Util {
		public static Texture2D MakeSolidTexture(Color col) {
			int width  = 1;
			int height = 1;
			var pixels = new Color[width * height];
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = col;
			}
			var result = new Texture2D(width, height);
			result.SetPixels(pixels);
			result.wrapMode = TextureWrapMode.Repeat;
			result.Apply();
			return result;
		}

		public static Rect MakeMouseBasedPosition(float width, float height, Vector2 offset) {
			var pos = Event.current.mousePosition + offset;
			return new Rect(
				pos.x,
				pos.y,
                width,
                height);
		}
	}

	public class Input {
		public static bool IsMouseDown() {
			return Event.current.isMouse && Event.current.type == EventType.mouseDown;
		}

		public static bool IsMouse() {
			return Event.current.isMouse;
		}

		public static bool IsKeyDown(KeyCode keyCode) {
			return Event.current.type == EventType.keyDown && Event.current.keyCode == keyCode;
		}

		public static bool IsPressedDownKey () {
			return Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.DownArrow;
		}

		public static bool IsPressedOpenKey () {
			return _IsPlatformMac()? Event.current.command:Event.current.control;
		}

		public static bool IsPressedRoundGoKey () {
			return Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Tab;
		}

		public static bool IsPressedReverseKey() {
			return Event.current.shift;
		}

		public static bool IsPressedUpKey () {
			return Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.UpArrow;
		}

		public static bool IsPressedExpandKey() {
			return Event.current.shift;
		}

		public static bool IsPressedToggleKey() {
			return _IsPlatformMac()? Event.current.command:Event.current.control;
		}

		public static bool IsPressedStopKey() {
			return Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Escape;
		}

		public static bool IsPressedDoneKey() {
			return Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return;
		}

		private static bool _IsPlatformMac() {
			return Application.platform == RuntimePlatform.OSXEditor;
		}
	}

}
