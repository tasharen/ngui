//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor class used to view UIRects.
/// </summary>

[CanEditMultipleObjects]
[CustomEditor(typeof(UIRect))]
public class UIRectEditor : Editor
{
	protected virtual bool ShouldDrawProperties () { return true; }
	protected virtual void DrawCustomProperties () { }

	/// <summary>
	/// Draw the "Anchors" property block.
	/// </summary>

	protected virtual void DrawFinalProperties ()
	{
		if (NGUIEditorTools.DrawHeader("Anchors"))
		{
			NGUIEditorTools.BeginContents();
#if UNITY_3_5
			string horizontal = "<>";
#else
			string horizontal = "\u25C4\u25BA";
#endif
			string vertical = "\u25B2\u25BC";
			float space = 38f;
			NGUIEditorTools.SetLabelWidth(62f);

			SerializedProperty sp = NGUIEditorTools.DrawProperty("Left", serializedObject, "leftAnchor.target");

			if (sp.objectReferenceValue != null || sp.hasMultipleDifferentValues)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(space);
				GUILayout.Label(horizontal, GUILayout.Width(24f));
				NGUIEditorTools.DrawProperty("", serializedObject, "leftAnchor.relative", GUILayout.MinWidth(30f));
				GUILayout.Label("+", GUILayout.Width(12f));
				NGUIEditorTools.DrawProperty("", serializedObject, "leftAnchor.absolute", GUILayout.MinWidth(30f));
				GUILayout.Space(18f);
				GUILayout.EndHorizontal();
			}

			sp = NGUIEditorTools.DrawProperty("Right", serializedObject, "rightAnchor.target");

			if (sp.objectReferenceValue != null || sp.hasMultipleDifferentValues)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(space);
				GUILayout.Label(horizontal, GUILayout.Width(24f));
				NGUIEditorTools.DrawProperty("", serializedObject, "rightAnchor.relative", GUILayout.MinWidth(30f));
				GUILayout.Label("+", GUILayout.Width(12f));
				NGUIEditorTools.DrawProperty("", serializedObject, "rightAnchor.absolute", GUILayout.MinWidth(30f));
				GUILayout.Space(18f);
				GUILayout.EndHorizontal();
			}

			sp = NGUIEditorTools.DrawProperty("Bottom", serializedObject, "bottomAnchor.target");

			if (sp.objectReferenceValue != null || sp.hasMultipleDifferentValues)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(space);
				GUILayout.Label(vertical, GUILayout.Width(24f));
				NGUIEditorTools.DrawProperty("", serializedObject, "bottomAnchor.relative", GUILayout.MinWidth(30f));
				GUILayout.Label("+", GUILayout.Width(12f));
				NGUIEditorTools.DrawProperty("", serializedObject, "bottomAnchor.absolute", GUILayout.MinWidth(30f));
				GUILayout.Space(18f);
				GUILayout.EndHorizontal();
			}

			sp = NGUIEditorTools.DrawProperty("Top", serializedObject, "topAnchor.target");

			if (sp.objectReferenceValue != null || sp.hasMultipleDifferentValues)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(space);
				GUILayout.Label(vertical, GUILayout.Width(24f));
				NGUIEditorTools.DrawProperty("", serializedObject, "topAnchor.relative", GUILayout.MinWidth(30f));
				GUILayout.Label("+", GUILayout.Width(12f));
				NGUIEditorTools.DrawProperty("", serializedObject, "topAnchor.absolute", GUILayout.MinWidth(30f));
				GUILayout.Space(18f);
				GUILayout.EndHorizontal();
			}

			NGUIEditorTools.EndContents();
		}
	}

	/// <summary>
	/// Draw the inspector properties.
	/// </summary>

	public override void OnInspectorGUI ()
	{
		NGUIEditorTools.SetLabelWidth(80f);
		EditorGUILayout.Space();

		serializedObject.Update();

		EditorGUI.BeginDisabledGroup(!ShouldDrawProperties());
		DrawCustomProperties();
		EditorGUI.EndDisabledGroup();
		DrawFinalProperties();

		serializedObject.ApplyModifiedProperties();
	}
}
