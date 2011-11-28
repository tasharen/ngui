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

		List<UIWidget> widgets = panel.widgets;

		panel.hidden = EditorGUILayout.Toggle("Hidden", panel.hidden);
		EditorGUILayout.LabelField("Widgets", widgets.Count.ToString());
		EditorGUILayout.LabelField("Draw Calls", drawcalls.Count.ToString());

		// We want to figure out which materials get merged
		Dictionary<Material, int> mats = new Dictionary<Material, int>();
		foreach (Material mat in panel.mergeable) mats[mat] = 1;

		foreach (UIWidget w in widgets)
		{
			if (!mats.ContainsKey(w.material))
			{
				// The material is absent -- make note of it
				mats[w.material] = 0;
			}
			else if (mats[w.material] == 1)
			{
				// The material is present and is being merged
				mats[w.material] = 2;
			}
		}

		// List all materials that are in use
		if (mats.Count > 0)
		{
			GUITools.DrawSeparator();

			foreach (KeyValuePair<Material, int> val in mats)
			{
				if (val.Value == 0)
				{
					// Materials that aren't being merged should show up as red.
					GUI.backgroundColor = Color.red;

					if (GUILayout.Button("\"" + val.Key.name + "\" is separate"))
					{
						panel.mergeable.Add(val.Key);
						panel.Merge();
					}
				}
				else
				{
					// Materials that get merged but aren't being used should be orange.
					// Materials that get merged and are being used should be green.
					GUI.backgroundColor = (val.Value == 1) ? new Color(1f, 0.5f, 0f) : Color.green;

					if (GUILayout.Button("\"" + val.Key.name + "\" is merged"))
					{
						panel.mergeable.Remove(val.Key);
						panel.Merge();
					}
				}
			}
			GUI.backgroundColor = Color.white;
		}

		foreach (UIDrawCall dc in drawcalls)
		{
			GUITools.DrawSeparator();
			EditorGUILayout.ObjectField("Material", dc.material, typeof(Material), false);
			EditorGUILayout.LabelField("Triangles", dc.triangles.ToString());
		}
	}
}