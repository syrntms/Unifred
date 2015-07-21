using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Unifred
{
	public class Styles
	{
		public static GUIStyle Entire;
		public static GUIStyle Header;
		public static GUIStyle Body;
		public static GUIStyle HeaderTop;
		public static GUIStyle HeaderBottom;
		public static GUIStyle HeaderTopDescription;
		public static GUIStyle HeaderTopExecuteButton;
		public static GUIStyle HeaderBottomSearchBox;
		public static GUIStyle BodyNormalRow;
		public static GUIStyle BodySelectedRow;
		public static GUIStyle BodySelectedGroup;
		public static GUIStyle BodyVerticalScrollBar;
		public static GUIStyle BodyHorizontalScrollBar;

		public static void Setup()
		{
			Entire = new GUIStyle() {
				normal = new GUIStyleState() {
					background = TextureUtility.MakeFrameTexture(Color.white, Color.black),
				},
				border = new RectOffset(7, 8, 7, 8),
			};

			Header = new GUIStyle() { };

			Body = new GUIStyle() {
				normal = new GUIStyleState() {
					background = TextureUtility.MakeFrameTexture(Color.gray / 1.5f, Color.gray / 1.5f),
				},
				border = new RectOffset(7, 8, 7, 8),
				margin = new RectOffset(1, 1, 2, 2),
				padding = new RectOffset(5, 5, 3, 3),
			};

			HeaderTop = new GUIStyle() {
				normal = new GUIStyleState() {
					background = TextureUtility.MakeFrameTexture(Color.black, (Color.blue / 5f + Color.gray * 1.5f)),
				},
				border = new RectOffset(7, 8, 7, 8),
				margin = new RectOffset(1, 1, 2, 2),
				padding = new RectOffset(10, 10, 3, 3),
			};

			HeaderBottom = new GUIStyle() {};

			HeaderTopDescription = new GUIStyle() {};

			HeaderTopExecuteButton = new GUIStyle() {
				normal = new GUIStyleState() {
					background = TextureUtility.MakeFrameTexture(Color.blue + Color.gray * 1.5f, Color.gray),
					textColor = Color.white,
				},
				active = new GUIStyleState() {
					background = TextureUtility.MakeFrameTexture(Color.white + Color.gray * 1.5f, Color.black),
					textColor = Color.white,
				},
				border = new RectOffset(7, 8, 7, 8),
				margin = new RectOffset(30, 30, 2, 2),
				padding = new RectOffset(10, 10, 3, 3),
				alignment = TextAnchor.MiddleCenter,
			};

			HeaderBottomSearchBox = new GUIStyle() {
				normal = new GUIStyleState() {
					background = TextureUtility.MakeFrameTexture(Color.blue, Color.white),
					textColor = Color.black,
				},
				border = new RectOffset(7, 8, 7, 8),
				margin = new RectOffset(30, 30, 2, 2),
				padding = new RectOffset(10, 10, 3, 3),
			};

			BodyNormalRow = new GUIStyle() {};
			BodySelectedRow = new GUIStyle() {};

			BodySelectedGroup = new GUIStyle() {
				normal = new GUIStyleState() {
					background = TextureUtility.MakeFrameTexture(Color.white, Color.gray),
				},
				border = new RectOffset(7, 8, 7, 8),
			};

			_InitScrollbarGUIStyle();

			BodyHorizontalScrollBar = GUIStyle.none;
		}

		private static void _InitScrollbarGUIStyle ()
		{
			string scrollbar_name = "unifredscrollbar";

			BodyVerticalScrollBar = new GUIStyle(GUI.skin.verticalScrollbar);
			BodyVerticalScrollBar.normal.background = TextureUtility.MakeFrameTexture(Color.gray, Color.black);
			BodyVerticalScrollBar.border = new RectOffset(7, 8, 7, 8);
			BodyVerticalScrollBar.name = scrollbar_name;

			GUIStyle thumb = new GUIStyle() {
				normal = new GUIStyleState {
					background = TextureUtility.MakeFrameTexture(Color.blue, Color.gray * 1.8f),
				},
				name = scrollbar_name + "thumb",
				border = new RectOffset(7, 8, 7, 8),
			};

			GUIStyle upbutton = new GUIStyle() {
				normal = new GUIStyleState {
					background = null,
				},
				name = scrollbar_name + "upbutton",
				border = new RectOffset(7, 8, 7, 8),
			};

			GUIStyle downbutton = new GUIStyle() {
				normal = new GUIStyleState {
					background = null,
				},
				name = scrollbar_name + "downbutton",
				border = new RectOffset(7, 8, 7, 8),
			};

			//registor styles to DEFAULT_SKIN(GUI.skin) for gui system to access these styles
			//gui cant access skin created by user.
			var additional_styles = new GUIStyle[]{
				upbutton,
				downbutton,
				thumb,
			};
			var custom_styles  = GUI.skin.customStyles.ToList();
			additional_styles.ForEach(style => custom_styles.Add(style));
			GUI.skin.customStyles = custom_styles.ToArray();
		}
	}
}
