//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2018 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector class used to edit UISprites.
/// </summary>

[CanEditMultipleObjects]
[CustomEditor(typeof(UISprite), true)]
public class UISpriteInspector : UIBasicSpriteEditor
{
	/// <summary>
	/// Atlas selection callback.
	/// </summary>

	void OnSelectAtlas (Object obj)
	{
		serializedObject.Update();

		var oldAtlas = serializedObject.FindProperty("mAtlas");
		if (oldAtlas != null) oldAtlas.objectReferenceValue = obj;

		serializedObject.ApplyModifiedProperties();
		NGUITools.SetDirty(serializedObject.targetObject);
		NGUISettings.atlas = obj as INGUIAtlas;
	}

	/// <summary>
	/// Sprite selection callback function.
	/// </summary>

	void SelectSprite (string spriteName)
	{
		serializedObject.Update();
		SerializedProperty sp = serializedObject.FindProperty("mSpriteName");
		sp.stringValue = spriteName;
		serializedObject.ApplyModifiedProperties();
		NGUITools.SetDirty(serializedObject.targetObject);
		NGUISettings.selectedSprite = spriteName;
	}

	/// <summary>
	/// Draw the atlas and sprite selection fields.
	/// </summary>

	protected override bool ShouldDrawProperties ()
	{
		var atlasProp = serializedObject.FindProperty("mAtlas");
		var atlas = atlasProp.objectReferenceValue as INGUIAtlas;

		GUILayout.BeginHorizontal();

		if (NGUIEditorTools.DrawPrefixButton("Atlas")) ComponentSelector.Show(atlas, OnSelectAtlas);

		atlasProp = NGUIEditorTools.DrawProperty("", serializedObject, "mAtlas", GUILayout.MinWidth(20f));

		if (GUILayout.Button("Edit", GUILayout.Width(40f)) && atlas != null)
		{
			NGUISettings.atlas = atlas;
			NGUIEditorTools.Select(atlas as Object);
		}

		GUILayout.EndHorizontal();
		var sp = serializedObject.FindProperty("mSpriteName");
		NGUIEditorTools.DrawAdvancedSpriteField(atlas, sp.stringValue, SelectSprite, false);
		NGUIEditorTools.DrawProperty("Material", serializedObject, "mMat");
		return true;
	}

	/// <summary>
	/// All widgets have a preview.
	/// </summary>

	public override bool HasPreviewGUI ()
	{
		return (Selection.activeGameObject == null || Selection.gameObjects.Length == 1);
	}

	/// <summary>
	/// Draw the sprite preview.
	/// </summary>

	public override void OnPreviewGUI (Rect rect, GUIStyle background)
	{
		UISprite sprite = target as UISprite;
		if (sprite == null || !sprite.isValid) return;

		Texture2D tex = sprite.mainTexture as Texture2D;
		if (tex == null) return;

		UISpriteData sd = sprite.GetSprite(sprite.spriteName);
		NGUIEditorTools.DrawSprite(tex, rect, sd, sprite.color);
	}
}
