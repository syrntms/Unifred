using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred
{
	public class UnifredWindowController<T>
	{
		private UnifredFeatureBase<T> feature;
		private bool isInitializeWindowPosition;
		private string prevSearchWord = null;
		private string searchWord = null;
		private GUIStyle searchUIStyle = null;
		private GUIStyle descUIStyle = null;
		private IEnumerable<T> candidateList = new List<T>();
		private List<IntRange> selectedList = new List<IntRange>();
		private Vector2 scrollPosition = Vector2.zero;
		private bool isScrollToSelected = false;
		private bool isExecuteFromMouse = false;

		private GUIStyle scrollbarHorizontal;
		private GUIStyle scrollbarVertical;

		protected static void ShowWindow(UnifredFeatureBase<T> instance, string input)
		{
			UnifredWindowController<T> controller = new UnifredWindowController<T>();
			controller._Initialize(instance, input);

			UnifredWindow window = EditorWindow.GetWindow<UnifredWindow>();
			window.Close();
			window = EditorWindow.CreateInstance<UnifredWindow>();
			window.OnGUIAction     = () => {controller.OnGUI();};
			window.OnDestroyAction = () => {controller.OnDestroy();};
			window.ShowAsDropDown(
				new Rect(0, 0, 0, 0),
				Vector2.one
			);
		}

		public void OnDestroy()
		{
			feature.OnDestroy();
		}

		public void OnGUI()
		{
			var window = EditorWindow.GetWindow<UnifredWindow>();

			if (searchUIStyle == null) {
				_InitUnifredWindowStyle();
			}

			if (!isInitializeWindowPosition) {
				window.wantsMouseMove = true;
				_ResizeWindow();
				isInitializeWindowPosition = true;
			}

			if (Input.IsPressedCancelKey()) {
				window.Close();
				return;
			}

			if (Input.IsPressedDoneKey() || isExecuteFromMouse) {
				_OnPressedDoneKey();
				window.Close();
				return;
			}

			_UpdateSelectedByKeyboard();

			if (Event.current.isMouse) {
				_UpdateSelectedByMouse();
			}

			_ClampSelected();

			if (prevSearchWord != searchWord) {
				selectedList.Clear();
			}

			_DisplaySearchTextField();

			_DisplayCandidate();

			_UpdateCandidate();

			prevSearchWord = searchWord;
		}

		/// <summary>
		/// clamp the selected element.
		/// make selected element single, if feature DO NOT allow multiple select.
		/// And, make 0 <= selected[i] < CandidateList.Length.
		/// </summary>
		private void _ClampSelected()
		{
			IntRange current_selected = (selectedList.Count == 0)? null : selectedList.Last();
			if (!feature.IsMultipleSelect()) {
				if (selectedList.Count > 1) {
					selectedList.Clear();
					selectedList.Add(current_selected);
				}
				if (current_selected != null) {
					current_selected.from = current_selected.to;
				}
			}
			if (current_selected != null) {
				current_selected.from = Mathf.Clamp(current_selected.from, 0, candidateList.Count() - 1);
				current_selected.to   = Mathf.Clamp(current_selected.to,   0, candidateList.Count() - 1);
			}
		}

		/// <summary>
		/// get row height of SelectedListUI of editor window from top.
		/// </summary>
		/// <returns>The get row start height.</returns>
		private float _GetRowStartHeight()
		{
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

		/// <summary>
		/// get index of CandidateList by mouse clicked.
		/// </summary>
		/// <returns>The get index on mouse clicked.</returns>
		private int _GetCandidateIndexOnMouse()
		{
			float mouse_y = Event.current.mousePosition.y;
			float scroll_y = scrollPosition.y;
			float offset = _GetRowStartHeight();
			var index = Mathf.Floor((mouse_y + scroll_y - offset) / feature.GetRowHeight());
			return (int)index;
		}

		/// <summary>
		/// update selected list by mouse input
		/// </summary>
		private void _UpdateSelectedByMouse()
		{
			int index = _GetCandidateIndexOnMouse();
			bool isRowClicked = index >= 0;

			if (!Input.IsMouseDown() || !isRowClicked) {
				return;
			}

			if (Input.IsPressedExpandKey() && feature.IsMultipleSelect()) {
				_UpdateSelectedByMouseWithExpand();
			}
			else {
				_UpdateSelectedByMouseWithoutOption();
			}
		}

		/// <summary>
		/// get max row count in editor window
		/// </summary>
		private int _GetMaxRowCountInWindow()
		{
			return (int)((feature.GetWindowSize().y - _GetRowStartHeight()) / feature.GetRowHeight());
		}

		/// <summary>
		/// update selected list by mouse with expand key
		/// </summary>
		private void _UpdateSelectedByMouseWithExpand()
		{
			IntRange current_selected = (selectedList.Count() == 0)? null : selectedList.Last();
			if (current_selected == null) {
				current_selected = new IntRange();
				current_selected.from = current_selected.to = 0;
				selectedList.Add(current_selected);
			}
			current_selected.to = _GetCandidateIndexOnMouse();
		}

		/// <summary>
		/// update selected list by mouse
		/// </summary>
		private void _UpdateSelectedByMouseWithoutOption()
		{
			int index = _GetCandidateIndexOnMouse();
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

		/// <summary>
		/// Update selected list by keyboard with expand key 
		/// </summary>
		private void _UpdateSelectedByKeyboardWithExpand()
		{
			IntRange current_selected = (selectedList.Count() == 0)? null : selectedList.Last();

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

		/// <summary>
		/// Update selected list by keyboard without expand key 
		/// </summary>
		private void _UpdateSelectedByKeyboardWithoutExpand()
		{
			IntRange current_selected = (selectedList.Count() == 0)? null : selectedList.Last();

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

		/// <summary>
		/// Update selected list by keyboard with round go key
		/// </summary>
		private void _UpdateSelectedByRoundGoKey()
		{
			int result_count = candidateList.Count();
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

		/// <summary>
		/// Update selected list by command select all 
		/// </summary>
		private void _UpdateSelectedByCommandSelectAll()
		{
			IntRange current_selected = (selectedList.Count() == 0)? null:selectedList.Last();
			current_selected = (selectedList.Count() == 0)? null:selectedList.Last();
			if (current_selected == null) {
				selectedList.Add(current_selected = new IntRange());
				current_selected.from = 0;
				current_selected.to   = candidateList.Count() - 1;
			}
			else {
				int min = Mathf.Min(current_selected.from, current_selected.to);
				int max = Mathf.Max(current_selected.from, current_selected.to);
				if (min == 0 && max == candidateList.Count() - 1) {
					selectedList.Clear();
				}
				else {
					current_selected.from = 0;
					current_selected.to   = candidateList.Count() - 1;
				}
			}
			Event.current.Use();
		}

		/// <summary>
		/// update selected list by keyboard
		/// </summary>
		private void _UpdateSelectedByKeyboard()
		{
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

		/// <summary>
		/// update candidate list by UnifredFeature instance
		/// </summary>
        private void _UpdateCandidate()
		{
            if (prevSearchWord != searchWord) {
                candidateList = feature.UpdateCandidate(searchWord).ToList();
			}
        }

		/// <summary>
		/// display candidate list
		/// </summary>
		private void _DisplayCandidate()
		{
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

			float row_height = feature.GetRowHeight();
			float viewable_size = feature.GetWindowSize().y - _GetRowStartHeight();
			int offset = Mathf.Max(0, Mathf.FloorToInt(scrollPosition.y / row_height));
			int last   = Mathf.Min(candidateList.Count(), (int)((scrollPosition.y + viewable_size + row_height) / row_height));
			int count  = last - offset;
			float rest = (candidateList.Count() - count + 1) * row_height - scrollPosition.y;
	
			GUILayout.Space(scrollPosition.y);
			feature.Draw(searchWord, candidateList, selectedList, offset, count);
			GUILayout.Space(rest);

			EditorGUILayout.EndScrollView();
		}

		private Vector2 _CulcScrollPosition()
		{
			Vector2 result = scrollPosition;

			float select_area_height = feature.GetWindowSize().y - _GetRowStartHeight();
			int scroll_start_index = Mathf.CeilToInt(scrollPosition.y / feature.GetRowHeight());
			int scroll_end_index   = scroll_start_index + Mathf.FloorToInt(select_area_height / feature.GetRowHeight()) - 1;
			//adjustment

			IntRange current_selected = (selectedList.Count() == 0)? null:selectedList.Last();
			int latest_index = current_selected == null? 0:current_selected.to;

			if (latest_index <= scroll_start_index) {
				int diff = scroll_start_index - latest_index + 1;
				result.y -= diff * feature.GetRowHeight();
				float min = 0f;
				result.y = Mathf.Max(result.y, min);
			}
			else if(scroll_end_index <= latest_index) {
				int diff = latest_index - scroll_end_index + 1;
				result.y += diff * feature.GetRowHeight();
				float max = (candidateList.Count() + 1) * feature.GetRowHeight() - select_area_height;
				result.y = Mathf.Min(result.y, max);
			}
			return result;
		}
		
		private void _ResizeWindow()
		{
			Vector2 size = feature.GetWindowSize();
			UnifredWindow window = EditorWindow.GetWindow<UnifredWindow>();
			window.minSize = window.maxSize = size;
			window.position = Input.MakeMouseBasedPosition(size.x, size.y, window.position.position);
		}
		
		private void _DisplaySearchTextField()
		{
			int selected_count = IntRange.Split(selectedList).Count();
			string candidate_count = "(" + selected_count + "/" + candidateList.Count() + ")  ";

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(candidate_count + feature.GetDescription(), descUIStyle);
			EditorGUILayout.Space();
			isExecuteFromMouse = GUILayout.Button("execute");
			GUILayout.EndHorizontal();

			const string controlName = "search_word";
			GUI.SetNextControlName(controlName);
            searchWord = GUILayout.TextField((searchWord ?? ""), searchUIStyle, GUILayout.ExpandWidth(true));
            GUI.FocusControl(controlName);
			_MakeCaretEndOfText();
		}
		
		private void _MakeCaretEndOfText()
		{
			var editor = (TextEditor) GUIUtility.GetStateObject(typeof (TextEditor), GUIUtility.keyboardControl);            
			editor.pos = searchWord.Length;
			editor.selectPos = searchWord.Length;
		}
		
		private void _OnPressedDoneKey()
		{
			List<T> selected_list = new List<T>();
			foreach (IntRange select in selectedList) {
				int min = Mathf.Min(select.from, select.to);
				int max = Mathf.Max(select.from, select.to);
				for (int i = min ; i <= max ; ++i) {
					selected_list.Add(candidateList.ElementAt(i));
				}
			}
			feature.Select(searchWord, selected_list);
		}

		private void _InitUnifredWindowStyle()
		{
			string skin_prefab_path = feature.GetGuiSkinPrefabPath();
			if (!string.IsNullOrEmpty(skin_prefab_path)) {
				GUISkin ui_skin = AssetDatabase.LoadAssetAtPath(feature.GetGuiSkinPrefabPath(), typeof(GUISkin)) as GUISkin;
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

		private void _Initialize(UnifredFeatureBase<T> instance, string word)
		{
			isInitializeWindowPosition = false;
			searchWord = word;
			prevSearchWord = null;
			searchUIStyle = null;
			candidateList = new List<T>();
			selectedList.Clear();
			this.feature = instance;
			instance.OnInit();
		}
	}
}
