using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Panel wizard that allows enabling / disabling and selecting panels in the scene.
/// </summary>

public class UIPanelTool : EditorWindow
{
	/// <summary>
	/// Draw the custom wizard.
	/// </summary>

	void OnGUI ()
	{
		EditorGUIUtility.LookLikeControls(80f);

		UIPanel[] panels = Resources.FindObjectsOfTypeAll(typeof(UIPanel)) as UIPanel[];

		if (panels.Length > 0)
		{
			DrawRow("Panel", "Widgets", "DCs", "Clip", null, false);
			GUITools.DrawSeparator();

			UIPanel selectedPanel = null;
			Transform t = Selection.activeTransform;

			while (t != null && selectedPanel == null)
			{
				selectedPanel = t.GetComponent<UIPanel>();
				t = t.parent;
			}

			foreach (UIPanel panel in panels)
			{
#if UNITY_3_4
				PrefabType type = EditorUtility.GetPrefabType(panel.gameObject);
#else
				PrefabType type = PrefabUtility.GetPrefabType(panel.gameObject);
#endif
				if (type != PrefabType.Prefab)
				{
					DrawRow(panel.name,
						panel.widgets.Count.ToString(),
						panel.drawCalls.Count.ToString(),
						(panel.clipping != UIDrawCall.Clipping.None) ? "Yes" : "",
						panel, panel == selectedPanel);
				}
			}
		}
		else
		{
			GUILayout.Label("No UI Panels found in the scene");
		}
	}

	/// <summary>
	/// Helper function used to print things in columns.
	/// </summary>

	void DrawRow (string a, string b, string c, string d, UIPanel panel, bool highlight)
	{
		GUILayout.BeginHorizontal();
		{
			bool disabled = (panel != null && !panel.gameObject.active);

			if (disabled)
			{
				GUI.color = highlight ? new Color(0f, 0.5f, 0.8f) : Color.grey;
				GUILayout.Label(a, GUILayout.MinWidth(100f));
			}
			else
			{
				GUI.color = highlight ? new Color(0f, 0.8f, 1f) : Color.white;
				GUILayout.Label(a, GUILayout.MinWidth(100f));
			}
			
			GUILayout.Label(b, GUILayout.Width(50f));
			GUILayout.Label(c, GUILayout.Width(30f));
			GUILayout.Label(d, GUILayout.Width(40f));

			GUI.color = disabled ? new Color(0.7f, 0.7f, 0.7f) : Color.white;

			if (panel)
			{
				if (GUILayout.Button("Select", GUILayout.Width(50f)))
				{
					Selection.activeGameObject = panel.gameObject;
					EditorUtility.SetDirty(panel.gameObject);
				}

				if (GUILayout.Button(panel.gameObject.active ? "Deactivate" : "Activate", GUILayout.Width(80f)))
				{
					panel.gameObject.SetActiveRecursively(!panel.gameObject.active);
					EditorUtility.SetDirty(panel.gameObject);
				}
			}
			else
			{
				// Even though its 50+80... here it needs to be 10 more to align properly
				GUILayout.Space(140f);
			}
		}
		GUILayout.EndHorizontal();
	}
}