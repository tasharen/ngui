//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(UIPanel))]
public class UIPanelInspector : Editor
{
	/// <summary>
	/// Handles & interaction.
	/// </summary>

	public void OnSceneGUI ()
	{
		Event e = Event.current;

		switch (e.type)
		{
			case EventType.KeyDown:
			{
				if (e.keyCode == KeyCode.Escape)
				{
					Tools.current = Tool.Move;
					Selection.activeGameObject = null;
					e.Use();
				}
			}
			break;
		}
	}

	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI ()
	{
		UIPanel panel = target as UIPanel;
		NGUIEditorTools.SetLabelWidth(80f);
		EditorGUILayout.Space();

		float alpha = EditorGUILayout.Slider("Alpha", panel.alpha, 0f, 1f);

		if (alpha != panel.alpha)
		{
			NGUIEditorTools.RegisterUndo("Panel Alpha", panel);
			panel.alpha = alpha;
		}

		GUILayout.BeginHorizontal();
		bool norms = EditorGUILayout.Toggle("Normals", panel.generateNormals, GUILayout.Width(100f));
		GUILayout.Label("Needed for lit shaders");
		GUILayout.EndHorizontal();

		if (panel.generateNormals != norms)
		{
			panel.generateNormals = norms;
			UIPanel.SetDirty();
			EditorUtility.SetDirty(panel);
		}

		GUILayout.BeginHorizontal();
		bool cull = EditorGUILayout.Toggle("Cull", panel.cullWhileDragging, GUILayout.Width(100f));
		GUILayout.Label("Cull widgets while dragging them");
		GUILayout.EndHorizontal();

		if (panel.cullWhileDragging != cull)
		{
			panel.cullWhileDragging = cull;
			UIPanel.SetDirty();
			EditorUtility.SetDirty(panel);
		}

		GUILayout.BeginHorizontal();
		bool stat = EditorGUILayout.Toggle("Static", panel.widgetsAreStatic, GUILayout.Width(100f));
		GUILayout.Label("Check if widgets won't move");
		GUILayout.EndHorizontal();

		if (panel.widgetsAreStatic != stat)
		{
			panel.widgetsAreStatic = stat;
			UIPanel.SetDirty();
			EditorUtility.SetDirty(panel);
		}

		if (stat)
		{
			EditorGUILayout.HelpBox("Only mark the panel as 'static' if you know FOR CERTAIN that the widgets underneath will not move, rotate, or scale. Doing this improves performance, but moving widgets around will have no effect.", MessageType.Warning);
		}

		if (panel.showInPanelTool != EditorGUILayout.Toggle("Panel Tool", panel.showInPanelTool))
		{
			panel.showInPanelTool = !panel.showInPanelTool;
			EditorUtility.SetDirty(panel);
			EditorWindow.FocusWindowIfItsOpen<UIPanelTool>();
		}

		UIDrawCall.Clipping clipping = (UIDrawCall.Clipping)EditorGUILayout.EnumPopup("Clipping", panel.clipping);

		if (panel.clipping != clipping)
		{
			panel.clipping = clipping;
			EditorUtility.SetDirty(panel);
		}

		if (panel.clipping != UIDrawCall.Clipping.None)
		{
			Vector4 range = panel.clipRange;

			GUILayout.BeginHorizontal();
			GUILayout.Space(80f);
			Vector2 pos = EditorGUILayout.Vector2Field("Center", new Vector2(range.x, range.y));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Space(80f);
			Vector2 size = EditorGUILayout.Vector2Field("Size", new Vector2(range.z, range.w));
			GUILayout.EndHorizontal();

			if (size.x < 0f) size.x = 0f;
			if (size.y < 0f) size.y = 0f;

			range.x = pos.x;
			range.y = pos.y;
			range.z = size.x;
			range.w = size.y;

			if (panel.clipRange != range)
			{
				NGUIEditorTools.RegisterUndo("Clipping Change", panel);
				panel.clipRange = range;
				EditorUtility.SetDirty(panel);
			}

			if (panel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(80f);
				Vector2 soft = EditorGUILayout.Vector2Field("Softness", panel.clipSoftness);
				GUILayout.EndHorizontal();

				if (soft.x < 1f) soft.x = 1f;
				if (soft.y < 1f) soft.y = 1f;

				if (panel.clipSoftness != soft)
				{
					NGUIEditorTools.RegisterUndo("Clipping Change", panel);
					panel.clipSoftness = soft;
					EditorUtility.SetDirty(panel);
				}
			}

#if !UNITY_3_5 && !UNITY_4_0 && (UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8 || UNITY_BLACKBERRY)
			if (PlayerSettings.targetGlesGraphics == TargetGlesGraphics.OpenGLES_1_x)
			{
				EditorGUILayout.HelpBox("Clipping requires shader support!\n\nOpen File -> Build Settings -> Player Settings -> Other Settings, then set:\n\n- Graphics Level: OpenGL ES 2.0.", MessageType.Error);
			}
#endif
		}

		if (clipping != UIDrawCall.Clipping.None && !NGUIEditorTools.IsUniform(panel.transform.lossyScale))
		{
			EditorGUILayout.HelpBox("Clipped panels must have a uniform scale, or clipping won't work properly!", MessageType.Error);
			
			if (GUILayout.Button("Auto-fix"))
			{
				NGUIEditorTools.FixUniform(panel.gameObject);
			}
		}

		for (int i = 0; i < UIDrawCall.list.size; ++i)
		{
			UIDrawCall dc = UIDrawCall.list[i];

			if (dc.panel != panel) continue;

			string key = dc.keyName;
			bool wasOn = EditorPrefs.GetBool(key, true);
			bool shouldBeOn = NGUIEditorTools.DrawHeader(key + " of " + UIDrawCall.list.size, key);
			
			if (wasOn != shouldBeOn)
			{
				dc.isActive = shouldBeOn;
				UnityEditor.EditorUtility.SetDirty(panel);
			}

			if (shouldBeOn)
			{
				NGUIEditorTools.BeginContents();
				EditorGUILayout.ObjectField("Material", dc.material, typeof(Material), false);

				int count = 0;

				for (int b = 0; b < UIWidget.list.size; ++b)
				{
					UIWidget w = UIWidget.list[b];
					if (w.drawCall == dc)
						++count;
				}

				int initial = NGUITools.GetHierarchy(panel.cachedGameObject).Length + 1;
				string[] list = new string[count + 1];
				list[0] = count.ToString();
				count = 0;

				for (int b = 0; b < UIWidget.list.size; ++b)
				{
					UIWidget w = UIWidget.list[b];
					
					if (w.drawCall == dc)
					{
						list[++count] = count + ". " + NGUITools.GetHierarchy(w.cachedGameObject).Remove(0, initial);
					}
				}

				GUILayout.BeginHorizontal();
				int sel = EditorGUILayout.Popup("Widgets", 0, list);
				GUILayout.Space(18f);
				GUILayout.EndHorizontal();

				if (sel != 0)
				{
					count = 0;

					for (int b = 0; b < UIWidget.list.size; ++b)
					{
						UIWidget w = UIWidget.list[b];

						if (w.drawCall == dc && ++count == sel)
						{
							Selection.activeGameObject = w.gameObject;
							break;
						}
					}
				}

				EditorGUILayout.LabelField("Triangles", dc.triangles.ToString());

				if (clipping != UIDrawCall.Clipping.None && !dc.isClipped)
				{
					EditorGUILayout.HelpBox("You must switch this material's shader to Unlit/Transparent Colored or Unlit/Premultiplied Colored in order for clipping to work.",
						MessageType.Warning);
				}

				NGUIEditorTools.EndContents();
			}
		}
	}
}
