using UnityEngine;

namespace Unifred
{
	public class Input
	{
		public static Rect MakeMouseBasedPosition(float width, float height, Vector2 offset)
		{
			var pos = Event.current.mousePosition + offset;
			return new Rect(pos.x, pos.y, width, height);
		}

		public static Vector2 GetMousePosition()
		{
			return Event.current.mousePosition;
		}

		public static bool IsMouseDown()
		{
			return Event.current.isMouse && Event.current.type == EventType.mouseDown;
		}

		public static bool IsMouse()
		{
			return Event.current.isMouse;
		}

		public static bool IsPressedKey(KeyCode keyCode)
		{
			return Event.current.type == EventType.keyDown && Event.current.keyCode == keyCode;
		}

		public static bool IsPressedDownKey()
		{
			return IsPressedKey(KeyCode.DownArrow);
		}

		public static bool IsPressedUpKey()
		{
			return IsPressedKey(KeyCode.UpArrow);
		}

		public static bool IsPressedTabKey()
		{
			return IsPressedKey(KeyCode.Tab);
		}

		public static bool IsPressedEscapeKey()
		{
			return IsPressedKey(KeyCode.Escape);
		}

		public static bool IsPressedReturnKey()
		{
			return IsPressedKey(KeyCode.Return);
		}

		public static bool IsPressedCommandKey()
		{
			return _IsPlatformMac()? Event.current.command:Event.current.control;
		}

		public static bool IsPressedShiftKey() {
			return Event.current.shift;
		}

		private static bool _IsPlatformMac()
		{
			return Application.platform == RuntimePlatform.OSXEditor;
		}

	}

}
