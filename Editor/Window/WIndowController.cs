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

		private GUIStyle unifredStyle;
		private GUISkin skin;
		private GUIStyle[] styles;

		public enum StyleType
		{
			Entire = 0,
			Header,
			Body,
			HeaderTop,
			HeaderBottom,
			HeaderTopDescription,
			HeaderTopExecuteButton,
			HeaderBottomSearchBox,
			BodyNormalRow,
			BodySelectedRow,
			BodySelectedGroup,
		};

		protected static void ShowWindow(UnifredFeatureBase<T> instance, string input)
		{
			UnifredWindow window = EditorWindow.GetWindow<UnifredWindow>();
			window.Close();

			UnifredWindowController<T> controller = new UnifredWindowController<T>();
			controller._Initialize(instance, input);

			window = EditorWindow.CreateInstance<UnifredWindow>();
			window.ShowAsDropDown(default(Rect), Vector2.one);
		}

		private void onGUIOnce()
		{
			Texture2D texture = null;
			texture = (Config.BACK_GROUND_MODE == BackGroundMode.Transparent)?
				_CreateTransparentTexture():TextureUtility.MakeSolidTexture(Config.BACKGROUND_COLOR);

			unifredStyle = new GUIStyle() {
				normal = { background = texture, },
			};
			_ResizeWindow();
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

			if (Input.IsPressedDoneKey() || isExecuteFromMouse) {
				_OnPressedDoneKey();
				window.Close();
				return;
			}

			_UpdateSelectedByKeyboard();

			if (Event.current.isMouse) {
				_UpdateSelectedByMouse();
				if (!_IsScrollBarEvent()) {
					Event.current.Use();
				}
			}

			_ClampSelected();

			_DisplayEntire();
		}

		private bool _IsScrollBarEvent()
		{
			var mouse_position = Event.current.mousePosition;
			var space = styles[(int)StyleType.Entire].margin.right + styles[(int)StyleType.Entire].padding.right
				+ styles[(int)StyleType.Body].margin.right + styles[(int)StyleType.Body].padding.right;
			var is_scrollbar_event = feature.GetWindowSize().x - space - skin.verticalScrollbar.fixedWidth < mouse_position.x
				&& mouse_position.x < feature.GetWindowSize().x - space;
			return is_scrollbar_event;
		}

		private void _DisplayEntire()
		{
			GUILayout.BeginVertical(unifredStyle);
			GUILayout.BeginVertical(styles[(int)StyleType.Entire]);

			GUILayout.BeginVertical(styles[(int)StyleType.Header]);
			_DisplayHeader();
			GUILayout.EndVertical();

			if (prevSearchWord != searchWord) {
				selectedList.Clear();
			}

			_UpdateCandidate();

			GUILayout.BeginVertical(styles[(int)StyleType.Body]);
			_DisplayCandidate();
			GUILayout.EndVertical();

			prevSearchWord = searchWord;

			GUILayout.EndVertical();
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
			StyleType[] styleIndexes = {
				StyleType.BodyNormalRow,
				StyleType.BodySelectedRow,
			};

			float offset = styleIndexes.Select(index => styles[(int)index])
				.Sum(style => style.margin.top + style.margin.bottom + style.padding.top + style.padding.bottom);
			return offset + feature.GetRowHeight();
		}

		/// <summary>
		/// get row height of SelectedListUI of editor window from top.
		/// </summary>
		/// <returns>The get row start height.</returns>
		private float _GetRowStartHeight()
		{
			StyleType[] styleIndexes = {
				StyleType.Header,
				StyleType.HeaderBottom,
				StyleType.HeaderBottomSearchBox,
				StyleType.HeaderTop,
				StyleType.HeaderTopDescription,
			};

			float offset = styleIndexes.Select(index => styles[(int)index])
				.Sum(style => style.margin.top + style.margin.bottom + style.padding.top + style.padding.bottom);

			StyleType[] stringStyleIndexes = {
				StyleType.HeaderBottomSearchBox,
				StyleType.HeaderTopDescription,
			};
			offset += stringStyleIndexes
				.Select(index => styles[(int)index])
				.Sum(style => style.CalcSize(new GUIContent(searchWord)).y );

			StyleType[] topStyleIndexes = {
				StyleType.Entire,
				StyleType.Body,
			};
			offset += topStyleIndexes
				.Select(index => styles[(int)index])
				.Sum(style => style.margin.top + style.padding.top);
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
		/// update candidate list by UnifredFeature instance
		/// </summary>
        private void _UpdateCandidate()
		{
            if (prevSearchWord != searchWord) {
                candidateList = feature.UpdateCandidate(searchWord).ToList();
			}
        }

		/// <summary>
		/// display candidate area in editor window 
		/// </summary>
		private void _DisplayCandidate()
		{
			if (isScrollToSelected) {
				scrollPosition = _CulcScrollPosition();
				isScrollToSelected = false;
			}

			scrollPosition = EditorGUILayout.BeginScrollView(
				scrollPosition,
			    skin.horizontalScrollbar,
			    skin.verticalScrollbar,
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
					GUILayout.BeginVertical(styles[(int)StyleType.BodySelectedGroup]);
				}
				else if (is_selected_before && !is_selected) {
					GUILayout.EndVertical();
				}

				is_selected_before = is_selected;

				GUIStyle style = (is_selected)?
					styles[(int)StyleType.BodySelectedRow]:styles[(int)StyleType.BodyNormalRow];

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
		
		private void _DisplayHeader()
		{
			int selected_count = IntRange.Split(selectedList).Count();
			string candidate_count = "(" + selected_count + "/" + candidateList.Count() + ")  ";

			GUILayout.BeginHorizontal(styles[(int)StyleType.HeaderTop]);
			EditorGUILayout.LabelField(
				candidate_count + feature.GetDescription(),
				styles[(int)StyleType.HeaderTopDescription]
			);
			EditorGUILayout.Space();
			isExecuteFromMouse = GUILayout.Button("execute");
			GUILayout.EndHorizontal();

			const string controlName = "search_word";
			GUI.SetNextControlName(controlName);
			GUILayout.BeginHorizontal(styles[(int)StyleType.HeaderBottom]);
			searchWord = GUILayout.TextField(
				(searchWord ?? ""),
				styles[(int)StyleType.HeaderBottomSearchBox],
				GUILayout.ExpandWidth(true)
			);
			GUILayout.EndHorizontal();
            GUI.FocusControl(controlName);
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
			onGuiOnceAction = onGUIOnce;

			skin = Resources.Load("Unifred/CustomeStyles", typeof(GUISkin)) as GUISkin;
			styles = skin.customStyles;

			UnifredWindow.OnGUIAction = OnGUI;
		}

		private Texture2D _CreateTransparentTexture()
		{
			Rect rect = _GetWindowRect();
			var colors = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(
				rect.position,
				(int)rect.size.x,
				(int)rect.size.y
			);
			return TextureUtility.MakeTexture(
				colors,
				(int)rect.size.x,
				(int)rect.size.y
			);
		}

		private Rect _GetWindowRect()
		{
			switch (Application.platform) {
			case RuntimePlatform.OSXEditor:
				return _GetOSXWindowRect();
			case RuntimePlatform.WindowsEditor:
				return _GetWindowsWindowRect();
			}
			return new Rect(0, 0, 0, 0);
		}

		private Rect _GetWindowsWindowRect()
		{
			return new Rect(
				Input.GetMousePosition(),
				feature.GetWindowSize()
			);
		}

		private Rect _GetOSXWindowRect()
		{
			Vector2 screenSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
			Vector2 screenOffset = Vector2.zero;

			Vector2 startPosition = Input.GetMousePosition();
			float menu_height = EditorGUIUtility.GUIToScreenPoint(Vector2.zero).y;

			startPosition.x = Mathf.Max(startPosition.x, screenOffset.x);
			startPosition.x = Mathf.Min(startPosition.x, screenOffset.x + screenSize.x - feature.GetWindowSize().x);
			startPosition.y += menu_height;
			startPosition.y = Mathf.Max(startPosition.y, screenOffset.y + menu_height);
			startPosition.y = Mathf.Min(startPosition.y, screenOffset.y + screenSize.y - Config.DOCK_HEIGHT - feature.GetWindowSize().y);

			return new Rect(
				startPosition,
				feature.GetWindowSize()
			);
		}
	}
}
