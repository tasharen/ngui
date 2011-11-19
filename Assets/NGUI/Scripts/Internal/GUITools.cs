using UnityEngine;

/// <summary>
/// Helper class using Unity's GUI functionality. Meant to be used by inspector tools.
/// </summary>

static public class GUITools
{
	/// <summary>
	/// Draws the tiled texture. Like GUI.DrawTexture() but tiled instead of stretched.
	/// </summary>

	static public void DrawTiledTexture (Rect rect, Texture tex)
	{
		GUI.BeginGroup(rect);
		{
			int width  = Mathf.RoundToInt(rect.width);
			int height = Mathf.RoundToInt(rect.height);

			for (int y = 0; y < height; y += tex.height)
			{
				for (int x = 0; x < width; x += tex.width)
				{
					GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
				}
			}
		}
		GUI.EndGroup();
	}

	/// <summary>
	/// Create a checker-background texture
	/// </summary>

	static public Texture2D CreateCheckerTex ()
	{
		Texture2D tex = new Texture2D(16, 16);
		tex.name = "[Generated] Checker Texture";

		Color c0 = new Color(0f, 0f, 0f, 0.2f);
		Color c1 = new Color(0.25f, 0.25f, 0.25f, 0.2f);

		for (int y = 0; y < 8;  ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c1);
		for (int y = 8; y < 16; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c0);
		for (int y = 0; y < 8;  ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
		for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

		tex.Apply();
		tex.filterMode = FilterMode.Point;
		return tex;
	}

	/// <summary>
	/// Create a white dummy texture.
	/// </summary>

	static public Texture2D CreateDummyTex ()
	{
		Texture2D tex = new Texture2D(1, 1);
		tex.name = "[Generated] Dummy Texture";
		tex.SetPixel(0, 0, Color.white);
		tex.Apply();
		tex.filterMode = FilterMode.Point;
		return tex;
	}

	/// <summary>
	/// Draw a single-pixel outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Texture2D tex)
	{
		GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
		GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
		GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
		GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
	}
}