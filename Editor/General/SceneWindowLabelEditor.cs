using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Unifred
{
	[CustomEditor(typeof(SceneWindowLabel))]
	public class SceneWindowLabelEditor : Editor
	{

		private static GUIStyle style;

		[DrawGizmo(GizmoType.Pickable | GizmoType.NonSelected | GizmoType.Selected)]
		public static void OnDrawGizmo(SceneWindowLabel sceneWindowLabel, GizmoType type)
		{
			string label = sceneWindowLabel.GetLabel();
			if (string.IsNullOrEmpty(label)) {
				return;
			}

			if (style == null) {
				style = _CreateStyle();
			}

			Handles.BeginGUI();
			Handles.Label(sceneWindowLabel.transform.position, label, style);
			Handles.EndGUI();
		}

		private static GUIStyle _CreateStyle()
		{
			return new GUIStyle() {
				normal =
				new GUIStyleState() {
					background	= TextureUtility.MakeFrameTexture(Color.gray, new Color32(0, 0, 0, 0x3F)),
					textColor	= new Color32(0xCC, 0x66, 0x00, 0xFF),
				},
				border			= new RectOffset(7, 8, 7, 8),
				padding			= new RectOffset(5, 5, 5, 5),
				margin			= new RectOffset(0, 0, 0, 0),
				contentOffset	= new Vector2(3, 3),
				richText		= true,
			};
		}
	}
}