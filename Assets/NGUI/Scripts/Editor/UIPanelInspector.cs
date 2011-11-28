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

		Dictionary<Material, bool> mats = new Dictionary<Material, bool>();
		foreach (Material mat in panel.mergeable) mats[mat] = true;

		foreach (UIWidget w in widgets)
		{
			if (!mats.ContainsKey(w.material))
			{
				mats[w.material] = false;
			}
		}

		if (mats.Count > 0)
		{
			GUITools.DrawSeparator();

			foreach (KeyValuePair<Material, bool> val in mats)
			{
				if (val.Value)
				{
					// Start with an orange color
					Color c = new Color(1f, 0.5f, 0f);

					// Check the draw calls -- is this material being used? If so, change the color to green.
					if (Event.current.type == EventType.Repaint)
					{
						foreach (UIDrawCall dc in panel.drawCalls)
						{
							if (dc.material == val.Key)
							{
								c = Color.green;
								break;
							}
						}
					}

					GUI.backgroundColor = c;

					if (GUILayout.Button(val.Key.name + " is merged"))
					{
						panel.mergeable.Remove(val.Key);
						panel.Merge();
					}
				}
				else
				{
					GUI.backgroundColor = Color.red;

					if (GUILayout.Button(val.Key.name + " is separate"))
					{
						panel.mergeable.Add(val.Key);
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