using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UIFont contains everything needed to be able to print text.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Font")]
public class UIFont : MonoBehaviour
{
#if UNITY_FLASH // Unity 3.5b6 is bugged when SerializeField is mixed with prefabs (after LoadLevel)
	public Material mMat;
	public Rect mUVRect = new Rect(0f, 0f, 1f, 1f);
	public BMFont mFont = new BMFont();
	public int mSpacingX = 0;
	public int mSpacingY = 0;
	public UIAtlas mAtlas;
	public string mSpriteName = "";
#else
	[SerializeField] Material mMat;
	[SerializeField] Rect mUVRect = new Rect(0f, 0f, 1f, 1f);
	[SerializeField] BMFont mFont = new BMFont();
	[SerializeField] int mSpacingX = 0;
	[SerializeField] int mSpacingY = 0;
	[SerializeField] UIAtlas mAtlas;
	[SerializeField] string mSpriteName = "";
#endif

	// Cached value
	UIAtlas.Sprite mSprite = null;

	// BUG: There is a bug in Unity 3.4.2 and all the way up to 3.5 b7 -- when instantiating from prefabs,
	// for some strange reason classes get initialized with default values. So for example, 'mSprite' above
	// gets initialized as if it was created with 'new UIAtlas.Sprite()' instead of 'null'. Fun, huh?

	bool mSpriteSet = false;

	// I'd use a Stack here, but then Flash export wouldn't work as it doesn't support it
	List<Color> mColors = new List<Color>();

	/// <summary>
	/// Access to the BMFont class directly.
	/// </summary>

	public BMFont bmFont { get { return mFont; } }

	/// <summary>
	/// Original width of the font's texture in pixels.
	/// </summary>

	public int texWidth { get { return (mFont != null) ? mFont.texWidth : 1; } }

	/// <summary>
	/// Original height of the font's texture in pixels.
	/// </summary>

	public int texHeight { get { return (mFont != null) ? mFont.texHeight : 1; } }

	/// <summary>
	/// Atlas used by the font, if any.
	/// </summary>

	public UIAtlas atlas
	{
		get
		{
			return mAtlas;
		}
		set
		{
			if (mAtlas != value)
			{
				if (value == null)
				{
					if (mAtlas != null) mMat = mAtlas.material;
					if (mSprite != null) mUVRect = uvRect;
				}

				mSprite = null;
				mAtlas = value;
				Refresh();
			}
		}
	}

	/// <summary>
	/// Get or set the material used by this font.
	/// </summary>

	public Material material
	{
		get
		{
			return (mAtlas != null) ? mAtlas.material : mMat;
		}
		set
		{
			if (mAtlas == null && mMat != value)
			{
				mMat = value;
				Refresh();
			}
		}
	}

	/// <summary>
	/// Convenience function that returns the texture used by the font.
	/// </summary>

	public Texture2D texture
	{
		get
		{
			Material mat = material;
			return (mat != null) ? mat.mainTexture as Texture2D : null;
		}
	}

	/// <summary>
	/// Offset and scale applied to all UV coordinates.
	/// </summary>

	public Rect uvRect
	{
		get
		{
			if (mAtlas != null && sprite != null)
			{
				Texture2D tex = mAtlas.texture;

				if (tex != null)
				{
					mUVRect = mSprite.outer;

					if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
					{
						mUVRect = NGUIMath.ConvertToTexCoords(mUVRect, tex.width, tex.height);
					}

					// Account for trimmed sprites
					if (mSprite.hasPadding)
					{
						Rect rect = mUVRect;
						mUVRect.xMin = rect.xMin - mSprite.paddingLeft * rect.width;
						mUVRect.xMax = rect.xMax + mSprite.paddingRight * rect.width;
						mUVRect.yMin = rect.yMin - mSprite.paddingBottom * rect.height;
						mUVRect.yMax = rect.yMax + mSprite.paddingTop * rect.height;
					}
				}
			}
			return mUVRect;
		}
		set
		{
			if (sprite == null && mUVRect != value)
			{
				mUVRect = value;
				Refresh();
			}
		}
	}

