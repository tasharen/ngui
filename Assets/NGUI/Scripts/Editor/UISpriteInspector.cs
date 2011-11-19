using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UISprites.
/// </summary>

[CustomEditor(typeof(UISprite))]
public class UISpriteInspector : UIWidgetInspector
{
	static public bool atlasTexture = true;

	Texture2D mCheckerTex;
	Texture2D mSelectionTex;

	/// <summary>
	/// Any and all derived functionality.
	/// </summary>

	protected override void OnCustomGUI ()
	{
		UISprite sprite = mWidget as UISprite;
		Texture2D tex = sprite.mainTexture;

		if (tex != null)
		{
			if (atlasTexture)
			{
				if (mCheckerTex == null) mCheckerTex = GUITools.CreateCheckerTex();
				if (mSelectionTex == null) mSelectionTex = GUITools.CreateDummyTex();

				Rect rect = GUILayoutUtility.GetRect(128f, 128f);
				rect.height = rect.width * (tex.height / tex.width);

				// Lines above and below the texture rectangle
				GUI.color = new Color(0f, 0f, 0f, 0.2f);
				GUI.DrawTexture(new Rect(rect.xMin, rect.yMin - 1, rect.width, 1f), mSelectionTex);
				GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), mSelectionTex);
				GUI.color = Color.white;

				// Checker background
				GUITools.DrawTiledTexture(rect, mCheckerTex);

				// Actual texture
				GUI.DrawTexture(rect, sprite.mainTexture);

				// Calculate where the outer rectangle would be
				Rect outer = sprite.outerUV;
				float x = rect.xMin + rect.width * outer.xMin;
				float y = rect.yMax - rect.height * outer.yMin;
				float width = rect.width * outer.width;
				float height = -rect.height * outer.height;
				outer = new Rect(x, y, width, height);

				// Calculate where the inner rectangle would be
				Rect inner = sprite.innerUV;
				x = rect.xMin + rect.width * inner.xMin;
				y = rect.yMax - rect.height * inner.yMin;
				width = rect.width * inner.width;
				height = -rect.height * inner.height;
				inner = new Rect(x, y, width, height);

				// Draw the inner and outer rectangles
				GUI.color = new Color(0.4f, 1f, 0f, 1f);
				GUITools.DrawOutline(outer, mSelectionTex);
				GUI.color = new Color(0f, 0.7f, 1f, 1f);
				GUITools.DrawOutline(inner, mSelectionTex);
				GUI.color = Color.white;
			}
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