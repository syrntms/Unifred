using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Unifred
{
	public static class Config
	{
		public static readonly int HISTORY_COUNT = 100;
		public static readonly int COPYCOMPONENT_HISTORY_COUNT = 10;
		public static readonly BackGroundMode BACK_GROUND_MODE = BackGroundMode.Transparent;

		//view
		public static readonly float DOCK_HEIGHT = 41;
		public static readonly Color BACKGROUND_COLOR = Color.gray;
	}

	public enum BackGroundMode{
		Transparent,
		Fill
	}
}
