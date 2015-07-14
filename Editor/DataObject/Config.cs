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
		public static readonly Color NORMAL_ROW_COLOR = Color.clear;
		public static readonly Color SELECTED_ROW_COLOR = Color.cyan * 0.55f;
	}
}
