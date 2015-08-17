using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred
{
	public class UnifredWindowController<T>
	{
		private UnifredFeatureBase<T> feature;
		private string prevSearchWord = null;
		private string searchWord = null;
		private IEnumerable<T> candidateList = new List<T>();
		private List<IntRange> selectedList = new List<IntRange>();
		private Vector2 scrollPosition = Vector2.zero;
		private bool isScrollToSelected = false;
		private bool isExecuteFromMouse = false;
		private Action onGuiOnceAction = null;


		protected static void ShowWindow(UnifredFeatureBase<T> instance, string input)
		{
			UnifredWindow window = EditorWindow.GetWindow<UnifredWindow>();
			window.Close();

			UnifredWindowController<T> controller = new UnifredWindowController<T>();
			controller._Initialize(instance, input);

			window = EditorWindow.CreateInstance<UnifredWindow>();
			window.ShowAsDropDown(default(Rect), Vector2.one);
		}

		public void OnGUI()
		{
			var window = EditorWindow.GetWindow<UnifredWindow>();

			if (onGuiOnceAction != null) {
				onGuiOnceAction();
				onGuiOnceAction = null;
			}

			if (Input.IsPressedCancelKey()) {
				window.Close();
				return;
			}

			if (Input.IsPressedDoneKey()) {
				_OnPressedDoneKey();
				window.Close();
				return;
			}

			_UpdateSelectedByKeyboard();

			if (Event.current.isMouse) {
				_UpdateSelectedByMouse();
			}

			_ClampSelected();

			_DisplayEntire();

			if (isExecuteFromMouse) {
				_OnPressedDoneKey();
				window.Close();
				return;
			}
		}

		private bool _IsScrollBarEvent()
		{
			var mouse_position = Event.current.mousePosition;
			var space = Styles.Entire.margin.right + Styles.Entire.padding.right
				+ Styles.Body.margin.right + Styles.Body.padding.right;
			var is_scrollbar_event = feature.GetWindowSize().x - space - Styles.BodyVerticalScrollBar.fixedWidth < mouse_position.x
				&& mouse_position.x < feature.GetWindowSize().x - space;
			return is_scrollbar_event;
		}

		private void _DisplayEntire()
		{
			GUILayout.BeginVertical(Styles.Entire);

			_DisplayHeader();

			if (prevSearchWord != searchWord) {
				selectedList.Clear();
				scrollPosition = Vector2.zero;
                candidateList = feature.UpdateCandidate(searchWord).ToList();
			}

			_DisplayCandidate();

			prevSearchWord = searchWord;

			GUILayout.EndVertical();
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


		private float _GetRowHeight()
		{
			GUIStyle[] styleIndexes = {
				Styles.BodyNormalRow,
				Styles.BodySelectedRow,
			};

			float offset = styleIndexes
				.Sum(style => style.margin.top + style.margin.bottom + style.padding.top + style.padding.bottom);
			return offset + feature.GetRowHeight();
		}

		/// <summary>
		/// get row height of SelectedListUI of editor window from top.
		/// </summary>
		/// <returns>The get row start height.</returns>
		private float _GetRowStartHeight()
		{
			GUIStyle[] styleIndexes = {
				Styles.Header,
				Styles.HeaderBottom,
				Styles.HeaderBottomSearchBox,
				Styles.HeaderTop,
				Styles.HeaderTopDescription,
			};

			float offset = styleIndexes
				.Sum(style => style.margin.top + style.margin.bottom + style.padding.top + style.padding.bottom);

			GUIStyle[] stringStyleIndexes = {
				Styles.HeaderBottomSearchBox,
				Styles.HeaderTopDescription,
			};
			offset += stringStyleIndexes
				.Sum(style => style.CalcSize(new GUIContent(searchWord)).y );

			GUIStyle[] topStyleIndexes = {
				Styles.Entire,
				Styles.Body,
			};
			offset += topStyleIndexes.Sum(style => style.margin.top + style.padding.top);
			return offset + styleIndexes.Count();
		}

		/// <summary>
		/// get index of CandidateList by mouse clicked.
		/// </summary>
		/// <returns>The get index on mouse clicked.</returns>
		private int _GetCandidateIndexOnMouse()
		{
			float mouse_y = Event.current.mousePosition.y;
			float scroll_y = Mathf.Floor(scrollPosition.y / feature.GetRowHeight()) * feature.GetRowHeight();
			float offset = _GetRowStartHeight();
			var index = Mathf.FloorToInt((mouse_y + scroll_y - offset) / feature.GetRowHeight());
			return index;
		}

		/// <summary>
		/// update selected list by mouse input
		/// </summary>
		private void _UpdateSelectedByMouse()
		{
			bool isRowClicked = !_IsScrollBarEvent() && _GetRowStartHeight() < Input.GetMousePosition().y;

			if (!Input.IsMouseDown() || !isRowClicked) {
				return;
			}

			if (Input.IsPressedExpandKey() && feature.IsMultipleSelect()) {
				_UpdateSelectedByMouseWithExpand();
			}
			else {
				_UpdateSelectedByMouseWithoutOption();
			}
			Event.current.Use();
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
			int index = _GetCandidateIndexOnMouse();
			if (current_selected == null) {
				selectedList.Add(new IntRange(0, index));
			}
			else {
				current_selected.to = index;
			}
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
				selectedList.Add(new IntRange(index, index));
			}
		}

		/// <summary>
		/// Update selected list by keyboard with expand key 
		/// </summary>
		private void _UpdateSelectedByKeyboardWithExpand()
		{
			bool is_first = selectedList.Count() == 0;
			IntRange current_selected = is_first? new IntRange() : selectedList.Last();
			bool is_update = false;

			if (Input.IsPressedDownKey()) {
				is_update = true;
				if (!is_first) {
					current_selected.to++;
				}
			}

			if (Input.IsPressedUpKey()) {
				is_update = true;
				current_selected.to--;
			}

			if (is_update) {
				isScrollToSelected = true;
				if (is_first) {
					selectedList.Add(current_selected);
				}
			}
		}

		/// <summary>
		/// Update selected list by keyboard without expand key 
		/// </summary>
		private void _UpdateSelectedByKeyboardWithoutExpand()
		{
			bool is_first = selectedList.Count() == 0;
			IntRange current_selected = is_first? new IntRange() : selectedList.Last();
			bool is_update = false;

			if (Input.IsPressedDownKey()) {
				is_update = true;
				if (!is_first) {
					current_selected.from = ++current_selected.to;
				}
			}
			if (Input.IsPressedUpKey()) {
				is_update = true;
				current_selected.from = --current_selected.to;
			}

			if (is_update) {
				isScrollToSelected = true;
				selectedList.Clear();
				selectedList.Add(current_selected);
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

			bool is_first = selectedList.Count() == 0;
			IntRange current_selected = is_first? new IntRange():selectedList.Last();
			selectedList.Clear();
			selectedList.Add(current_selected);
			isScrollToSelected = true;

			int next = 0;
			if (Input.IsPressedReverseKey()) {
				next = --current_selected.to;
			}
			else {
				next = (is_first)? 0:++current_selected.to;
			}
			current_selected.from = current_selected.to = (next + result_count) % result_count;
		}

		/// <summary>
		/// Update selected list by command select all 
		/// </summary>
		private void _UpdateSelectedByCommandSelectAll()
		{
			IntRange current_selected = (selectedList.Count() == 0)? null:selectedList.Last();
			selectedList.Clear();

			if (current_selected != null) {
				int min = Mathf.Min(current_selected.from, current_selected.to);
				int max = Mathf.Max(current_selected.from, current_selected.to);
				if (min == 0 && max == candidateList.Count() - 1) {
					return;
				}
			}
			selectedList.Add(new IntRange(0, candidateList.Count() - 1));
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
				Event.current.Use();
			}

			if (Event.current.type == EventType.ValidateCommand
			    && Event.current.commandName == "SelectAll"
		    ){
				_UpdateSelectedByCommandSelectAll();
				Event.current.Use();
			}
		}

		/// <summary>
		/// display candidate area in editor window 
		/// </summary>
		private void _DisplayCandidate()
		{
			GUILayout.BeginVertical(Styles.Body);
			if (isScrollToSelected) {
				scrollPosition = _CulcScrollPosition();
				isScrollToSelected = false;
			}
			scrollPosition = EditorGUILayout.BeginScrollView(
				scrollPosition,
			    Styles.BodyHorizontalScrollBar,
			    Styles.BodyVerticalScrollBar,
				null
			);

			float row_height = feature.GetRowHeight();
			float viewable_size = feature.GetWindowSize().y - _GetRowStartHeight();
			int offset = Mathf.Max(0, Mathf.FloorToInt(scrollPosition.y / row_height));
			int last   = Mathf.Min(candidateList.Count(), (int)((scrollPosition.y + viewable_size + row_height) / row_height));
			int count  = last - offset;
			float rest = (candidateList.Count() - count + 1) * row_height - scrollPosition.y;
	
			GUILayout.Space(scrollPosition.y);

			_DisplayEachCandidate(searchWord, offset, count);

			GUILayout.Space(rest);

			EditorGUILayout.EndScrollView();
			GUILayout.EndVertical();
		}

		/// <summary>
		/// disp each element of candidate list with feature base instance.
		/// </summary>
		/// <param name="word">Word.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="count">Count.</param>
		private void _DisplayEachCandidate(string word, int offset, int count)
		{
			IEnumerable<int> uniq_selected_list = IntRange.Split(selectedList);
			bool is_selected_before = false;
			bool is_selected;

			for (int i = offset ; i < offset + count ; ++i) {
				is_selected = uniq_selected_list.Contains(i);
				if (!is_selected_before && is_selected) {
					GUILayout.BeginVertical(Styles.BodySelectedGroup);
				}
				else if (is_selected_before && !is_selected) {
					GUILayout.EndVertical();
				}

				is_selected_before = is_selected;

				GUIStyle style = (is_selected)? Styles.BodySelectedRow:Styles.BodyNormalRow;

				T candidate = candidateList.ElementAt(i);
	            GUILayout.BeginHorizontal(style);
				feature.Draw(word, candidate, is_selected);
	            GUILayout.EndHorizontal();
			}

			if (is_selected_before) {
				GUILayout.EndVertical();
			}
		}

		private Vector2 _CulcScrollPosition()
		{
			Vector2 result = scrollPosition;

			float select_area_height = feature.GetWindowSize().y - _GetRowStartHeight();
			int scroll_start_index = Mathf.CeilToInt(scrollPosition.y / feature.GetRowHeight());
			int scroll_end_index   = scroll_start_index + Mathf.FloorToInt(select_area_height / feature.GetRowHeight()) - 1;

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
		
		private void _DisplayHeader()
		{
			GUILayout.BeginVertical(Styles.Header);

			//header top
			string header_label = string.Format("<color=#00AFFF>(<color=#CC6600>{0}</color> / {1}) {2}</color>",
				IntRange.Split(selectedList).Count(),
	                candidateList.Count(),
			    feature.GetDescription()
            );
			GUILayout.BeginHorizontal(Styles.HeaderTop);
			EditorGUILayout.LabelField(header_label, Styles.HeaderTopDescription);

			EditorGUILayout.Space();

			isExecuteFromMouse = GUILayout.Button("execute", Styles.HeaderTopExecuteButton);
			GUILayout.EndHorizontal();

			//header bottom
			const string controlName = "search_word";
			GUI.SetNextControlName(controlName);
            GUI.FocusControl(controlName);
			GUILayout.BeginHorizontal(Styles.HeaderBottom);
			searchWord = GUILayout.TextField(
				(searchWord ?? ""),
				Styles.HeaderBottomSearchBox,
				GUILayout.ExpandWidth(true)
			);
			_MakeCursorEndOfText();
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}

		private void _MakeCursorEndOfText()
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

		private void _Initialize(UnifredFeatureBase<T> instance, string word)
		{
			searchWord = word;
			prevSearchWord = null;
			candidateList = new List<T>();
			selectedList.Clear();
			this.feature = instance;
			instance.OnInit();
			onGuiOnceAction = () => {_ResizeWindow(); Styles.Setup();};

			UnifredWindow.OnGUIAction = OnGUI;
		}
	}
}
