using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UISlicedSprites.
/// </summary>

[CustomEditor(typeof(UISlicedSprite))]
public class UISlicedSpriteInspector : UISpriteInspector
{
	/// <summary>
	/// Draw the selection outline.
	/// </summary>

	protected override void DrawSelection (UISprite sprite, Rect rect)
	{
		// Calculate where the inner rectangle would be
		Rect inner = sprite.innerUV;
		float x = rect.xMin + rect.width * inner.xMin;
		float y = rect.yMax - rect.height * inner.yMin;
		float width = rect.width * inner.width;
		float height = -rect.height * inner.height;
		inner = new Rect(x, y, width, height);

		// Draw the selection
		GUI.color = new Color(0f, 0.7f, 1f, 1f);
		GUITools.DrawOutline(inner, mSelectionTex);
		GUI.color = Color.white;

		// Draw the outer selection
		base.DrawSelection(sprite, rect);
	}
}