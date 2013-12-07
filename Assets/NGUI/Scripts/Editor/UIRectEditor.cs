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
		Padded,
		Relative,
		Unified,
		Advanced,
	}

	AnchorType mAnchorType = AnchorType.None;
	Transform mTarget0 = null;
	Transform mTarget1 = null;
	Transform mTarget2 = null;
	Transform mTarget3= null;

	/// <summary>
	/// Determine the initial anchor type.
	/// </summary>

	protected virtual void OnEnable ()
	{
		if (serializedObject.isEditingMultipleObjects)
		{
			mAnchorType = AnchorType.Advanced;
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
					mAnchorType = AnchorType.None;
				}
				else if (rect.leftAnchor.absolute == 0 &&
					rect.rightAnchor.absolute == 0 &&
					rect.bottomAnchor.absolute == 0 &&
					rect.topAnchor.absolute == 0)
				{
					mAnchorType = AnchorType.Relative;
				}
				else if (rect.leftAnchor.relative == 0f &&
					rect.rightAnchor.relative == 1f &&
					rect.bottomAnchor.relative == 0f &&
					rect.topAnchor.relative == 1f)
				{
					mAnchorType = AnchorType.Padded;
				}
				else
				{
					mAnchorType = AnchorType.Unified;
				}
			}
			else mAnchorType = AnchorType.Advanced;
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
			AnchorType type = (AnchorType)EditorGUILayout.EnumPopup("Type", mAnchorType);
			GUILayout.Space(18f);
			GUILayout.EndHorizontal();

			SerializedProperty t0 = serializedObject.FindProperty("leftAnchor.target");
			SerializedProperty t1 = serializedObject.FindProperty("rightAnchor.target");
			SerializedProperty t2 = serializedObject.FindProperty("bottomAnchor.target");
			SerializedProperty t3 = serializedObject.FindProperty("topAnchor.target");

			if (mAnchorType == AnchorType.None && type != AnchorType.None)
			{
				t0.objectReferenceValue = mTarget0;
				t1.objectReferenceValue = mTarget1;
				t2.objectReferenceValue = mTarget2;
				t3.objectReferenceValue = mTarget3;

				mTarget0 = null;
				mTarget1 = null;
				mTarget2 = null;
				mTarget3 = null;

				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
			}

			if (type == AnchorType.Advanced)
			{
				if (mAnchorType == AnchorType.None) UpdateAnchors(false, true);
				DrawAdvancedAnchors();
			}
			else if (type == AnchorType.Relative)
			{
				if (mAnchorType != type) UpdateAnchors(true, false);
				DrawRelativeAnchors();
			}
			else if (type == AnchorType.Padded)
			{
				if (mAnchorType != type) UpdateAnchors(false, false);
				DrawPaddedAnchors();
			}
			else if (type == AnchorType.Unified)
			{
				if (mAnchorType == AnchorType.None) UpdateAnchors(false, true);
				DrawUnifiedAnchors();
			}
			else if (type == AnchorType.None && mAnchorType != type)
			{
				// Save values to make it easy to "go back"
				mTarget0 = t0.objectReferenceValue as Transform;
				mTarget1 = t1.objectReferenceValue as Transform;
				mTarget2 = t2.objectReferenceValue as Transform;
				mTarget3 = t3.objectReferenceValue as Transform;

				t0.objectReferenceValue = null;
				t1.objectReferenceValue = null;
				t2.objectReferenceValue = null;
				t3.objectReferenceValue = null;

				serializedObject.FindProperty("leftAnchor.relative").floatValue = 0f;
				serializedObject.FindProperty("bottomAnchor.relative").floatValue = 0f;
				serializedObject.FindProperty("rightAnchor.relative").floatValue = 1f;
				serializedObject.FindProperty("topAnchor.relative").floatValue = 1f;
			}
			mAnchorType = type;
			NGUIEditorTools.EndContents();
		}
	}

	/// <summary>
	/// Draw anchors when in padded mode.
	/// </summary>

	void DrawPaddedAnchors ()
	{
		SerializedProperty sp = serializedObject.FindProperty("leftAnchor.target");
		Object before = sp.objectReferenceValue;

		GUILayout.Space(3f);
		NGUIEditorTools.DrawProperty("Target", sp, false);

		Object after = sp.objectReferenceValue;
		serializedObject.FindProperty("rightAnchor.target").objectReferenceValue = after;
		serializedObject.FindProperty("bottomAnchor.target").objectReferenceValue = after;
		serializedObject.FindProperty("topAnchor.target").objectReferenceValue = after;

		if (sp.objectReferenceValue != null || sp.hasMultipleDifferentValues)
			DrawSimpleAnchors(before == null && after != null);
	}

	/// <summary>
	/// Draw basic padded anchors.
	/// </summary>

	protected virtual void DrawSimpleAnchors (bool reset)
	{
		if (reset) UpdateAnchors(false, false);

		GUILayout.Space(3f);

		GUILayout.BeginHorizontal();
		NGUIEditorTools.DrawProperty("Left", serializedObject, "leftAnchor.absolute", GUILayout.Width(110f));
		GUILayout.Label("pixels");
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		NGUIEditorTools.DrawProperty("Right", serializedObject, "rightAnchor.absolute", GUILayout.Width(110f));
		GUILayout.Label("pixels");
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		NGUIEditorTools.DrawProperty("Bottom", serializedObject, "bottomAnchor.absolute", GUILayout.Width(110f));
		GUILayout.Label("pixels");
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		NGUIEditorTools.DrawProperty("Top", serializedObject, "topAnchor.absolute", GUILayout.Width(110f));
		GUILayout.Label("pixels");
		GUILayout.EndHorizontal();
	}

	/// <summary>
	/// Draw the anchors when using relative mode.
	/// </summary>

	void DrawRelativeAnchors ()
	{
		SerializedProperty sp = serializedObject.FindProperty("leftAnchor.target");
		Object before = sp.objectReferenceValue;

		GUILayout.Space(3f);
		NGUIEditorTools.DrawProperty("Target", sp, false);

		Object after = sp.objectReferenceValue;
		serializedObject.FindProperty("rightAnchor.target").objectReferenceValue = after;
		serializedObject.FindProperty("bottomAnchor.target").objectReferenceValue = after;
		serializedObject.FindProperty("topAnchor.target").objectReferenceValue = after;

		if (sp.objectReferenceValue != null || sp.hasMultipleDifferentValues)
		{
			if (IsRect(sp))
			{
				if (before == null && after != null) UpdateAnchors(true, false);
				DrawRelativeAnchor(sp, "Left", "leftAnchor", "width");
				DrawRelativeAnchor(sp, "Right", "rightAnchor", "width");
				DrawRelativeAnchor(sp, "Bottom", "bottomAnchor", "height");
				DrawRelativeAnchor(sp, "Top", "topAnchor", "height");
			}
			else DrawSimpleAnchors(before == null && after != null);
		}
	}

	/// <summary>
	/// Draw the anchor when in relative mode.
	/// </summary>

	void DrawRelativeAnchor (SerializedProperty sp, string prefix, string name, string suffix)
	{
		GUILayout.Space(3f);
		GUILayout.BeginHorizontal();
		NGUIEditorTools.DrawProperty(prefix, serializedObject, name + ".relative", GUILayout.Width(140f));
		GUILayout.Label("* target's " + suffix);
		GUILayout.EndHorizontal();
	}

	/// <summary>
	/// Draw the anchors when using relative mode.
	/// </summary>

	void DrawUnifiedAnchors ()
	{
		SerializedProperty sp = serializedObject.FindProperty("leftAnchor.target");
		Object before = sp.objectReferenceValue;
		
		GUILayout.Space(3f);
		NGUIEditorTools.DrawProperty("Target", sp, false);
		
		Object after = sp.objectReferenceValue;
		serializedObject.FindProperty("rightAnchor.target").objectReferenceValue = after;
		serializedObject.FindProperty("bottomAnchor.target").objectReferenceValue = after;
		serializedObject.FindProperty("topAnchor.target").objectReferenceValue = after;

		if (after != null || sp.hasMultipleDifferentValues)
		{
			if (IsRect(sp))
			{
				if (before == null && after != null) UpdateAnchors(false, true);
				DrawUnifiedAnchor(sp, "Left", "leftAnchor", "width");
				DrawUnifiedAnchor(sp, "Right", "rightAnchor", "width");
				DrawUnifiedAnchor(sp, "Bottom", "bottomAnchor", "height");
				DrawUnifiedAnchor(sp, "Top", "topAnchor", "height");
			}
			else DrawSimpleAnchors(before == null && after != null);
		}
	}

	/// <summary>
	/// Draw the anchor when in relative mode.
	/// </summary>

	void DrawUnifiedAnchor (SerializedProperty sp, string prefix, string name, string suffix)
	{
		GUILayout.Space(3f);

		NGUIEditorTools.SetLabelWidth(16f);

		GUILayout.BeginHorizontal();
		GUILayout.Label(prefix, GUILayout.Width(44f));
		GUILayout.Space(-2f);

		NGUIEditorTools.DrawProperty(" ", serializedObject, name + ".relative", GUILayout.Width(80f));
		GUILayout.Label("* target's " + suffix);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(50f);

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

	void UpdateAnchors (bool relative, bool chooseClosest)
	{
		serializedObject.ApplyModifiedProperties();

		Object[] objs = serializedObject.targetObjects;

		for (int i = 0; i < objs.Length; ++i)
		{
			UIRect rect = objs[i] as UIRect;

			if (rect)
			{
				UpdateHorizontalAnchor(rect, rect.leftAnchor, relative, chooseClosest);
				UpdateHorizontalAnchor(rect, rect.rightAnchor, relative, chooseClosest);
				UpdateVerticalAnchor(rect, rect.bottomAnchor, relative, chooseClosest);
				UpdateVerticalAnchor(rect, rect.topAnchor, relative, chooseClosest);
				UnityEditor.EditorUtility.SetDirty(rect);
			}
		}

		serializedObject.Update();
	}

	/// <summary>
	/// Convenience function that switches the anchor mode and ensures that dimensions are kept intact.
	/// </summary>

	static void UpdateHorizontalAnchor (UIRect r, UIRect.AnchorPoint anchor, bool relative, bool chooseClosest)
	{
		// Update the target
		if (anchor.target == null) return;

		// Update the rect
		anchor.rect = anchor.target.GetComponent<UIRect>();

		// Continue only if we have a parent to work with
		Transform parent = r.cachedTransform.parent;
		if (parent == null) return;

		bool inverted = (anchor == r.rightAnchor);
		int i0 = inverted ? 2 : 0;
		int i1 = inverted ? 3 : 1;

		// Calculate the left side
		Vector3[] myCorners = r.worldCorners;
		Vector3 localPos = parent.InverseTransformPoint(Vector3.Lerp(myCorners[i0], myCorners[i1], 0.5f));

		if (anchor.rect == null)
		{
			// Anchored to a simple transform
			Vector3 remotePos = parent.InverseTransformPoint(anchor.target.position);
			anchor.absolute = Mathf.FloorToInt(localPos.x - remotePos.x + 0.5f);
			anchor.relative = inverted ? 1f : 0f;
		}
		else
		{
			// Anchored to a rectangle -- must anchor to the same side
			Vector3[] targetCorners = anchor.rect.worldCorners;

			if (relative)
			{
				Vector3 remotePos = parent.InverseTransformPoint(Vector3.Lerp(targetCorners[i0], targetCorners[i1], 0.5f));
				float offset = localPos.x - remotePos.x;
				targetCorners = anchor.rect.localCorners;
				float remoteSize = targetCorners[3].x - targetCorners[0].x;

				anchor.absolute = 0;
				anchor.relative = offset / remoteSize;
				if (inverted) anchor.relative += 1f;
			}
			else
			{
				// We want to choose the side with the shortest offset
				Vector3 remotePos0 = parent.InverseTransformPoint(Vector3.Lerp(targetCorners[0], targetCorners[1], 0.5f));
				Vector3 remotePos1 = parent.InverseTransformPoint(Vector3.Lerp(targetCorners[2], targetCorners[3], 0.5f));

				float offset0 = localPos.x - remotePos0.x;
				float offset1 = localPos.x - remotePos1.x;

				if (chooseClosest && Mathf.Abs(offset0) > Mathf.Abs(offset1) || !chooseClosest && inverted)
				{
					anchor.relative = 1f;
					anchor.absolute = Mathf.FloorToInt(offset1 + 0.5f);
				}
				else
				{
					anchor.relative = 0f;
					anchor.absolute = Mathf.FloorToInt(offset0 + 0.5f);
				}
			}
		}
	}

	/// <summary>
	/// Convenience function that switches the anchor mode and ensures that dimensions are kept intact.
	/// </summary>

	static void UpdateVerticalAnchor (UIRect r, UIRect.AnchorPoint anchor, bool relative, bool chooseClosest)
	{
		// Update the target
		if (anchor.target == null) return;

		// Update the rect
		anchor.rect = anchor.target.GetComponent<UIRect>();

		// Continue only if we have a parent to work with
		Transform parent = r.cachedTransform.parent;
		if (parent == null) return;

		bool inverted = (anchor == r.topAnchor);
		int i0 = inverted ? 1 : 0;
		int i1 = inverted ? 2 : 3;

		// Calculate the bottom side
		Vector3[] myCorners = r.worldCorners;
		Vector3 localPos = parent.InverseTransformPoint(Vector3.Lerp(myCorners[i0], myCorners[i1], 0.5f));

		if (anchor.rect == null)
		{
			// Anchored to a simple transform
			Vector3 remotePos = parent.InverseTransformPoint(anchor.target.position);
			anchor.absolute = Mathf.FloorToInt(localPos.y - remotePos.y + 0.5f);
			anchor.relative = inverted ? 1f : 0f;
		}
		else
		{
			// Anchored to a rectangle -- must anchor to the same side
			Vector3[] targetCorners = anchor.rect.worldCorners;

			if (relative)
			{
				Vector3 remotePos = parent.InverseTransformPoint(Vector3.Lerp(targetCorners[i0], targetCorners[i1], 0.5f));
				float offset = localPos.y - remotePos.y;
				targetCorners = anchor.rect.localCorners;
				float remoteSize = targetCorners[1].y - targetCorners[0].y;

				anchor.absolute = 0;
				anchor.relative = offset / remoteSize;
				if (inverted) anchor.relative += 1f;
			}
			else
			{
				// We want to choose the side with the shortest offset
				Vector3 remotePos0 = parent.InverseTransformPoint(Vector3.Lerp(targetCorners[0], targetCorners[3], 0.5f));
				Vector3 remotePos1 = parent.InverseTransformPoint(Vector3.Lerp(targetCorners[1], targetCorners[2], 0.5f));

				float offset0 = localPos.y - remotePos0.y;
				float offset1 = localPos.y - remotePos1.y;

				if (chooseClosest && Mathf.Abs(offset0) > Mathf.Abs(offset1) || !chooseClosest && inverted)
				{
					anchor.relative = 1f;
					anchor.absolute = Mathf.FloorToInt(offset1 + 0.5f);
				}
				else
				{
					anchor.relative = 0f;
					anchor.absolute = Mathf.FloorToInt(offset0 + 0.5f);
				}
			}
		}
	}
}
