//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

#if !UNITY_3_5 && !UNITY_FLASH
#define DYNAMIC_FONT
#endif

using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UILabels.
/// </summary>

[CanEditMultipleObjects]
[CustomEditor(typeof(UILabel))]
public class UILabelInspector : UIWidgetInspector
{
	enum FontType
	{
		Bitmap,
		Dynamic,
	}

	UILabel mLabel;
	FontType mType;

	protected override void OnEnable ()
	{
		base.OnEnable();
		SerializedProperty bit = serializedObject.FindProperty("mFont");
		mType = (bit != null && bit.objectReferenceValue != null) ? FontType.Bitmap : FontType.Dynamic;
	}

	void OnBitmapFont (Object obj)
	{
		serializedObject.Update();
		SerializedProperty sp = serializedObject.FindProperty("mFont");
		sp.objectReferenceValue = obj;
		serializedObject.ApplyModifiedProperties();
	}

	void OnDynamicFont (Object obj)
	{
		serializedObject.Update();
		SerializedProperty sp = serializedObject.FindProperty("mTrueTypeFont");
		sp.objectReferenceValue = obj;
		serializedObject.ApplyModifiedProperties();
	}

	protected override bool DrawProperties ()
	{
		mLabel = mWidget as UILabel;

		GUILayout.BeginHorizontal();
		
		if (NGUIEditorTools.DrawPrefixButton("Font"))
		{
			if (mType == FontType.Bitmap)
			{
				ComponentSelector.Show<UIFont>(OnBitmapFont);
			}
			else
			{
				ComponentSelector.Show<Font>(OnDynamicFont);
			}
		}

#if DYNAMIC_FONT
		mType = (FontType)EditorGUILayout.EnumPopup(mType, GUILayout.Width(62f));
#else
		mType = FontType.Bitmap;
#endif
		bool isValid = false;
		SerializedProperty fnt = null;
		SerializedProperty ttf = null;

		if (mType == FontType.Bitmap)
		{
			fnt = NGUIEditorTools.DrawProperty("", serializedObject, "mFont", GUILayout.MinWidth(40f));
			if (fnt.objectReferenceValue != null) isValid = true;
		}
		else
		{
			ttf = NGUIEditorTools.DrawProperty("", serializedObject, "mTrueTypeFont", GUILayout.MinWidth(40f));
			if (ttf.objectReferenceValue != null) isValid = true;
		}

		GUILayout.EndHorizontal();

		EditorGUI.BeginDisabledGroup(!isValid);
		{
			if (ttf != null && ttf.objectReferenceValue != null)
			{
				GUILayout.BeginHorizontal();
				{
					EditorGUI.BeginDisabledGroup(ttf.hasMultipleDifferentValues);
					NGUIEditorTools.DrawProperty("Font Size", serializedObject, "mFontSize", GUILayout.Width(142f));
					NGUIEditorTools.DrawProperty("", serializedObject, "mFontStyle", GUILayout.MinWidth(40f));
					EditorGUI.EndDisabledGroup();
				}
				GUILayout.EndHorizontal();
			}

			GUI.skin.textField.wordWrap = true;
			GUILayout.Space(-16f);
			GUILayout.BeginHorizontal();
			GUILayout.Space(4f);
			NGUIEditorTools.DrawProperty("", serializedObject, "mText", GUILayout.Height(80f));
			GUILayout.Space(4f);
			GUILayout.EndHorizontal();
			GUI.skin.textField.wordWrap = false;

			NGUIEditorTools.DrawProperty("Overflow", serializedObject, "mOverflow");

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Encoding", serializedObject, "mEncoding", GUILayout.Width(100f));
			GUILayout.Label("use emoticons and colors");
			GUILayout.EndHorizontal();

			if (mLabel.supportEncoding && mLabel.font != null && mLabel.font.hasSymbols)
				NGUIEditorTools.DrawProperty("Symbols", serializedObject, "mSymbols");

			GUILayout.BeginHorizontal();
			SerializedProperty sp = NGUIEditorTools.DrawProperty("Effect", serializedObject, "mEffectStyle", GUILayout.Width(170f));
			if (sp.hasMultipleDifferentValues || sp.boolValue)
				NGUIEditorTools.DrawProperty("", serializedObject, "mEffectColor", GUILayout.MinWidth(40f));
			GUILayout.EndHorizontal();

			if (sp.hasMultipleDifferentValues || sp.boolValue)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Distance", GUILayout.Width(56f));
				NGUIEditorTools.SetLabelWidth(20f);
				NGUIEditorTools.DrawProperty("X", serializedObject, "mEffectDistance.x", GUILayout.MinWidth(40f));
				NGUIEditorTools.DrawProperty("Y", serializedObject, "mEffectDistance.y", GUILayout.MinWidth(40f));
				GUILayout.Space(18f);
				NGUIEditorTools.SetLabelWidth(80f);
				GUILayout.EndHorizontal();
			}

			NGUIEditorTools.DrawProperty("Max Lines", serializedObject, "mMaxLineCount", GUILayout.Width(110f));
		}
		EditorGUI.EndDisabledGroup();
		return isValid;
	}
}