	/// <summary>
	/// Sprite used by the font, if any.
	/// </summary>

	public string spriteName { get { return mSpriteName; } set { if (mSpriteName != value) { mSpriteName = value; mSprite = null; Refresh(); } } }

	/// <summary>
	/// Horizontal spacing applies to characters. If positive, it will add extra spacing between characters. If negative, it will make them be closer together.
	/// </summary>

	public int horizontalSpacing { get { return mSpacingX; } set { if (mSpacingX != value) { mSpacingX = value; Refresh(); } } }

	/// <summary>
	/// Vertical spacing applies to lines. If positive, it will add extra spacing between lines. If negative, it will make them be closer together.
	/// </summary>

	public int verticalSpacing { get { return mSpacingY; } set { if (mSpacingY != value) { mSpacingY = value; Refresh(); } } }

	/// <summary>
	/// Pixel-perfect size of this font.
	/// </summary>

	public int size { get { return mFont.charSize; } }

	/// <summary>
	/// Retrieves the sprite used by the font, if any.
	/// </summary>

	public UIAtlas.Sprite sprite
	{
		get
		{
			if (!mSpriteSet) mSprite = null;

			if (mSprite == null && mAtlas != null && !string.IsNullOrEmpty(mSpriteName))
			{
				mSprite = mAtlas.GetSprite(mSpriteName);
				mSpriteSet = true;

				if (mSprite == null)
				{
					Debug.LogError("Can't find the sprite '" + mSpriteName + "' in UIAtlas on " + NGUITools.GetHierarchy(mAtlas.gameObject));
					mSpriteName = null;
				}
			}
			return mSprite;
		}
	}

	/// <summary>
	/// Refresh all labels that use this font.
	/// </summary>

	public void Refresh ()
	{
		UILabel[] labels = (UILabel[])Object.FindSceneObjectsOfType(typeof(UILabel));

		foreach (UILabel lbl in labels)
		{
			if (lbl.font == this)
			{
				lbl.font = null;
				lbl.font = this;
			}
		}
	}

	/// <summary>
	/// Text wrapping functionality.
	/// </summary>

	public string WrapText (string text, float maxWidth, bool multiline, bool encoding)
	{
		string newText = "";
		float widthOfSpace = CalculatePrintedSize(" ", false).x;
		bool addNewLine = false;

		// Break the text into lines
		string[] lines = text.Split(new char[] { '\n' });

		// Run through each line
		foreach (string line in lines)
		{
			if (addNewLine) newText += "\n";

			// Break lines into words
			string[] words = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
			float spaceLeft = maxWidth;

			foreach (string word in words)
			{
				float width = CalculatePrintedSize(word, encoding).x;

				// If this is not a brand-new line, we'll need to append a space as well
				if (spaceLeft != maxWidth) spaceLeft -= widthOfSpace;

				if (width < spaceLeft)
				{
					// Append the word
					if (spaceLeft != maxWidth) newText += " ";
					newText += word;
					spaceLeft -= width;
				}
				else
				{
					// If multi-line is not supported, we're done
					if (!multiline) return newText;

					// Insert line break before word.
					newText += "\n" + word;

					// Reset space left on line
					spaceLeft = maxWidth - width;
				}
			}
			addNewLine = true;
			if (!multiline) break;
		}
		return newText;
	}

	/// <summary>
	/// Get the printed size of the specified string.
	/// </summary>

