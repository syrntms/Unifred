﻿using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Unifred
{
	public class GameObjectUtility
	{
		public static IEnumerable<GameObject> FindAllInHierarchy()
		{
			return Resources.FindObjectsOfTypeAll<GameObject>().Distinct();
		}
	}
}
