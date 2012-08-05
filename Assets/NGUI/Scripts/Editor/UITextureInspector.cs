//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UITextures.
/// </summary>

[CustomEditor(typeof(UITexture))]
public class UITextureInspector : UIWidgetInspector
{
	UITexture mTex;

	override protected bool OnDrawProperties ()
	{
		mTex = mWidget as UITexture;

		if (!mTex.hasDynamicMaterial && (mTex.material != null || mTex.mainTexture == null))
		{
			Material mat = EditorGUILayout.ObjectField("Material", mTex.material, typeof(Material), false) as Material;

			if (mTex.material != mat)
			{
				NGUIEditorTools.RegisterUndo("Material Selection", mTex);
				mTex.material = mat;
			}
		}

		if (mTex.material == null || mTex.hasDynamicMaterial)
		{
			Shader shader = EditorGUILayout.ObjectField("Shader", mTex.shader, typeof(Shader), false) as Shader;

			if (mTex.shader != shader)
			{
				NGUIEditorTools.RegisterUndo("Shader Selection", mTex);
				mTex.shader = shader;
			}

			Texture tex = EditorGUILayout.ObjectField("Texture", mTex.mainTexture, typeof(Texture), false) as Texture;

			if (mTex.mainTexture != tex)
			{
				NGUIEditorTools.RegisterUndo("Texture Selection", mTex);
				mTex.mainTexture = tex;
			}
		}

		if (mTex.mainTexture != null)
		{
			Rect rect = EditorGUILayout.RectField("UV Rectangle", mTex.uvRect);

			if (rect != mTex.uvRect)
			{
				NGUIEditorTools.RegisterUndo("UV Rectangle Change", mTex);
				mTex.uvRect = rect;
			}
		}
		return (mWidget.material != null);
	}

	override protected void OnDrawTexture ()
	{
		Texture2D tex = mWidget.mainTexture as Texture2D;

		if (tex != null)
		{
			// Draw the atlas
			EditorGUILayout.Separator();

			// Draw the texture and its UV outline
			bool isSmall = (mTex.uvRect.width < 0.25f && mTex.uvRect.height < 0.25f);
			Rect spriteRect = NGUIEditorTools.DrawSprite(tex, isSmall ?
				mTex.uvRect : new Rect(0f, 0f, 1f, 1f), null, isSmall);
			NGUIEditorTools.DrawOutline(spriteRect, mTex.uvRect, Color.green);

			// Sprite size label
			Rect rect = GUILayoutUtility.GetRect(Screen.width, 18f);
			EditorGUI.DropShadowLabel(rect, "Texture Size: " + tex.width + "x" + tex.height);
		}
	}
}