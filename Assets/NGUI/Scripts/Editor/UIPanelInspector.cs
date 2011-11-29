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
		//EditorGUILayout.LabelField("Widgets", panel.widgets.Count.ToString());
		EditorGUILayout.LabelField("Draw Calls", drawcalls.Count.ToString());

		foreach (UIDrawCall dc in drawcalls)
		{
			GUITools.DrawSeparator();
			EditorGUILayout.ObjectField("Material", dc.material, typeof(Material), false);
			EditorGUILayout.LabelField("Triangles", dc.triangles.ToString());
		}
	}
}