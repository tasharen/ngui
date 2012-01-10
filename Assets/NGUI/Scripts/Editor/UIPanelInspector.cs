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

		bool norms = EditorGUILayout.Toggle("Normals", panel.generateNormals);

		if (panel.generateNormals != norms)
		{
			panel.generateNormals = norms;
			EditorUtility.SetDirty(panel);
		}

		bool gizmos = EditorGUILayout.Toggle("Gizmos", panel.showGizmos);

		if (panel.showGizmos != gizmos)
		{
			panel.showGizmos = gizmos;
			EditorUtility.SetDirty(panel);
		}

		panel.debug = EditorGUILayout.Toggle("Debug", panel.debug);
		EditorGUILayout.LabelField("Widgets", panel.widgets.Count.ToString());
		EditorGUILayout.LabelField("Draw Calls", drawcalls.Count.ToString());

		UIDrawCall.Clipping clipping = (UIDrawCall.Clipping)EditorGUILayout.EnumPopup("Clipping", panel.clipping);

		if (panel.clipping != clipping)
		{
			// Automatically switch shaders on the material to something that supports or disables clipping
			foreach (UIDrawCall dc in panel.drawCalls)
			{
				Material mat = dc.material;
				
				if (mat != null && mat.shader != null)
				{
					bool isClipped = mat.shader.name.Contains(" (Clipped)");

					if (isClipped && clipping == UIDrawCall.Clipping.None)
					{
						// Try to find the non-clipped version of this shader
						UIPanel[] panels = Resources.FindObjectsOfTypeAll(typeof(UIPanel)) as UIPanel[];

						// Only swap shaders back to non-clipped version if this is the only panel in the scene.
						// Reason being, another panel can still be clipped, and can still use the same atlas,
						// and we don't want to mess everything up for it.

						if (panels.Length == 1)
						{
							string name = mat.shader.name.Replace(" (Clipped)", "");
							Shader shader = Shader.Find(name);
							if (shader != null) mat.shader = shader;
						}
					}
					else if (!isClipped && clipping != UIDrawCall.Clipping.None)
					{
						// Try to find the clipped version of this shader
						Shader shader = Shader.Find(mat.shader.name += " (Clipped)");

						if (shader != null)
						{
							mat.shader = shader;
						}
						else if (!mat.HasProperty("_ClipRange"))
						{
							// If this warning gets triggered, it means you've tried to enable clipping with a shader
							// that has not been set up to use it. Consider switching your material to use the
							// "Unlit/Transparent Colored" shader or its clipped counterpart, "Unlit/Transparent Colored (Clipped)".
							Debug.LogWarning("Shader '" + mat.shader.name + "' used by material '" + mat.name + "' does not support clipping");
						}
					}
				}
			}

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
				Undo.RegisterUndo(panel, "Clipping Change");
				panel.clipRange = range;
				EditorUtility.SetDirty(panel);
			}

			if (panel.clipping == UIDrawCall.Clipping.SoftAlpha)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(80f);
				Vector2 soft = EditorGUILayout.Vector2Field("Softness", panel.clipSoftness);
				GUILayout.EndHorizontal();

				if (soft.x < 1f) soft.x = 1f;
				if (soft.y < 1f) soft.y = 1f;

				if (panel.clipSoftness != soft)
				{
					Undo.RegisterUndo(panel, "Clipping Change");
					panel.clipSoftness = soft;
					EditorUtility.SetDirty(panel);
				}
			}
		}

		foreach (UIDrawCall dc in drawcalls)
		{
			GUITools.DrawSeparator();
			EditorGUILayout.ObjectField("Material", dc.material, typeof(Material), false);
			EditorGUILayout.LabelField("Triangles", dc.triangles.ToString());
		}
	}
}