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
		EditorGUIUtility.LookLikeControls(80f);

		List<UIDrawCall> drawcalls = panel.drawCalls;
		int widgets = 0;
		foreach (UIDrawCall dc in drawcalls) widgets += dc.widgets;

		EditorGUILayout.LabelField("Widgets", widgets.ToString());
		EditorGUILayout.LabelField("Draw Calls", drawcalls.Count.ToString());
		panel.hidden = EditorGUILayout.Toggle("Hidden", panel.hidden);
	}
}