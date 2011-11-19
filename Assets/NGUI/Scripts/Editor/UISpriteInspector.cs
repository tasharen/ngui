using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UISprites.
/// </summary>

[CustomEditor(typeof(UISprite))]
public class UISpriteInspector : UIWidgetInspector
{
	protected Texture2D mCheckerTex;
	protected Texture2D mSelectionTex;

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

	/// <summary>
	/// Release the checker-box texture.
	/// </summary>

	void OnDestroy ()
	{
		if (mSelectionTex != null)
		{
			if (Application.isPlaying) Destroy(mSelectionTex);
			else DestroyImmediate(mSelectionTex);
			mSelectionTex = null;
		}

		if (mCheckerTex != null)
		{
			if (Application.isPlaying) Destroy(mCheckerTex);
			else DestroyImmediate(mCheckerTex);
			mCheckerTex = null;
		}
	}
}