	public Vector2 CalculatePrintedSize (string text, bool encoding)
	{
		Vector2 v = Vector2.zero;

		if (mFont != null && mFont.isValid && !string.IsNullOrEmpty(text))
		{
			if (encoding) text = NGUITools.StripSymbols(text);

			Vector2 scale = mFont.charSize > 0 ? new Vector2(1f / mFont.charSize, 1f / mFont.charSize) : Vector2.one;

			int maxX = 0;
			int x = 0;
			int y = 0;
			int prev = 0;
			int lineHeight = (mFont.charSize + mSpacingY);

			for (int i = 0, imax = text.Length; i < imax; ++i)
			{
				char c = text[i];

				if (c == '\n' || (encoding && (c == '\\') && (i + 1 < imax) && (text[i + 1] == 'n')))
				{
					if (x > maxX) maxX = x;
					x = 0;
					y += lineHeight;
					prev = 0;
					if (c != '\n') ++i;
					continue;
				}

				if (c < ' ')
				{
					prev = 0;
					continue;
				}

				BMGlyph glyph = mFont.GetGlyph(c);

				if (glyph != null)
				{
					x += mSpacingX + ((prev != 0) ? glyph.advance + glyph.GetKerning(prev) : glyph.advance);
					prev = c;
				}
			}

			if (x > maxX) maxX = x;
			v.x = scale.x * maxX;
			v.y = scale.y * (y + lineHeight);
		}
		return v;
	}

	/// <summary>
	/// Print the specified text into the buffers.
	/// </summary>

	public void Print (string text, Color color, List<Vector3> verts, List<Vector2> uvs, List<Color> cols, bool encoding)
	{
		if (mFont != null && text != null)
		{
			if (!mFont.isValid)
			{
				Debug.LogError("Attempting to print using an invalid font!");
				return;
			}

			mColors.Clear();
			mColors.Add(color);

			Vector2 scale = mFont.charSize > 0 ? new Vector2(1f / mFont.charSize, 1f / mFont.charSize) : Vector2.one;

			int maxX = 0;
			int x = 0;
			int y = 0;
			int prev = 0;
			int lineHeight = (mFont.charSize + mSpacingY);
			Vector3 v0 = Vector3.zero, v1 = Vector3.zero;
			Vector2 u0 = Vector2.zero, u1 = Vector2.zero;
			float invX = uvRect.width / mFont.texWidth;
			float invY = mUVRect.height / mFont.texHeight;

			for (int i = 0, imax = text.Length; i < imax; ++i)
			{
				char c = text[i];

				if (c == '\n' || (encoding && (c == '\\') && (i + 1 < imax) && (text[i + 1] == 'n')))
				{
					if (x > maxX) maxX = x;
					x = 0;
					y += lineHeight;
					prev = 0;
					if (c != '\n') ++i;
					continue;
				}

				if (c < ' ')
				{
					prev = 0;
					continue;
				}

				if (encoding && c == '[')
				{
					int retVal = NGUITools.ParseSymbol(text, i, mColors);

					if (retVal > 0)
					{
						color = mColors[mColors.Count - 1];
						i += retVal - 1;
						continue;
					}
				}

				BMGlyph glyph = mFont.GetGlyph(c);

				if (glyph != null)
				{
					if (prev != 0) x += glyph.GetKerning(prev);

					v0.x = scale.x * (x + glyph.offsetX);
					v0.y = -scale.y * (y + glyph.offsetY);

					v1.x = v0.x + scale.x * glyph.width;
					v1.y = v0.y - scale.y * glyph.height;

					u0.x = mUVRect.xMin + invX * glyph.x;
					u0.y = mUVRect.yMax - invY * glyph.y;

					u1.x = u0.x + invX * glyph.width;
					u1.y = u0.y - invY * glyph.height;

					verts.Add(new Vector3(v1.x, v0.y, 0f));
					verts.Add(v1);
					verts.Add(new Vector3(v0.x, v1.y, 0f));
					verts.Add(v0);

					uvs.Add(new Vector2(u1.x, u0.y));
					uvs.Add(u1);
					uvs.Add(new Vector2(u0.x, u1.y));
					uvs.Add(u0);

					cols.Add(color);
					cols.Add(color);
					cols.Add(color);
					cols.Add(color);

					x += mSpacingX + glyph.advance;
					prev = c;
				}
			}
		}
	}
}