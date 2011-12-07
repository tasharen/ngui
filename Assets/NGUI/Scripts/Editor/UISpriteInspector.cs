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
	UIAtlas.Sprite mAtlasSprite;

	/// <summary>
	/// Atlas selection callback.
	/// </summary>

	void OnSelectAtlas (MonoBehaviour obj)
	{
		mSprite.atlas = obj as UIAtlas;

		if (mSprite != null)
		{
			Undo.RegisterUndo(mSprite, "Atlas Selection");
			mSprite.atlas = mSprite.atlas;
			EditorUtility.SetDirty(mSprite.gameObject);
		}
	}

	/// <summary>
	/// Draw the atlas and sprite selection fields.
	/// </summary>

	override protected bool OnCustomStart ()
	{
		mSprite = mWidget as UISprite;
		UIAtlas atlas = ComponentSelector.Draw<UIAtlas>(mSprite.atlas, OnSelectAtlas);
		if (mSprite.atlas != atlas) OnSelectAtlas(atlas);

		mAtlasSprite = null;
		if (mSprite.atlas == null) return false;

		List<string> sprites = mSprite.atlas.GetListOfSprites();

		if (sprites != null && sprites.Count > 0)
		{
			int index = 0;
			string spriteName = (mSprite.spriteName != null) ? mSprite.spriteName : sprites[0];

			// We need to find the sprite in order to have it selected
			if (!string.IsNullOrEmpty(spriteName))
			{
				for (int i = 0; i < sprites.Count; ++i)
				{
					if (string.Equals(sprites[i], spriteName, System.StringComparison.OrdinalIgnoreCase))
					{
						index = i;
						break;
					}
				}
			}

			// Draw the sprite selection popup
			index = EditorGUILayout.Popup("Sprite", index, sprites.ToArray());
			mAtlasSprite = mSprite.atlas.GetSprite(sprites[index]);
			GUITools.DrawSeparator();
		}
		return true;
	}

	/// <summary>
	/// Any and all derived functionality.
	/// </summary>

	override protected void OnCustomEnd ()
	{
		Texture2D tex = mSprite.mainTexture;

		if (tex != null)
		{
			// Draw the atlas
			EditorGUILayout.Separator();
			Rect rect = GUITools.DrawSprite(tex, mSprite.outerUV, mUseShader ? mSprite.atlas.material : null);

			// Draw the selection
			GUITools.DrawOutline(rect, mSprite.outerUV, new Color(0.4f, 1f, 0f, 1f));

			// Sprite size label
			string text = "Sprite Size: ";
			text += Mathf.RoundToInt(Mathf.Abs(mSprite.outerUV.width * tex.width));
			text += "x";
			text += Mathf.RoundToInt(Mathf.Abs(mSprite.outerUV.height * tex.height));

			rect = GUILayoutUtility.GetRect(Screen.width, 18f);
			EditorGUI.DropShadowLabel(rect, text);
		}
	}

	/// <summary>
	/// Save the atlas and sprites.
	/// </summary>

	override protected void OnCustomSave ()
	{
		mSprite.atlas = mSprite.atlas;
		mSprite.spriteName = (mAtlasSprite != null) ? mAtlasSprite.name : "";
	}
}