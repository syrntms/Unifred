using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Unifred
{
	public class GameObjectUtility
	{
		public static IEnumerable<GameObject> FindAll()
		{
			return Resources.FindObjectsOfTypeAll(typeof(GameObject))
				.Select( g => g as GameObject )
				.Distinct();
		}

		public static IEnumerable<GameObject> FindAllInHierarchy()
		{
			return FindAll()
				.Where( go => {
					var path = AssetDatabase.GetAssetOrScenePath(go);
					return path.Contains(".unity") || string.IsNullOrEmpty(path);
				});
		}
	}
}					