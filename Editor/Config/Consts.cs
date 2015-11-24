using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Unifred
{
	public static class Consts
	{
		public static readonly int		HISTORY_COUNT				= 100;
		public static readonly int		COPYCOMPONENT_HISTORY_COUNT	= 10;
		public static readonly int		DEFAULT_WIDTH				= 600;
		public static readonly int		DEFAULT_HEIGHT				= 400;
	}

	public enum CandidateSelectMode
	{
		Single = 0,
		Multiple,
		SingleImmediately,
	}
}
