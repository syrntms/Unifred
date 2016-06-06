//using UnityEngine;
//using UnityEngine.UI;
//using UnityEditor;
//using System;
//using System.Linq;
//using System.Collections.Generic;
//
//namespace Unifred
//{
//	[InitializeOnLoad]
//	public class OverridePrefabIconExtension : HierarchyDrawerBase
//	{
//		public bool OnGUI(Rect r, int instanceID, Dictionary<int, object> log)
//		{
//			throw new NotImplementedException();
//		}
//		public object UpdateData(int instanceId)
//		{
//			throw new NotImplementedException();
//		}
//		public bool IsEnable {
//			get {
//				throw new NotImplementedException();
//			}
//			set {
//				throw new NotImplementedException();
//			}
//		}
//
//		private static Texture2D iconTexture;
//		private static Dictionary<int, List<SerializedObject>> idToSerializedObject 
//			= new Dictionary<int, List<SerializedObject>>();
//
//		static OverridePrefabIconExtension()
//		{
//			HierarchyDrawerManager.AddDrawer(new OverridePrefabIconExtension());
//			iconTexture = AssetDatabase.LoadAssetAtPath(
//				"Assets/HierarchyIconExtension/Editor/HierarchyIconExtension/Feature/Bob.png",
//				typeof(Texture2D)
//			) as Texture2D;
//			EditorApplication.hierarchyWindowChanged += resetSerializedObject;
//		}
//
//		public int GetPriority()
//		{
//			return 1;
//		}
//
//		public Texture2D GetDisplayIcon(int instanceId)
//		{
//			var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
//			var root = PrefabUtility.FindPrefabRoot(go);
//			if (root == null) {
//				return null;
//			}
//
//			bool isDisplay = hasOverrideProperty(go);
//			return isDisplay ? iconTexture : null;
//		}
//
//		private static void resetSerializedObject()
//		{
//			idToSerializedObject.Clear();
//		}
//
//		private static List<SerializedObject> createSerializedObjectList(GameObject go)
//		{
//			var components = go.GetComponents<Component>();
//			var list = components
//				.Where(component => component != null)
//				.Select(component => new SerializedObject(component))
//				.ToList();
//			list.Add(new SerializedObject(go));
//
//			return list.Distinct().ToList();
//		}
//
//		private static bool isAddComponent(GameObject go)
//		{
//			var components = go.GetComponents<Component>();
//			foreach (var component in components) {
//				bool isAdd = PrefabUtility.IsComponentAddedToPrefabInstance(component);
//				if (isAdd) {
//					return true;
//				}
//			}
//			return false;
//		}
//
//		private static bool hasOverrideProperty(GameObject go)
//		{
//			var isAdd = isAddComponent(go);
//			if (isAdd) {
//				return true;
//			}
//
//			List<SerializedObject> list;
//			bool isExist = idToSerializedObject.TryGetValue(go.GetInstanceID(), out list);
//			if (isExist) {
//				//+1 is for gameobject
//				if (list.Count != go.GetComponents<Component>().Count() + 1) {
//					list = createSerializedObjectList(go);
//					idToSerializedObject[go.GetInstanceID()] = list;
//				}
//				else {
//					foreach (var item in list) {
//						item.UpdateIfDirtyOrScript();
//					}
//				}
//			}
//			else {
//				list = createSerializedObjectList(go);
//				idToSerializedObject.Add(go.GetInstanceID(), list);
//			}
//
//			if (list.Count <= 0) {
//				return go.transform
//					.Cast<Transform>()
//					.Any(child => hasOverrideProperty(child.gameObject));
//			}
//
//			var ignorePropertyNames = getIgnorePropertyNames();
//
//			foreach (var serializedObject in list) {
//				var iterator = serializedObject.GetIterator();
//				var isSkip = false;
//
//				while (true) {
//					bool isContinue = iterator.Next(!isSkip);
//					if (!isContinue) {
//						break;
//					}
//
//					isSkip = ignorePropertyNames.Contains(iterator.name);
//					if (isSkip) {
//						continue;
//					}
//
//					if (iterator.prefabOverride) {
//						return true;
//					}
//				}
//			}
//			return go.transform.Cast<Transform>().ToList().Any(child => hasOverrideProperty(child.gameObject));
//		}
//
//		private static string[] getIgnorePropertyNames() {
//			//ignore these property
//			//because all gameobject automatically serialized these property, when user put prefab in scene.
//			string[] ignorePropertyNames = {
//				"m_LocalRotation",
//				"m_LocalPosition",
//				"m_RootOrder",
//				"m_AnchorMin",
//				"m_AnchorMax",
//				"m_AnchoredPosition",
//				"m_SizeDelta",
//				"m_Pivot",	
//				"m_Name",	//auto renamed when object was duped (ctrl + D)
//			};
//			return ignorePropertyNames;
//		}
//	}
//}
