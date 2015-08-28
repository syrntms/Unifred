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

			Header = new GUIStyle() {
				normal = new GUIStyleState() {
					background = Resources.Load("Unifred/Texture/UI/header") as Texture2D,
					textColor = Color.black,
				},
				border = new RectOffset(3, 4, 0, 0),
				margin = new RectOffset(3, 3, 2, 2),
			};

			Body = new GUIStyle() {
				margin = new RectOffset(1, 1, 2, 2),
				padding = new RectOffset(5, 5, 3, 3),
			};

			HeaderTop = new GUIStyle() {
				margin = new RectOffset(0, 0, 0, 5),
			};

			HeaderBottom = new GUIStyle() {};

			HeaderTopDescription = new GUIStyle() {
			};

			HeaderTopExecuteButton = new GUIStyle() {
				normal = new GUIStyleState() {
					background = Resources.Load("Unifred/Texture/UI/button_normal") as Texture2D,
					textColor = Color.black,
				},
				active = new GUIStyleState() {
					background = Resources.Load("Unifred/Texture/UI/button_pressed") as Texture2D,
					textColor = Color.black,
				},
				border = new RectOffset(7, 8, 0, 0),
				margin = new RectOffset(10, 30, 5, 5),
//				padding = new RectOffset(10, 10, 3, 3),
				alignment = TextAnchor.MiddleCenter,
			};

			HeaderBottomSearchBox = new GUIStyle() {
				normal = new GUIStyleState() {
					background = Resources.Load("Unifred/Texture/UI/box") as Texture2D,
					textColor = Color.black,
				},
				border = new RectOffset(10, 10, 10, 10),
				margin = new RectOffset(30, 30, 2, 2),
				padding = new RectOffset(10, 10, 3, 3),
			};

			BodyNormalRow = new GUIStyle() {};
			BodySelectedRow = new GUIStyle() {
				normal = new GUIStyleState() {
					background = TextureUtility.MakeSolidTexture(new Color32(204, 102, 0, 255)),
				},
			};

			BodySelectedGroup = new GUIStyle() {
//				normal = new GUIStyleState() {
//					background = TextureUtility.MakeFrameTexture(Color.white, Color.gray),
//				},
//				border = new RectOffset(7, 8, 7, 8),
			};

			_InitScrollbarGUIStyle();

			BodyHorizontalScrollBar = GUIStyle.none;
		}

		public static void CleanTexture()
		{
			Texture2D[] targets = {
				Entire.normal.background,
				BodySelectedRow.normal.background,
				BodyVerticalScrollBar.normal.background,
			};
			targets.ForEach(target => GameObject.DestroyImmediate(target));
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
					background = Resources.Load("Unifred/Texture/UI/slider") as Texture2D,
				},
				name = scrollbar_name + "thumb",
				border = new RectOffset(0, 0, 11, 11),
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
