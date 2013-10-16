//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

#if !UNITY_3_5 && !UNITY_FLASH
#define DYNAMIC_FONT
#endif

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UIPopupLists.
/// </summary>

[CustomEditor(typeof(UIPopupList))]
public class UIPopupListInspector : UIWidgetContainerEditor
{
	enum FontType
	{
		Bitmap,
		Dynamic,
	}

	UIPopupList mList;
	FontType mType;

	void OnEnable ()
	{
		SerializedProperty bit = serializedObject.FindProperty("font");
		mType = (bit != null && bit.objectReferenceValue != null) ? FontType.Bitmap : FontType.Dynamic;
	}

	void RegisterUndo ()
	{
		NGUIEditorTools.RegisterUndo("Popup List Change", mList);
	}

	void OnSelectAtlas (Object obj)
	{
		RegisterUndo();
		mList.atlas = obj as UIAtlas;
	}

	void OnBackground (string spriteName)
	{
		RegisterUndo();
		mList.backgroundSprite = spriteName;
		Repaint();
	}

	void OnHighlight (string spriteName)
	{
		RegisterUndo();
		mList.highlightSprite = spriteName;
		Repaint();
	}

	void OnBitmapFont (Object obj)
	{
		serializedObject.Update();
		SerializedProperty sp = serializedObject.FindProperty("font");
		sp.objectReferenceValue = obj;
		serializedObject.ApplyModifiedProperties();
	}

	void OnDynamicFont (Object obj)
	{
		serializedObject.Update();
		SerializedProperty sp = serializedObject.FindProperty("trueTypeFont");
		sp.objectReferenceValue = obj;
		serializedObject.ApplyModifiedProperties();
	}

	public override void OnInspectorGUI ()
	{
		serializedObject.Update();
		NGUIEditorTools.SetLabelWidth(80f);
		mList = target as UIPopupList;

		UILabel lbl = EditorGUILayout.ObjectField("Text Label", mList.textLabel, typeof(UILabel), true) as UILabel;

		if (mList.textLabel != lbl)
		{
			RegisterUndo();
			mList.textLabel = lbl;
			if (lbl != null) lbl.text = mList.value;
		}

		if (mList.textLabel == null)
		{
			EditorGUILayout.HelpBox("This popup list has no label to update, so it will behave like a menu.", MessageType.Info);
		}

		if (mList.atlas != null)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(6f);
			GUILayout.Label("Options");
			GUILayout.EndHorizontal();

			string text = "";
			foreach (string s in mList.items) text += s + "\n";

			GUILayout.Space(-14f);
			GUILayout.BeginHorizontal();
			GUILayout.Space(84f);
			string modified = EditorGUILayout.TextArea(text, GUILayout.Height(100f));
			GUILayout.EndHorizontal();

			if (modified != text)
			{
				RegisterUndo();
				string[] split = modified.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
				mList.items.Clear();
				foreach (string s in split) mList.items.Add(s);

				if (string.IsNullOrEmpty(mList.value) || !mList.items.Contains(mList.value))
				{
					mList.value = mList.items.Count > 0 ? mList.items[0] : "";
				}
			}

			GUI.changed = false;
			string sel = NGUIEditorTools.DrawList("Default", mList.items.ToArray(), mList.value);
			if (GUI.changed) serializedObject.FindProperty("mSelectedItem").stringValue = sel;

			NGUIEditorTools.DrawProperty("Position", serializedObject, "position");
			NGUIEditorTools.DrawProperty("Localized", serializedObject, "isLocalized");

			DrawAtlas();
			DrawFont();

			NGUIEditorTools.DrawEvents("On Value Change", mList, mList.onChange);
		}
		serializedObject.ApplyModifiedProperties();
	}

	void DrawAtlas()
	{
		if (NGUIEditorTools.DrawHeader("Atlas"))
		{
			NGUIEditorTools.BeginContents();

			GUILayout.BeginHorizontal();
			{
				if (NGUIEditorTools.DrawPrefixButton("Atlas"))
					ComponentSelector.Show<UIAtlas>(OnSelectAtlas);
				NGUIEditorTools.DrawProperty("", serializedObject, "atlas");
			}
			GUILayout.EndHorizontal();

			NGUIEditorTools.DrawPaddedSpriteField("Background", mList.atlas, mList.backgroundSprite, OnBackground);
			NGUIEditorTools.DrawPaddedSpriteField("Highlight", mList.atlas, mList.highlightSprite, OnHighlight);

			EditorGUILayout.Space();

			NGUIEditorTools.DrawProperty("Background", serializedObject, "backgroundColor");
			NGUIEditorTools.DrawProperty("Highlight", serializedObject, "highlightColor");
			NGUIEditorTools.DrawProperty("Animated", serializedObject, "isAnimated");
			NGUIEditorTools.EndContents();
		}
	}

	void DrawFont ()
	{
		if (NGUIEditorTools.DrawHeader("Font"))
		{
			NGUIEditorTools.BeginContents();

			SerializedProperty ttf = null;

			GUILayout.BeginHorizontal();
			{
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
				GUI.changed = false;
				mType = (FontType)EditorGUILayout.EnumPopup(mType, GUILayout.Width(62f));

				if (GUI.changed)
				{
					GUI.changed = false;

					if (mType == FontType.Bitmap)
					{
						serializedObject.FindProperty("trueTypeFont").objectReferenceValue = null;
					}
					else
					{
						serializedObject.FindProperty("font").objectReferenceValue = null;
					}
				}
#else
				mType = FontType.Bitmap;
#endif

				if (mType == FontType.Bitmap)
				{
					NGUIEditorTools.DrawProperty("", serializedObject, "font", GUILayout.MinWidth(40f));
				}
				else
				{
					ttf = NGUIEditorTools.DrawProperty("", serializedObject, "trueTypeFont", GUILayout.MinWidth(40f));
				}
			}
			GUILayout.EndHorizontal();

			if (ttf != null && ttf.objectReferenceValue != null)
			{
				GUILayout.BeginHorizontal();
				{
					EditorGUI.BeginDisabledGroup(ttf.hasMultipleDifferentValues);
					NGUIEditorTools.DrawProperty("Font Size", serializedObject, "fontSize", GUILayout.Width(142f));
					NGUIEditorTools.DrawProperty("", serializedObject, "fontStyle", GUILayout.MinWidth(40f));
					GUILayout.Space(18f);
					EditorGUI.EndDisabledGroup();
				}
				GUILayout.EndHorizontal();
			}

			NGUIEditorTools.DrawProperty("Text Color", serializedObject, "textColor");

			GUILayout.BeginHorizontal();
			NGUIEditorTools.SetLabelWidth(66f);
			EditorGUILayout.PrefixLabel("Padding");
			NGUIEditorTools.SetLabelWidth(14f);
			NGUIEditorTools.DrawProperty("X", serializedObject, "padding.x", GUILayout.MinWidth(30f));
			NGUIEditorTools.DrawProperty("Y", serializedObject, "padding.y", GUILayout.MinWidth(30f));
			GUILayout.Space(18f);
			NGUIEditorTools.SetLabelWidth(80f);
			GUILayout.EndHorizontal();

			NGUIEditorTools.EndContents();
		}
	}
}
