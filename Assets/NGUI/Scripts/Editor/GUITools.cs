using UnityEngine;

/// <summary>
/// Helper class using Unity's GUI functionality. Meant to be used by inspector tools.
/// </summary>

static public class GUITools
{
	// Not a fan of the darker hues? Change this to 'Color.white'.
	static public Color backgroundColor = Color.white;//new Color(0.65f, 0.65f, 0.65f, 1f);

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

		Color c0 = new Color(0.1f, 0.1f, 0.1f, 0.5f);
		Color c1 = new Color(0.2f, 0.2f, 0.2f, 0.5f);

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
		if (Event.current.type == EventType.Repaint)
		{
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
		}
	}

	/// <summary>
	/// Draw a selection outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect relative, Texture2D tex)
	{
		if (Event.current.type == EventType.Repaint)
		{
			// Calculate where the outer rectangle would be
			float x = rect.xMin + rect.width * relative.xMin;
			float y = rect.yMax - rect.height * relative.yMin;
			float width = rect.width * relative.width;
			float height = -rect.height * relative.height;
			relative = new Rect(x, y, width, height);

			// Draw the selection
			DrawOutline(relative, tex);
		}
	}

	/// <summary>
	/// Draw a texture atlas, complete with a background texture and an outline.
	/// </summary>

	static public Rect DrawAtlas (Texture2D atlasTex, Texture2D selectionTex, Texture2D backgroundTex)
	{
		Rect rect = GUILayoutUtility.GetRect(0f, 0f);
		rect.width = Screen.width;
		rect.height = rect.width * (atlasTex.height / atlasTex.width);
		GUILayout.Space(Screen.width);

		if (Event.current.type == EventType.Repaint)
		{
			// Lines above and below the texture rectangle
			GUI.color = new Color(0f, 0f, 0f, 0.2f);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin - 1, rect.width, 1f), selectionTex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), selectionTex);
			GUI.color = Color.white;

			// Checker background
			DrawTiledTexture(rect, backgroundTex);

			// Actual texture
			GUI.DrawTexture(rect, atlasTex);
		}
		return rect;
	}

	/// <summary>
	/// Draw a visible separator in addition to adding some padding.
	/// </summary>

	static public void DrawSeparator (Texture2D tex)
	{
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint)
		{
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
	}
}