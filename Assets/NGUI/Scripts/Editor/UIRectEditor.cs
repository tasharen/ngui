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
				if (mType != type) UpdateAnchors(true);
				DrawRelativeAnchors();
			}
			else if (type == AnchorType.Padding)
			{
				if (mType != type) UpdateAnchors(false);
				DrawPaddedAnchors();
			}
			else if (mType != type)
			{
				// No anchors needed -- clear the references
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

	/// <summary>
	/// Draw anchors when in padded mode.
	/// </summary>

	void DrawPaddedAnchors ()
	{
		GUILayout.Space(3f);
		
		SerializedProperty sp = serializedObject.FindProperty("leftAnchor.target");
		Object before = sp.objectReferenceValue;
		NGUIEditorTools.DrawProperty("Target", sp, false);
		Object after = sp.objectReferenceValue;
		serializedObject.FindProperty("rightAnchor.target").objectReferenceValue = after;
		serializedObject.FindProperty("bottomAnchor.target").objectReferenceValue = after;
		serializedObject.FindProperty("topAnchor.target").objectReferenceValue = after;

		if (sp.objectReferenceValue != null || sp.hasMultipleDifferentValues)
		{
			if (before == null && after != null) UpdateAnchors(false);

			GUILayout.Space(3f);

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Left", serializedObject, "leftAnchor.absolute", GUILayout.Width(124f));
			GUILayout.Label("pixels");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Right", serializedObject, "rightAnchor.absolute", GUILayout.Width(124f));
			GUILayout.Label("pixels");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Bottom", serializedObject, "bottomAnchor.absolute", GUILayout.Width(124f));
			GUILayout.Label("pixels");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Top", serializedObject, "topAnchor.absolute", GUILayout.Width(124f));
			GUILayout.Label("pixels");
			GUILayout.EndHorizontal();
		}
	}

	/// <summary>
	/// Draw the anchors when using relative mode.
	/// </summary>

	void DrawRelativeAnchors ()
	{
		GUILayout.Space(3f);
		
		SerializedProperty sp = serializedObject.FindProperty("leftAnchor.target");
		Object before = sp.objectReferenceValue;
		NGUIEditorTools.DrawProperty("Target", sp, false);
		Object after = sp.objectReferenceValue;
		serializedObject.FindProperty("rightAnchor.target").objectReferenceValue = after;
		serializedObject.FindProperty("bottomAnchor.target").objectReferenceValue = after;
		serializedObject.FindProperty("topAnchor.target").objectReferenceValue = after;

		if (sp.objectReferenceValue != null || sp.hasMultipleDifferentValues)
		{
			if (before == null && after != null) UpdateAnchors(true);

			DrawRelativeAnchor(sp, "Left", "leftAnchor", "width");
			DrawRelativeAnchor(sp, "Right", "rightAnchor", "width");
			DrawRelativeAnchor(sp, "Bottom", "bottomAnchor", "height");
			DrawRelativeAnchor(sp, "Top", "topAnchor", "height");
		}
	}

	/// <summary>
	/// Draw the anchor when in relative mode.
	/// </summary>

	void DrawRelativeAnchor (SerializedProperty sp, string prefix, string name, string suffix)
	{
		GUILayout.Space(3f);

		NGUIEditorTools.SetLabelWidth(16f);

		GUILayout.BeginHorizontal();
		GUILayout.Label(prefix, GUILayout.Width(44f));
		GUILayout.Space(-2f);

		if (IsRect(sp))
		{
			NGUIEditorTools.DrawProperty(" ", serializedObject, name + ".relative", GUILayout.Width(80f));
			GUILayout.Label("* target's " + suffix);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Space(50f);
		}

		NGUIEditorTools.DrawProperty("+", serializedObject, name + ".absolute", GUILayout.Width(80f));
		GUILayout.Label("pixels");
		GUILayout.EndHorizontal();

		NGUIEditorTools.SetLabelWidth(62f);
	}

	/// <summary>
	/// Draw anchors when in advanced mode.
	/// </summary>

	void DrawAdvancedAnchors ()
	{
		DrawAdvancedAnchor("Left", "leftAnchor", "width");
		DrawAdvancedAnchor("Right", "rightAnchor", "width");
		DrawAdvancedAnchor("Bottom", "bottomAnchor", "height");
		DrawAdvancedAnchor("Top", "topAnchor", "height");
	}

	/// <summary>
	/// Draw the specified advanced anchor.
	/// </summary>

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
				NGUIEditorTools.DrawProperty(" ", serializedObject, name + ".relative", GUILayout.Width(80f));
				GUILayout.Label("* target's " + suffix);
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			GUILayout.Space(50f);
			NGUIEditorTools.DrawProperty("+", serializedObject, name + ".absolute", GUILayout.Width(80f));
			GUILayout.Label("pixels");
			GUILayout.EndHorizontal();

			NGUIEditorTools.SetLabelWidth(62f);
		}
	}

	/// <summary>
	/// Returns 'true' if the specified serialized property reference is a UIRect.
	/// </summary>

	static bool IsRect (SerializedProperty sp)
	{
		if (sp.hasMultipleDifferentValues) return true;
		Transform target = sp.objectReferenceValue as Transform;
		if (target == null) return false;
		UIRect rect = target.GetComponent<UIRect>();
		return (rect != null);
	}

	/// <summary>
	/// Convenience function that switches the anchor mode and ensures that dimensions are kept intact.
	/// </summary>

	void UpdateAnchors (bool relative)
	{
		serializedObject.ApplyModifiedProperties();

		Object[] objs = serializedObject.targetObjects;

		for (int i = 0; i < objs.Length; ++i)
		{
			UIRect rect = objs[i] as UIRect;

			if (rect)
			{
				UpdateHorizontalAnchor(rect, rect.leftAnchor, relative);
				UpdateHorizontalAnchor(rect, rect.rightAnchor, relative);
				UpdateVerticalAnchor(rect, rect.bottomAnchor, relative);
				UpdateVerticalAnchor(rect, rect.topAnchor, relative);
				UnityEditor.EditorUtility.SetDirty(rect);
			}
		}

		serializedObject.Update();
	}

	/// <summary>
	/// Convenience function that switches the anchor mode and ensures that dimensions are kept intact.
	/// </summary>

	static void UpdateHorizontalAnchor (UIRect r, UIRect.AnchorPoint anchor, bool relative)
	{
		// Update the target
		if (anchor.target == null) return;

		// Update the rect
		anchor.rect = anchor.target.GetComponent<UIRect>();

		// Continue only if we have a parent to work with
		Transform parent = r.cachedTransform.parent;
		if (parent == null) return;

		bool right = (anchor == r.rightAnchor);
		int i0 = right ? 2 : 0;
		int i1 = right ? 3 : 1;

		// Calculate the left side
		Vector3[] myCorners = r.worldCorners;
		Vector3 localPos = parent.InverseTransformPoint(Vector3.Lerp(myCorners[i0], myCorners[i1], 0.5f));

		if (anchor.rect == null)
		{
			// Anchored to a simple transform
			Vector3 remotePos = parent.InverseTransformPoint(anchor.target.position);
			anchor.absolute = Mathf.RoundToInt(localPos.x - remotePos.x);
			anchor.relative = right ? 1f : 0f;
		}
		else
		{
			// Anchored to a rectangle -- must anchor to the same side
			Vector3[] targetCorners = anchor.rect.worldCorners;
			Vector3 remotePos = parent.InverseTransformPoint(Vector3.Lerp(targetCorners[i0], targetCorners[i1], 0.5f));

			if (relative)
			{
				targetCorners = anchor.rect.localCorners;
				float remoteSize = targetCorners[3].x - targetCorners[0].x;

				anchor.absolute = 0;
				anchor.relative = (localPos.x - remotePos.x) / remoteSize;
				if (right) anchor.relative += 1f;
			}
			else
			{
				anchor.relative = right ? 1f : 0f;
				anchor.absolute = Mathf.RoundToInt(localPos.x - remotePos.x);
			}
		}
	}

	/// <summary>
	/// Convenience function that switches the anchor mode and ensures that dimensions are kept intact.
	/// </summary>

	static void UpdateVerticalAnchor (UIRect r, UIRect.AnchorPoint anchor, bool relative)
	{
		// Update the target
		if (anchor.target == null) return;

		// Update the rect
		anchor.rect = anchor.target.GetComponent<UIRect>();

		// Continue only if we have a parent to work with
		Transform parent = r.cachedTransform.parent;
		if (parent == null) return;

		bool top = (anchor == r.topAnchor);
		int i0 = top ? 1 : 0;
		int i1 = top ? 2 : 3;

		// Calculate the bottom side
		Vector3[] myCorners = r.worldCorners;
		Vector3 localPos = parent.InverseTransformPoint(Vector3.Lerp(myCorners[i0], myCorners[i1], 0.5f));

		if (anchor.rect == null)
		{
			// Anchored to a simple transform
			Vector3 remotePos = parent.InverseTransformPoint(anchor.target.position);
			anchor.absolute = Mathf.RoundToInt(localPos.y - remotePos.y);
			anchor.relative = top ? 1f : 0f;
		}
		else
		{
			// Anchored to a rectangle -- must anchor to the same side
			Vector3[] targetCorners = anchor.rect.worldCorners;
			Vector3 remotePos = parent.InverseTransformPoint(Vector3.Lerp(targetCorners[i0], targetCorners[i1], 0.5f));

			if (relative)
			{
				targetCorners = anchor.rect.localCorners;
				float remoteSize = targetCorners[1].y - targetCorners[0].y;

				anchor.absolute = 0;
				anchor.relative = (localPos.y - remotePos.y) / remoteSize;
				if (top) anchor.relative += 1f;
			}
			else
			{
				anchor.relative = top ? 1f : 0f;
				anchor.absolute = Mathf.RoundToInt(localPos.y - remotePos.y);
			}
		}
	}
}
