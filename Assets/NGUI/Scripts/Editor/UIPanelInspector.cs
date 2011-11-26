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
		EditorGUILayout.LabelField("Draw Calls", drawcalls.Count.ToString());

		foreach (UIDrawCall dc in drawcalls)
		{
			GUITools.DrawSeparator();
			EditorGUILayout.ObjectField("Material", dc.material, typeof(Material), false);
			EditorGUILayout.LabelField("Widgets", dc.widgets.ToString());
			EditorGUILayout.LabelField("Triangles", dc.triangles.ToString());
			//bool merge = EditorGUILayout.Toggle("Merge Atlas", dc.merge);

			//if (merge != dc.merge)
			{
				//dc.merge = merge;
				//Debug.Log("TODO: Merge the atlas");

				// TODO: 
			}
		}
	}
}