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
	enum AnchorType
	{
		None,
		Padding,
		Relative,
		Advanced,
	}

	AnchorType mType = AnchorType.None;

	/// <summary>
	/// Determine the initial anchor type.
	/// </summary>

	protected virtual void OnEnable ()
	{
		if (serializedObject.isEditingMultipleObjects)
		{
			mType = AnchorType.Advanced;
		}
		else
		{
			UIRect rect = target as UIRect;

			if (rect.leftAnchor.target == rect.rightAnchor.target &&
				rect.leftAnchor.target == rect.bottomAnchor.target &&
				rect.leftAnchor.target == rect.topAnchor.target)
			{
				if (rect.leftAnchor.target == null)
				{
					mType = AnchorType.None;
				}
				else if (rect.leftAnchor.relative == 0f &&
						rect.rightAnchor.relative == 1f &&
						rect.bottomAnchor.relative == 0f &&
						rect.topAnchor.relative == 1f)
				{
					mType = AnchorType.Padding;
				}
				else mType = AnchorType.Relative;
			}
			else mType = AnchorType.Advanced;
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
			NGUIEditorTools.SetLabelWidth(62f);

			GUILayout.BeginHorizontal();
			AnchorType type = (AnchorType)EditorGUILayout.EnumPopup("Type", mType);
			GUILayout.Space(18f);
			GUILayout.EndHorizontal();

			if (type == AnchorType.Advanced)
			{
				DrawAdvancedAnchors();
			}
			else if (type == AnchorType.Relative)
			{
				DrawRelativeAnchors();
			}
			else if (type == AnchorType.Padding)
			{
				if (mType != type)
				{
					serializedObject.FindProperty("leftAnchor.relative").floatValue = 0f;
					serializedObject.FindProperty("bottomAnchor.relative").floatValue = 0f;
					serializedObject.FindProperty("rightAnchor.relative").floatValue = 1f;
					serializedObject.FindProperty("topAnchor.relative").floatValue = 1f;
				}
				DrawPaddedAnchors();
			}
			else if (mType != type)
			{
				serializedObject.FindProperty("leftAnchor.target").objectReferenceValue = null;
				serializedObject.FindProperty("rightAnchor.target").objectReferenceValue = null;
				serializedObject.FindProperty("bottomAnchor.target").objectReferenceValue = null;
				serializedObject.FindProperty("topAnchor.target").objectReferenceValue = null;

				serializedObject.FindProperty("leftAnchor.relative").floatValue = 0f;
				serializedObject.FindProperty("bottomAnchor.relative").floatValue = 0f;
				serializedObject.FindProperty("rightAnchor.relative").floatValue = 1f;
				serializedObject.FindProperty("topAnchor.relative").floatValue = 1f;
			}
			mType = type;
			NGUIEditorTools.EndContents();
		}
	}

	void DrawPaddedAnchors ()
	{
		GUILayout.Space(3f);
		SerializedProperty sp = serializedObject.FindProperty("leftAnchor.target");
		NGUIEditorTools.DrawProperty("Target", sp, GUILayout.MinWidth(30f));
		Object after = sp.objectReferenceValue;
		serializedObject.FindProperty("rightAnchor.target").objectReferenceValue = after;
		serializedObject.FindProperty("bottomAnchor.target").objectReferenceValue = after;
		serializedObject.FindProperty("topAnchor.target").objectReferenceValue = after;

		if (sp.objectReferenceValue != null || sp.hasMultipleDifferentValues)
		{
			GUILayout.Space(3f);

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Left", serializedObject, "leftAnchor.absolute", GUILayout.Width(100f));
			GUILayout.Label("pixels");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Right", serializedObject, "rightAnchor.absolute", GUILayout.Width(100f));
			GUILayout.Label("pixels");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Bottom", serializedObject, "bottomAnchor.absolute", GUILayout.Width(100f));
			GUILayout.Label("pixels");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Top", serializedObject, "topAnchor.absolute", GUILayout.Width(100f));
			GUILayout.Label("pixels");
			GUILayout.EndHorizontal();
		}
	}

	void DrawRelativeAnchors ()
	{
		GUILayout.Space(3f);
		SerializedProperty sp = NGUIEditorTools.DrawProperty("Target", serializedObject, "leftAnchor.target", GUILayout.MinWidth(30f));

		Object obj = sp.objectReferenceValue;
		serializedObject.FindProperty("rightAnchor.target").objectReferenceValue = obj;
		serializedObject.FindProperty("bottomAnchor.target").objectReferenceValue = obj;
		serializedObject.FindProperty("topAnchor.target").objectReferenceValue = obj;

		if (sp.objectReferenceValue != null || sp.hasMultipleDifferentValues)
		{
			DrawRelativeAnchor(sp, "Left", "leftAnchor", "width");
			DrawRelativeAnchor(sp, "Right", "rightAnchor", "width");
			DrawRelativeAnchor(sp, "Bottom", "bottomAnchor", "height");
			DrawRelativeAnchor(sp, "Top", "topAnchor", "height");
		}
	}

	void DrawRelativeAnchor (SerializedProperty sp, string prefix, string name, string suffix)
	{
		GUILayout.Space(3f);

		NGUIEditorTools.SetLabelWidth(16f);

		GUILayout.BeginHorizontal();
		GUILayout.Label(prefix, GUILayout.Width(44f));
		GUILayout.Space(-2f);

		if (IsRect(sp))
		{
			NGUIEditorTools.DrawProperty("+", serializedObject, name + ".relative", GUILayout.Width(56f));
			GUILayout.Label("* target's " + suffix);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Space(50f);
		}

		NGUIEditorTools.DrawProperty("+", serializedObject, name + ".absolute", GUILayout.Width(56f));
		GUILayout.Label("pixels");
		GUILayout.EndHorizontal();

		NGUIEditorTools.SetLabelWidth(62f);
	}

	void DrawAdvancedAnchors ()
	{
		DrawAdvancedAnchor("Left", "leftAnchor", "width");
		DrawAdvancedAnchor("Right", "rightAnchor", "width");
		DrawAdvancedAnchor("Bottom", "bottomAnchor", "height");
		DrawAdvancedAnchor("Top", "topAnchor", "height");
	}

	void DrawAdvancedAnchor (string prefix, string name, string suffix)
	{
		GUILayout.Space(3f);
		SerializedProperty sp = NGUIEditorTools.DrawProperty(prefix, serializedObject, name + ".target");

		if (sp.objectReferenceValue != null || sp.hasMultipleDifferentValues)
		{
			NGUIEditorTools.SetLabelWidth(16f);

			if (IsRect(sp))
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(50f);
				NGUIEditorTools.DrawProperty("+", serializedObject, name + ".relative", GUILayout.Width(56f));
				GUILayout.Label("* target's " + suffix);
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			GUILayout.Space(50f);
			NGUIEditorTools.DrawProperty("+", serializedObject, name + ".absolute", GUILayout.Width(56f));
			GUILayout.Label("pixels");
			GUILayout.EndHorizontal();

			NGUIEditorTools.SetLabelWidth(62f);
		}
	}

	bool IsRect (SerializedProperty sp)
	{
		if (sp.hasMultipleDifferentValues) return true;
		Transform target = sp.objectReferenceValue as Transform;
		if (target == null) return false;
		UIRect rect = target.GetComponent<UIRect>();
		return (rect != null);
	}
}
