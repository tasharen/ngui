using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UISlicedSprites.
/// </summary>

[CustomEditor(typeof(UISlicedSprite))]
public class UISlicedSpriteInspector : UISpriteInspector
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
			if (mCheckerTex == null) mCheckerTex = GUITools.CreateCheckerTex();
			if (mSelectionTex == null) mSelectionTex = GUITools.CreateDummyTex();

			// Draw the atlas
			Rect rect = GUITools.DrawAtlas(tex, mSelectionTex, mCheckerTex);

			// Draw the selection
			GUI.color = new Color(0f, 0.7f, 1f, 1f);
			GUITools.DrawOutline(rect, sprite.innerUV, mSelectionTex);
			GUI.color = new Color(0.4f, 1f, 0f, 1f);
			GUITools.DrawOutline(rect, sprite.outerUV, mSelectionTex);
			GUI.color = Color.white;

			// Sprite size label
			string text = "Sprite Size: ";
			text += Mathf.RoundToInt(Mathf.Abs(sprite.outerUV.width * tex.width));
			text += "x";
			text += Mathf.RoundToInt(Mathf.Abs(sprite.outerUV.height * tex.height));
			EditorGUI.DropShadowLabel(new Rect(rect.xMin, rect.yMax, rect.width, 18f), text);
		}
	}
}