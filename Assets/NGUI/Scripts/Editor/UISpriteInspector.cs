using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UISprites.
/// </summary>

[CustomEditor(typeof(UISprite))]
public class UISpriteInspector : UIWidgetInspector
{
	UIAtlas mAtlas;
	UISprite mSprite;
	UIAtlas.Sprite mAtlasSprite;

	/// <summary>
	/// Draw the atlas and sprite selection fields.
	/// </summary>

	override protected bool OnCustomStart ()
	{
		mSprite = mWidget as UISprite;
		mAtlas = EditorGUILayout.ObjectField("Atlas", mSprite.atlas, typeof(UIAtlas), true) as UIAtlas;

		mAtlasSprite = null;
		if (mAtlas == null) return false;

		string[] sprites = mAtlas.GetListOfSprites();

		if (sprites != null && sprites.Length > 0)
		{
			int index = 0;
			string spriteName = (mSprite.spriteName != null) ? mSprite.spriteName : sprites[0];

			// We need to find the sprite in order to have it selected
			if (!string.IsNullOrEmpty(spriteName))
			{
				for (int i = 0; i < sprites.Length; ++i)
				{
					if (string.Equals(sprites[i], spriteName, System.StringComparison.OrdinalIgnoreCase))
					{
						index = i;
						break;
					}
				}
			}

			// Draw the sprite selection popup
			index = EditorGUILayout.Popup("Sprite", index, sprites);
			mAtlasSprite = mAtlas.GetSprite(sprites[index]);
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
			Rect rect = GUITools.DrawSprite(tex, mSprite.outerUV);

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
		mSprite.atlas = mAtlas;
		mSprite.spriteName = (mAtlasSprite != null) ? mAtlasSprite.name : "";
	}
}