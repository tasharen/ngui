using UnityEngine;

/// <summary>
/// Helper class using Unity's GUI functionality. Meant to be used by inspector tools.
/// </summary>

static public class GUITools
{
	static Texture2D mWhiteTex;
	static Texture2D mCheckerTex;

	/// <summary>
	/// Returns a blank usable 1x1 white texture.
	/// </summary>

	static public Texture2D blankTexture
	{
		get
		{
			if (mWhiteTex == null) mWhiteTex = CreateDummyTex();
			return mWhiteTex;
		}
	}

	/// <summary>
	/// Returns a usable texture that looks like a checker board.
	/// </summary>

	static public Texture2D checkerTexture
	{
		get
		{
			if (mCheckerTex == null) mCheckerTex = CreateCheckerTex();
			return mCheckerTex;
		}
	}

	/// <summary>
	/// Create a white dummy texture.
	/// </summary>

	static Texture2D CreateDummyTex ()
	{
		Texture2D tex = new Texture2D(1, 1);
		tex.name = "[Generated] Dummy Texture";
		tex.SetPixel(0, 0, Color.white);
		tex.Apply();
		tex.filterMode = FilterMode.Point;
		return tex;
	}

	/// <summary>
	/// Create a checker-background texture
	/// </summary>

	static Texture2D CreateCheckerTex ()
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
	/// Draw a single-pixel outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Color color)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Texture2D blank = blankTexture;
			GUI.color = color;
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), blank);
			GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), blank);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), blank);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), blank);
			GUI.color = Color.white;
		}
	}

	/// <summary>
	/// Draw a selection outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect relative, Color color)
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
			DrawOutline(relative, color);
		}
	}

	/// <summary>
	/// Draw a checkered background for the specified texture.
	/// </summary>

	static Rect DrawBackground (Texture2D tex, float ratio)
	{
		Rect rect = GUILayoutUtility.GetRect(0f, 0f);
		rect.width = Screen.width;
		rect.height = Screen.width * ratio;
		GUILayout.Space(rect.height);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D blank = blankTexture;
			Texture2D check = checkerTexture;

			// Lines above and below the texture rectangle
			GUI.color = new Color(0f, 0f, 0f, 0.2f);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin - 1, rect.width, 1f), blank);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), blank);
			GUI.color = Color.white;

			// Checker background
			DrawTiledTexture(rect, check);
		}
		return rect;
	}

	/// <summary>
	/// Draw a texture atlas, complete with a background texture and an outline.
	/// </summary>

	static public Rect DrawAtlas (Texture2D tex)
	{
		Rect rect = DrawBackground(tex, (float)tex.height / tex.width);

		if (Event.current.type == EventType.Repaint)
		{
			GUI.DrawTexture(rect, tex);
		}
		return rect;
	}

	/// <summary>
	/// Draw an enlarged sprite within the specified texture atlas.
	/// </summary>

	static public Rect DrawSprite (Texture2D tex, Rect sprite)
	{
		float paddingX = 4f / tex.width;
		float paddingY = 4f / tex.height;
		float ratio = (sprite.height + paddingY) / (sprite.width + paddingX);

		// Draw the checkered background
		Rect rect = DrawBackground(tex, ratio);

		// We only want to draw into this rectangle
		GUI.BeginGroup(rect);
		{
			if (Event.current.type == EventType.Repaint)
			{
				// We need to calculate where to begin and how to stretch the texture
				// for it to appear properly scaled in the rectangle
				float scaleX = rect.width  / (sprite.width  + paddingX);
				float scaleY = rect.height / (sprite.height + paddingY);
				float ox = scaleX * (sprite.x - paddingX * 0.5f);
				float oy = scaleY * (1f - (sprite.yMax + paddingY * 0.5f));

				Rect drawRect = new Rect(-ox, -oy, scaleX, scaleY);
				GUI.DrawTexture(drawRect, tex);
				rect = new Rect(drawRect.x + rect.x, drawRect.y + rect.y, drawRect.width, drawRect.height);
			}
		}
		GUI.EndGroup();
		return rect;
	}

	/// <summary>
	/// Draw a visible separator in addition to adding some padding.
	/// </summary>

	static public void DrawSeparator()
	{
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
	}
}