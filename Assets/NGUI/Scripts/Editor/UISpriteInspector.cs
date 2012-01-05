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
			Undo.RegisterUndo(mSprite, "Atlas Selection");
			mSprite.atlas = obj as UIAtlas;
			EditorUtility.SetDirty(mSprite.gameObject);
		}
	}

	/// <summary>
	/// Convenience function that displays a list of sprites and returns the selected value.
	/// </summary>

	static public string SpriteField (UIAtlas atlas, string name)
	{
		List<string> sprites = atlas.GetListOfSprites();

		if (sprites != null && sprites.Count > 0)
		{
			int index = 0;
			if (string.IsNullOrEmpty(name)) name = sprites[0];

			// We need to find the sprite in order to have it selected
			if (!string.IsNullOrEmpty(name))
			{
				for (int i = 0; i < sprites.Count; ++i)
				{
					if (name.Equals(sprites[i], System.StringComparison.OrdinalIgnoreCase))
					{
						index = i;
						break;
					}
				}
			}

			// Draw the sprite selection popup
			index = EditorGUILayout.Popup("Sprite", index, sprites.ToArray());
			return atlas.GetSprite(sprites[index]).name;
		}
		return null;
	}

	/// <summary>
	/// Draw the atlas and sprite selection fields.
	/// </summary>

	override protected bool OnDrawProperties ()
	{
		mSprite = mWidget as UISprite;
		UIAtlas atlas = ComponentSelector.Draw<UIAtlas>(mSprite.atlas, OnSelectAtlas);
		if (mSprite.atlas != atlas) OnSelectAtlas(atlas);
		if (mSprite.atlas == null) return false;

		string spriteName = SpriteField(atlas, mSprite.spriteName);

		if (mSprite.spriteName != spriteName)
		{
			Undo.RegisterUndo(mSprite, "Sprite Change");
			mSprite.spriteName = spriteName;
			EditorUtility.SetDirty(mSprite.gameObject);
		}
		return true;
	}

	/// <summary>
	/// Draw the sprite texture.
	/// </summary>

	override protected void OnDrawTexture ()
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
}