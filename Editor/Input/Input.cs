﻿using UnityEngine;

namespace Unifred
{
	public class Input
	{
		public static Rect MakeMouseBasedPosition(float width, float height, Vector2 offset)
		{
			var pos = Event.current.mousePosition + offset;
			return new Rect(pos.x, pos.y, width, height);
		}

		public static bool IsMouseDown()
		{
			return Event.current.isMouse && Event.current.type == EventType.mouseDown;
		}

		public static bool IsMouse()
		{
			return Event.current.isMouse;
		}

		public static bool IsKeyDown(KeyCode keyCode)
		{
			return Event.current.type == EventType.keyDown && Event.current.keyCode == keyCode;
		}

		public static bool IsPressedDownKey()
		{
			return Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.DownArrow;
		}

		public static bool IsPressedOpenKey()
		{
			return _IsPlatformMac()? Event.current.command:Event.current.control;
		}

		public static bool IsPressedRoundGoKey()
		{
			return Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Tab;
		}

		public static bool IsPressedReverseKey() {
			return Event.current.shift;
		}

		public static bool IsPressedUpKey()
		{
			return Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.UpArrow;
		}

		public static bool IsPressedExpandKey()
		{
			return Event.current.shift;
		}

		public static bool IsPressedToggleKey()
		{
			return _IsPlatformMac()? Event.current.command:Event.current.control;
		}

		public static bool IsPressedCancelKey()
		{
			return Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Escape;
		}

		public static bool IsPressedDoneKey()
		{
			return Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return;
		}

		private static bool _IsPlatformMac()
		{
			return Application.platform == RuntimePlatform.OSXEditor;
		}
	}

}