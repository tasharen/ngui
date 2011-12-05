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

	protected override void OnCustomEnd ()
	{
		UISlicedSprite sprite = mWidget as UISlicedSprite;
		Texture2D tex = sprite.mainTexture;

		if (tex != null)
		{
			// Draw the atlas
			EditorGUILayout.Separator();
			Rect rect = GUITools.DrawSprite(tex, sprite.outerUV, mUseShader ? mAtlas.material : null);

			// Draw the selection
			GUITools.DrawOutline(rect, sprite.innerUV, new Color(0f, 0.7f, 1f, 1f));
			GUITools.DrawOutline(rect, sprite.outerUV, new Color(0.4f, 1f, 0f, 1f));

			// Sprite size label
			string text = "Sprite Size: ";
			text += Mathf.RoundToInt(Mathf.Abs(sprite.outerUV.width * tex.width));
			text += "x";
			text += Mathf.RoundToInt(Mathf.Abs(sprite.outerUV.height * tex.height));

			rect = GUILayoutUtility.GetRect(Screen.width, 18f);
			EditorGUI.DropShadowLabel(rect, text);
		}
	}
}