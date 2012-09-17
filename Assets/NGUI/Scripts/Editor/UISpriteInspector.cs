//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UISprites.
/// </summary>

[CustomEditor(typeof(UISprite))]
public class UISpriteInspector : UIWidgetInspector
{
	protected UISprite mSprite;

	/// <summary>
	/// Atlas selection callback.
	/// </summary>

	void OnSelectAtlas (MonoBehaviour obj)
	{
		if (mSprite != null)
		{
			NGUIEditorTools.RegisterUndo("Atlas Selection", mSprite);
			bool resize = (mSprite.atlas == null);
			mSprite.atlas = obj as UIAtlas;
			if (resize) mSprite.MakePixelPerfect();
			EditorUtility.SetDirty(mSprite.gameObject);
		}
	}

	/// <summary>
	/// Sprite selection callback function.
	/// </summary>

	void SelectSprite (string spriteName)
	{
		if (mSprite != null && mSprite.spriteName != spriteName)
		{
			NGUIEditorTools.RegisterUndo("Sprite Change", mSprite);
			mSprite.spriteName = spriteName;
			mSprite.MakePixelPerfect();
			EditorUtility.SetDirty(mSprite.gameObject);
		}
	}

	/// <summary>
	/// Draw the atlas and sprite selection fields.
	/// </summary>

	override protected bool OnDrawProperties ()
	{
		mSprite = mWidget as UISprite;
		ComponentSelector.Draw<UIAtlas>(mSprite.atlas, OnSelectAtlas);
		if (mSprite.atlas == null) return false;
		NGUIEditorTools.AdvancedSpriteField(mSprite.atlas, mSprite.spriteName, SelectSprite, false);
		return true;
	}

	/// <summary>
	/// All widgets have a preview.
	/// </summary>

	public override bool HasPreviewGUI () { return true; }

	/// <summary>
	/// Draw the sprite preview.
	/// </summary>

	public override void OnPreviewGUI (Rect rect, GUIStyle background)
	{
		if (mSprite == null) return;

		Texture2D tex = mSprite.mainTexture as Texture2D;
		if (tex == null) return;

		Rect outer = new Rect(mSprite.sprite.outer);
		Rect inner = new Rect(mSprite.sprite.inner);
		Rect uv = outer;

		if (mSprite.atlas.coordinates == UIAtlas.Coordinates.Pixels)
		{
			uv = NGUIMath.ConvertToTexCoords(outer, tex.width, tex.height);
		}
		else
		{
			outer = NGUIMath.ConvertToPixels(outer, tex.width, tex.height, true);
			inner = NGUIMath.ConvertToPixels(inner, tex.width, tex.height, true);
		}
		NGUIEditorTools.DrawSprite(tex, rect, outer, inner, uv, mSprite.color);
	}
}