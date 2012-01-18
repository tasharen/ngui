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
			DrawRow(null, false);
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
				if (type != PrefabType.Prefab) DrawRow(panel, panel == selectedPanel);
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

	void DrawRow (UIPanel panel, bool highlight)
	{
		string panelName, widgetCount, drawCalls, clipping;

		if (panel != null)
		{
			panelName = panel.name;
			widgetCount = panel.widgets.Count.ToString();
			drawCalls = panel.drawCalls.Count.ToString();
			clipping = (panel.clipping != UIDrawCall.Clipping.None) ? "Yes" : "";
		}
		else
		{
			panelName = "Panel";
			widgetCount = "Widgets";
			drawCalls = "DCs";
			clipping = "Clip";
		}

		GUILayout.BeginHorizontal();
		{
			bool disabled = (panel != null && !panel.gameObject.active);

			GUI.color = Color.white;

			if (panel != null)
			{
				if (panel.gameObject.active != EditorGUILayout.Toggle(panel.gameObject.active, GUILayout.Width(20f)))
				{
					panel.gameObject.SetActiveRecursively(!panel.gameObject.active);
					EditorUtility.SetDirty(panel.gameObject);
				}
			}
			else
			{
				GUILayout.Space(30f);
			}

			if (disabled)
			{
				GUI.color = highlight ? new Color(0f, 0.5f, 0.8f) : Color.grey;
			}
			else
			{
				GUI.color = highlight ? new Color(0f, 0.8f, 1f) : Color.white;
			}

			GUILayout.Label(panelName, GUILayout.MinWidth(100f));
			GUILayout.Label(widgetCount, GUILayout.Width(50f));
			GUILayout.Label(drawCalls, GUILayout.Width(30f));
			GUILayout.Label(clipping, GUILayout.Width(40f));

			GUI.color = disabled ? new Color(0.7f, 0.7f, 0.7f) : Color.white;

			if (panel != null)
			{
				bool debug = (panel.debugInfo == UIPanel.DebugInfo.Geometry);

				if (debug != EditorGUILayout.Toggle(debug, GUILayout.Width(20f)))
				{
					// debug != value, so it's currently inverse
					panel.debugInfo = debug ? UIPanel.DebugInfo.Gizmos : UIPanel.DebugInfo.Geometry;
					EditorUtility.SetDirty(panel);
				}
			}
			else
			{
				GUILayout.Label("DB", GUILayout.Width(20f));
			}

			if (panel)
			{
				if (GUILayout.Button("Select", GUILayout.Width(50f)))
				{
					Selection.activeGameObject = panel.gameObject;
					EditorUtility.SetDirty(panel.gameObject);
				}
			}
			else
			{
				GUILayout.Space(60f);
			}
		}
		GUILayout.EndHorizontal();
	}
}