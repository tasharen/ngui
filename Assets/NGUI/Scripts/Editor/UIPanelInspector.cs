using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(UIPanel))]
public class UIPanelInspector : Editor
{
	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI ()
	{
		UIPanel panel = target as UIPanel;
		List<UIDrawCall> drawcalls = panel.drawCalls;
		EditorGUIUtility.LookLikeControls(80f);

		GUITools.DrawSeparator();

		panel.hidden = EditorGUILayout.Toggle("Hidden", panel.hidden);
		EditorGUILayout.LabelField("Widgets", panel.widgets.ToString());
		EditorGUILayout.LabelField("Draw Calls", drawcalls.Count.ToString());

		foreach (UIDrawCall dc in drawcalls)
		{
			GUITools.DrawSeparator();
			EditorGUILayout.ObjectField("Material", dc.material, typeof(Material), false);
			EditorGUILayout.LabelField("Triangles", dc.triangles.ToString());

			// TODO:
			// 1. UIPanel needs to have a List<> of mergeable materials.
			// 2. Add toggles here for each draw call.
			// 3. When toggled, add/remove the DC's material from the UIPanel's list of mergeable materials and panel.Remerge().

			// NOTE:
			// Wouldn't it better to instead select a bunch of DCs, click on a Merge button, and have it save out a permanent PNG
			// that's then used instead? But how to create a permanent material? Hmm...
		}
	}
}