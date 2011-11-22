using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UISprites.
/// </summary>

[CustomEditor(typeof(UISprite))]
public class UISpriteInspector : UIWidgetInspector
{
	/// <summary>
	/// Any and all derived functionality.
	/// </summary>

	protected override void OnCustomGUI ()
	{
		UISprite sprite = mWidget as UISprite;
		Texture2D tex = sprite.mainTexture;

		if (tex != null)
		{
			// Draw the atlas
			Rect rect = GUITools.DrawAtlas(tex);

			// Draw the selection
			GUITools.DrawOutline(rect, sprite.outerUV, new Color(0.4f, 1f, 0f, 1f));

			// Sprite size label
			string text = "Sprite Size: ";
			text += Mathf.RoundToInt(Mathf.Abs(sprite.outerUV.width * tex.width));
			text += "x";
			text += Mathf.RoundToInt(Mathf.Abs(sprite.outerUV.height * tex.height));
			EditorGUI.DropShadowLabel(new Rect(rect.xMin, rect.yMax, rect.width, 18f), text);
			GUILayout.Space(22f);
		}
	}
}