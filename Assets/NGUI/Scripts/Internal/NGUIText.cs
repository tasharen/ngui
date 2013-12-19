//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

#if !UNITY_3_5 && !UNITY_FLASH
#define DYNAMIC_FONT
#endif

using UnityEngine;
using System.Text;

/// <summary>
/// Helper class containing functionality related to using dynamic fonts.
/// </summary>

static public class NGUIText
{
	public enum SymbolStyle
	{
		None,
		Uncolored,
		Colored,
	}

	public class GlyphInfo
	{
		public float x = 0;
		public float y = 0;
		public float width = 0;
		public float height = 0;
		public float advance = 0;
		public int channel = 0;
	}

	/// <summary>
	/// When printing text, a lot of additional data must be passed in. In order to save allocations,
	/// this data is not passed at all, but is rather set in a single place before calling the functions that use it.
	/// </summary>

	static public BMFont bitmapFont;
#if DYNAMIC_FONT
	static public Font dynamicFont;
#endif
	static public GlyphInfo glyph = new GlyphInfo();

	static public int size = 16;
	static public float pixelDensity = 1f;
	static public FontStyle style = FontStyle.Normal;
	static public TextAlignment alignment = TextAlignment.Left;
	static public Color tint = Color.white;
		
	static public int lineWidth = 1000000;
	static public int lineHeight = 1000000;
	static public int maxLines = 0;

	static public bool gradient = false;
	static public Color gradientBottom = Color.white;
	static public Color gradientTop = Color.white;

	static public bool encoding = false;
	static public int spacingX = 0;
	static public int spacingY = 0;
	static public bool premultiply = false;
	static public SymbolStyle symbolStyle;

	static public int finalSize = 0;
	static public float finalSpacingX = 0f;
	static public float finalSpacingY = 0f;
	static public float finalLineWidth = 0f;
	static public float finalLineHeight = 0f;

	/// <summary>
	/// Recalculate the 'final' values.
	/// </summary>

	static public void Update()
	{
		finalSize = Mathf.RoundToInt(size * pixelDensity);
		finalSpacingX = (spacingX * pixelDensity);
		finalSpacingY = (spacingY * pixelDensity);
		finalLineWidth = (lineWidth * pixelDensity);
		finalLineHeight = (lineHeight * pixelDensity);
	}

	/// <summary>
	/// Prepare to use the specified text.
	/// </summary>

	static public void Prepare (string text)
	{
#if DYNAMIC_FONT
		if (dynamicFont != null)
			dynamicFont.RequestCharactersInTexture(text, finalSize, style);
#endif
	}

	static public GlyphInfo GetGlyph (char ch, char prev)
	{
		if (bitmapFont != null)
		{
			BMGlyph bmg = bitmapFont.GetGlyph(ch);

			if (bmg != null)
			{
				glyph.x = bmg.offsetX;
				glyph.y = bmg.offsetY;
				glyph.width = bmg.width;
				glyph.height = bmg.height;
				glyph.advance = bmg.advance;
				glyph.channel = bmg.channel;
				if (prev != 0) glyph.x += bmg.GetKerning(prev);
				return glyph;
			}
		}
/*#if DYNAMIC_FONT
		else if (dynamicFont != null)
		{
			if (dynamicFont.GetCharacterInfo(ch, out mTempChar, finalSize, style))
			{
				glyph.x = mTempChar.vert.xMin;
				glyph.y = bmg.offsetY;
				glyph.width = mTempChar.vert.width;
				glyph.height = bmg.height;
				glyph.advance = bmg.advance;
				glyph.channel = bmg.channel;
				if (prev != 0) glyph.x += bmg.GetKerning(prev);
				return glyph;

				v0.x = x + mTempChar.vert.xMin;
				v1.x = v0.x + mTempChar.vert.width;
				v0.y = mTempChar.vert.yMax - baseline - y;
				v1.y = v0.y - mTempChar.vert.height;

				u0.x = mTempChar.uv.xMin;
				u0.y = mTempChar.uv.yMin;
				u1.x = mTempChar.uv.xMax;
				u1.y = mTempChar.uv.yMax;

				if (mTempChar.flipped)
				{
					uvs.Add(new Vector2(u0.x, u0.y));
					uvs.Add(new Vector2(u1.x, u0.y));
					uvs.Add(new Vector2(u1.x, u1.y));
					uvs.Add(new Vector2(u0.x, u1.y));
				}
				else
				{
					uvs.Add(new Vector2(u0.x, u0.y));
					uvs.Add(new Vector2(u0.x, u1.y));
					uvs.Add(new Vector2(u1.x, u1.y));
					uvs.Add(new Vector2(u1.x, u0.y));
				}
			}
		}
#endif*/
		return null;
	}

	static Color mInvisible = new Color(0f, 0f, 0f, 0f);
#if DYNAMIC_FONT
	static BetterList<Color> mColors = new BetterList<Color>();
	static CharacterInfo mTempChar;
#endif

	/// <summary>
	/// Parse a RrGgBb color encoded in the string.
	/// </summary>

	static public Color ParseColor (string text, int offset)
	{
		int r = (NGUIMath.HexToDecimal(text[offset])     << 4) | NGUIMath.HexToDecimal(text[offset + 1]);
		int g = (NGUIMath.HexToDecimal(text[offset + 2]) << 4) | NGUIMath.HexToDecimal(text[offset + 3]);
		int b = (NGUIMath.HexToDecimal(text[offset + 4]) << 4) | NGUIMath.HexToDecimal(text[offset + 5]);
		float f = 1f / 255f;
		return new Color(f * r, f * g, f * b);
	}

	/// <summary>
	/// The reverse of ParseColor -- encodes a color in RrGgBb format.
	/// </summary>

	static public string EncodeColor (Color c)
	{
		int i = 0xFFFFFF & (NGUIMath.ColorToInt(c) >> 8);
		return NGUIMath.DecimalToHex(i);
	}

	/// <summary>
	/// Parse an embedded symbol, such as [FFAA00] (set color) or [-] (undo color change). Returns how many characters to skip.
	/// </summary>

	static public int ParseSymbol (string text, int index)
	{
		int length = text.Length;

		if (index + 2 < length && text[index] == '[')
		{
			if (text[index + 1] == '-')
			{
				if (text[index + 2] == ']')
					return 3;
			}
			else if (index + 7 < length)
			{
				if (text[index + 7] == ']')
				{
					Color c = ParseColor(text, index + 1);
					if (EncodeColor(c) == text.Substring(index + 1, 6).ToUpper())
						return 8;
				}
			}
		}
		return 0;
	}

	/// <summary>
	/// Parse an embedded symbol, such as [FFAA00] (set color) or [-] (undo color change). Returns whether the index was adjusted.
	/// </summary>

	static public bool ParseSymbol (string text, ref int index)
	{
		int val = ParseSymbol(text, index);
		
		if (val != 0)
		{
			index += val;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Parse an embedded symbol, such as [FFAA00] (set color) or [-] (undo color change). Returns whether the index was adjusted.
	/// </summary>

	static public bool ParseSymbol (string text, ref int index, BetterList<Color> colors, bool premultiply)
	{
		if (colors == null) return ParseSymbol(text, ref index);

		int length = text.Length;

		if (index + 2 < length && text[index] == '[')
		{
			if (text[index + 1] == '-')
			{
				if (text[index + 2] == ']')
				{
					if (colors != null && colors.size > 1)
						colors.RemoveAt(colors.size - 1);
					index += 3;
					return true;
				}
			}
			else if (index + 7 < length)
			{
				if (text[index + 7] == ']')
				{
					if (colors != null)
					{
						Color c = ParseColor(text, index + 1);

						if (EncodeColor(c) != text.Substring(index + 1, 6).ToUpper())
							return false;

						c.a = colors[colors.size - 1].a;
						if (premultiply && c.a != 1f)
							c = Color.Lerp(mInvisible, c, c.a);

						colors.Add(c);
					}
					index += 8;
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Runs through the specified string and removes all color-encoding symbols.
	/// </summary>

	static public string StripSymbols (string text)
	{
		if (text != null)
		{
			for (int i = 0, imax = text.Length; i < imax; )
			{
				char c = text[i];

				if (c == '[')
				{
					int retVal = ParseSymbol(text, i);

					if (retVal != 0)
					{
						text = text.Remove(i, retVal);
						imax = text.Length;
						continue;
					}
				}
				++i;
			}
		}
		return text;
	}

	/// <summary>
	/// Align the vertices to be right or center-aligned given the line width specified by NGUIText.lineWidth.
	/// </summary>

	static public void Align (BetterList<Vector3> verts, int indexOffset, float offset)
	{
		if (alignment != TextAlignment.Left)
		{
			float padding = 0f;
			float lineWidth = finalLineWidth;

			if (alignment == TextAlignment.Right)
			{
				padding = lineWidth - offset;
				if (padding < 0f) padding = 0f;
			}
			else
			{
				// Centered alignment
				padding = (lineWidth - offset) * 0.5f;
				if (padding < 0f) padding = 0f;

				// Keep it pixel-perfect
				int diff = Mathf.RoundToInt(lineWidth - offset);
				int intWidth = Mathf.RoundToInt(lineWidth);

				bool oddDiff = (diff & 1) == 1;
				bool oddWidth = (intWidth & 1) == 1;
				if ((oddDiff && !oddWidth) || (!oddDiff && oddWidth))
					padding += 0.5f;
			}

			padding /= pixelDensity;

			for (int i = indexOffset; i < verts.size; ++i)
			{
#if UNITY_FLASH
				verts.buffer[i] = verts.buffer[i] + new Vector3(padding, 0f);
#else
				verts.buffer[i] = verts.buffer[i];
				verts.buffer[i].x += padding;
#endif
			}
		}
	}

	/// <summary>
	/// Get the index of the closest character within the provided list of values.
	/// This function first sorts by Y, and only then by X.
	/// </summary>

	static public int GetClosestCharacter (BetterList<Vector3> verts, Vector2 pos)
	{
		// First sort by Y, and only then by X
		float bestX = float.MaxValue;
		float bestY = float.MaxValue;
		int bestIndex = 0;

		for (int i = 0; i < verts.size; ++i)
		{
			float diffY = Mathf.Abs(pos.y - verts[i].y);
			if (diffY > bestY) continue;

			float diffX = Mathf.Abs(pos.x - verts[i].x);

			if (diffY < bestY)
			{
				bestY = diffY;
				bestX = diffX;
				bestIndex = i;
			}
			else if (diffX < bestX)
			{
				bestX = diffX;
				bestIndex = i;
			}
		}
		return bestIndex;
	}

	/// <summary>
	/// Convenience function that ends the line by either appending a new line character or replacing a space with one.
	/// </summary>

	static public void EndLine (ref StringBuilder s)
	{
		int i = s.Length - 1;
		if (i > 0 && s[i] == ' ') s[i] = '\n';
		else s.Append('\n');
	}

#if DYNAMIC_FONT
	/// <summary>
	/// Get the printed size of the specified string. The returned value is in pixels.
	/// </summary>

	static public Vector2 CalculatePrintedSize (Font font, string text)
	{
		Vector2 v = Vector2.zero;

		if (font != null && !string.IsNullOrEmpty(text))
		{
			// When calculating printed size, get rid of all symbols first since they are invisible anyway
			if (encoding) text = StripSymbols(text);

			// Ensure we have characters to work with
			int size = finalSize;
			font.RequestCharactersInTexture(text, size, style);

			float x = 0;
			float y = 0;
			float maxX = 0f;
			float lineHeight = size + finalSpacingY;
			float spacingX = finalSpacingX;
			int chars = text.Length;

			for (int i = 0; i < chars; ++i)
			{
				char c = text[i];

				// Start a new line
				if (c == '\n')
				{
					if (x > maxX) maxX = x;
					x = 0f;
					y += lineHeight;
					continue;
				}

				// Skip invalid characters
				if (c < ' ') continue;

				if (font.GetCharacterInfo(c, out mTempChar, size, style))
					x += mTempChar.width + spacingX;
			}

			// Padding is always between characters, so it's one less than the number of characters
			v.x = ((x > maxX) ? x : maxX);
			v.y = (y + size);
			v /= pixelDensity;
		}
		return v;
	}

	/// <summary>
	/// Calculate the character index offset required to print the end of the specified text.
	/// NOTE: This function assumes that the text has been stripped of all symbols.
	/// </summary>

	static public int CalculateOffsetToFit (Font font, string text)
	{
		if (font == null || string.IsNullOrEmpty(text) || lineWidth < 1) return 0;

		// Ensure we have the characters to work with
		int size = finalSize;
		font.RequestCharactersInTexture(text, size, style);

		float remainingWidth = finalLineWidth;
		int textLength = text.Length;
		int currentCharacterIndex = textLength;

		while (currentCharacterIndex > 0 && remainingWidth > 0f)
		{
			char c = text[--currentCharacterIndex];
			if (font.GetCharacterInfo(c, out mTempChar, size, style))
				remainingWidth -= mTempChar.width;
		}

		if (remainingWidth < 0f) ++currentCharacterIndex;
		return currentCharacterIndex;
	}

	/// <summary>
	/// Ensure that we have the requested characters present.
	/// </summary>

	static public void RequestCharactersInTexture (Font font, string text)
	{
		if (font != null)
		{
			font.RequestCharactersInTexture(text, finalSize, style);
		}
	}

	/// <summary>
	/// Text wrapping functionality. The 'width' and 'height' should be in pixels.
	/// </summary>

	static public bool WrapText (Font font, string text, out string finalText)
	{
		if (lineWidth < 1 || lineHeight < 1 || string.IsNullOrEmpty(text))
		{
			finalText = "";
			return false;
		}

		int maxLineCount = (maxLines > 0) ? maxLines : 1000000;
		int size = finalSize;
		float height = (maxLines > 0) ?
			Mathf.Min(finalLineHeight, size * maxLines) :
			finalLineHeight;

		float sum = size + finalSpacingY;
		maxLineCount = Mathf.FloorToInt((sum > 0) ? Mathf.Min(maxLineCount, height / sum) : 0);

		if (maxLineCount == 0)
		{
			finalText = "";
			return false;
		}

		// Ensure that we have the required characters to work with
		if (font != null) font.RequestCharactersInTexture(text, size, style);

		StringBuilder sb = new StringBuilder();
		int textLength = text.Length;
		float lw = finalLineWidth;
		float remainingWidth = lw;
		float spaceX = finalSpacingX;

		int start = 0;
		int offset = 0;
		int lineCount = 1;
		int previousChar = 0;
		bool lineIsEmpty = true;

		// Run through all characters
		for (; offset < textLength; ++offset)
		{
			char ch = text[offset];

			// New line character -- start a new line
			if (ch == '\n')
			{
				if (lineCount == maxLineCount) break;
				remainingWidth = lw;

				// Add the previous word to the final string
				if (start < offset) sb.Append(text.Substring(start, offset - start + 1));
				else sb.Append(ch);

				lineIsEmpty = true;
				++lineCount;
				start = offset + 1;
				previousChar = 0;
				continue;
			}

			// If this marks the end of a word, add it to the final string.
			if (ch == ' ' && previousChar != ' ' && start < offset)
			{
				sb.Append(text.Substring(start, offset - start + 1));
				lineIsEmpty = false;
				start = offset + 1;
				previousChar = ch;
			}

			// When encoded symbols such as [RrGgBb] or [-] are encountered, skip past them
			if (ParseSymbol(text, ref offset)) { --offset; continue; }

			// If the character is missing for any reason, skip it
			if (!font.GetCharacterInfo(ch, out mTempChar, size, style)) continue;

			float glyphWidth = spaceX + mTempChar.width;
			remainingWidth -= glyphWidth;

			// Doesn't fit?
			if (remainingWidth < 0)
			{
				// Can't start a new line
				if (lineIsEmpty || lineCount == maxLineCount)
				{
					// This is the first word on the line -- add it up to the character that fits
					sb.Append(text.Substring(start, Mathf.Max(0, offset - start)));

					if (lineCount++ == maxLineCount)
					{
						start = offset;
						break;
					}
					EndLine(ref sb);

					// Start a brand-new line
					lineIsEmpty = true;

					if (ch == ' ')
					{
						start = offset + 1;
						remainingWidth = lw;
					}
					else
					{
						start = offset;
						remainingWidth = lw - glyphWidth;
					}
					previousChar = 0;
				}
				else
				{
					// Skip all spaces before the word
					while (start < textLength && text[start] == ' ') ++start;

					// Revert the position to the beginning of the word and reset the line
					lineIsEmpty = true;
					remainingWidth = lw;
					offset = start - 1;
					previousChar = 0;

					if (lineCount++ == maxLineCount) break;
					EndLine(ref sb);
					continue;
				}
			}
			else previousChar = ch;
		}

		if (start < offset) sb.Append(text.Substring(start, offset - start));
		finalText = sb.ToString();
		return (offset == textLength) || (lineCount <= Mathf.Min(maxLines, maxLineCount));
	}

	static Color32 s_c0, s_c1;

	/// <summary>
	/// Helper function that retrieves the font's baseline.
	/// There is currently no way to retrieve this value in a clean fashion from Unity, so it's using a hack.
	/// </summary>

	static float GetBaseline (Font font)
	{
		int size = finalSize;
		font.RequestCharactersInTexture("j", size, style);
		font.GetCharacterInfo('j', out mTempChar, size, style);
		return mTempChar.vert.yMin + (Mathf.RoundToInt(size - Mathf.Abs(mTempChar.vert.height)) >> 1);
	}

	/// <summary>
	/// Print the specified text into the buffers.
	/// </summary>

	static public void Print (Font font, string text, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		if (font == null || string.IsNullOrEmpty(text)) return;

		int size = finalSize;
		int indexOffset = verts.size;
		float lineHeight = size + finalSpacingY;

		// We need to know the baseline first
		float baseline = GetBaseline(font);

		// Ensure that the text we're about to print exists in the font's texture
		font.RequestCharactersInTexture(text, size, style);

		// Start with the white tint
		mColors.Add(Color.white);

		float x = 0f, y = 0f, maxX = 0f;
		float spacingX = finalSpacingX;
		float pixelSize = 1f / pixelDensity;
		float sizeF = size;

		Vector3 v0 = Vector3.zero, v1 = Vector3.zero;
		Vector2 u0 = Vector2.zero, u1 = Vector2.zero;
		Color gb = tint * gradientBottom;
		Color gt = tint * gradientTop;
		Color32 uc = tint;
		int textLength = text.Length;

		for (int i = 0; i < textLength; ++i)
		{
			char c = text[i];

			if (c == '\n')
			{
				if (x > maxX) maxX = x;

				if (alignment != TextAlignment.Left)
				{
					Align(verts, indexOffset, x - spacingX);
					indexOffset = verts.size;
				}

				x = 0;
				y += lineHeight;
				continue;
			}

			if (c < ' ') continue;

			// Color changing symbol
			if (encoding && ParseSymbol(text, ref i, mColors, premultiply))
			{
				Color fc = tint * mColors[mColors.size - 1];
				uc = fc;

				if (gradient)
				{
					gb = gradientBottom * fc;
					gt = gradientTop * fc;
				}
				--i;
				continue;
			}

			if (!font.GetCharacterInfo(c, out mTempChar, size, style))
				continue;

			v0.x = x + mTempChar.vert.xMin;
			v1.x = v0.x + mTempChar.vert.width;
			v0.y = mTempChar.vert.yMax - baseline - y;
			v1.y = v0.y - mTempChar.vert.height;
			//v0.y = -y;
			//v1.y = -y + size;

			if (pixelSize != 1f)
			{
				v0 *= pixelSize;
				v1 *= pixelSize;
			}

			x += (mTempChar.width + spacingX);

			verts.Add(new Vector3(v0.x, v0.y));
			verts.Add(new Vector3(v0.x, v1.y));
			verts.Add(new Vector3(v1.x, v1.y));
			verts.Add(new Vector3(v1.x, v0.y));

			// Texture coordinates
			if (uvs != null)
			{
				u0.x = mTempChar.uv.xMin;
				u0.y = mTempChar.uv.yMin;
				u1.x = mTempChar.uv.xMax;
				u1.y = mTempChar.uv.yMax;

				if (mTempChar.flipped)
				{
					uvs.Add(new Vector2(u0.x, u0.y));
					uvs.Add(new Vector2(u1.x, u0.y));
					uvs.Add(new Vector2(u1.x, u1.y));
					uvs.Add(new Vector2(u0.x, u1.y));
				}
				else
				{
					uvs.Add(new Vector2(u0.x, u0.y));
					uvs.Add(new Vector2(u0.x, u1.y));
					uvs.Add(new Vector2(u1.x, u1.y));
					uvs.Add(new Vector2(u1.x, u0.y));
				}
			}

			// Vertex colors
			if (cols != null)
			{
				if (gradient)
				{
					float min = sizeF - (-mTempChar.vert.yMax + baseline);
					float max = min - (mTempChar.vert.height);

					min /= sizeF;
					max /= sizeF;

					s_c0 = Color.Lerp(gb, gt, min);
					s_c1 = Color.Lerp(gb, gt, max);

					cols.Add(s_c0);
					cols.Add(s_c1);
					cols.Add(s_c1);
					cols.Add(s_c0);
				}
				else for (int b = 0; b < 4; ++b) cols.Add(uc);
			}
		}

		if (alignment != TextAlignment.Left && indexOffset < verts.size)
		{
			Align(verts, indexOffset, x - spacingX);
			indexOffset = verts.size;
		}
		mColors.Clear();
	}

	/// <summary>
	/// Print character positions and indices into the specified buffer. Meant to be used with the "find closest vertex" calculations.
	/// </summary>

	static public void PrintCharacterPositions (Font font, string text, BetterList<Vector3> verts, BetterList<int> indices)
	{
		if (font == null || string.IsNullOrEmpty(text)) return;

		// Ensure that the text we're about to print exists in the font's texture
		int fontSize = finalSize;
		font.RequestCharactersInTexture(text, fontSize, style);

		float x = 0f, y = 0f, maxX = 0f, halfSize = fontSize * 0.5f;
		float spacingX = finalSpacingX;
		float pixelSize = 1f / pixelDensity;
		float lineHeight = fontSize + finalSpacingY;
		int textLength = text.Length, indexOffset = verts.size;

		for (int i = 0; i < textLength; ++i)
		{
			char c = text[i];

			verts.Add(new Vector3(pixelSize * x, pixelSize * (-y - halfSize)));
			indices.Add(i);

			if (c == '\n')
			{
				if (x > maxX) maxX = x;

				if (alignment != TextAlignment.Left)
				{
					Align(verts, indexOffset, x - spacingX);
					indexOffset = verts.size;
				}

				x = 0;
				y += lineHeight;
				continue;
			}
			else if (c < ' ') continue;

			if (encoding && ParseSymbol(text, ref i))
			{
				--i;
				continue;
			}

			if (font.GetCharacterInfo(c, out mTempChar, fontSize, style))
			{
				x += mTempChar.width + spacingX;
				verts.Add(new Vector3(pixelSize * x, pixelSize * (-y - halfSize)));
				indices.Add(i + 1);
			}
		}

		if (alignment != TextAlignment.Left && indexOffset < verts.size)
			Align(verts, indexOffset, x - spacingX);
	}

	/// <summary>
	/// Print the caret and selection vertices. Note that it's expected that 'text' has been stripped clean of symbols.
	/// </summary>

	static public void PrintCaretAndSelection (Font font, string text, int start, int end, BetterList<Vector3> caret, BetterList<Vector3> highlight)
	{
		if (font == null || string.IsNullOrEmpty(text)) return;

		int caretPos = end;

		if (start > end)
		{
			end = start;
			start = caretPos;
		}

		// Ensure that the text we're about to use exists in the font's texture
		int size = finalSize;
		font.RequestCharactersInTexture(text, size, style);

		float x = 0f, y = 0f, maxX = 0f, fs = size;
		float lineHeight = size + finalSpacingY;
		float spacingX = finalSpacingX;
		float pixelSize = 1f / pixelDensity;
		int caretOffset = (caret != null) ? caret.size : 0;
		int highlightOffset = (highlight != null) ? highlight.size : 0;
		int textLength = text.Length, index = 0;
		bool highlighting = false, caretSet = false;

		Vector2 v0 = Vector2.zero;
		Vector2 v1 = Vector2.zero;
		Vector2 last0 = Vector2.zero;
		Vector2 last1 = Vector2.zero;

		for (; index < textLength; ++index)
		{
			// Print the caret
			if (caret != null && !caretSet && caretPos <= index)
			{
				caretSet = true;
				caret.Add(new Vector3(x - 1f, -y - fs));
				caret.Add(new Vector3(x - 1f, -y));
				caret.Add(new Vector3(x + 1f, -y));
				caret.Add(new Vector3(x + 1f, -y - fs));
			}

			char c = text[index];

			if (c == '\n')
			{
				// Used for alignment purposes
				if (x > maxX) maxX = x;

				// Align the caret
				if (caret != null && caretSet)
				{
					if (NGUIText.alignment != TextAlignment.Left)
						NGUIText.Align(caret, caretOffset, x - NGUIText.spacingX);
					caret = null;
				}

				if (highlight != null)
				{
					if (highlighting)
					{
						// Close the selection on this line
						highlighting = false;
						highlight.Add(last1);
						highlight.Add(last0);
					}
					else if (start <= index && end > index)
					{
						// This must be an empty line. Add a narrow vertical highlight.
						highlight.Add(new Vector3(x, -y - fs));
						highlight.Add(new Vector3(x, -y));
						highlight.Add(new Vector3(x + 2f, -y));
						highlight.Add(new Vector3(x + 2f, -y - fs));
					}

					// Align the highlight
					if (NGUIText.alignment != TextAlignment.Left && highlightOffset < highlight.size)
					{
						NGUIText.Align(highlight, highlightOffset, x - NGUIText.spacingX);
						highlightOffset = highlight.size;
					}
				}

				x = 0;
				y += lineHeight;
				continue;
			}
			else if (c < ' ') continue;

			if (encoding && ParseSymbol(text, ref index, mColors, premultiply))
			{
				--index;
				continue;
			}

			if (font.GetCharacterInfo(c, out mTempChar, size, style))
			{
				v0.x = x + mTempChar.vert.xMin;
				v1.x = v0.x + mTempChar.vert.width;
				v0.y = -y - fs;
				v1.y = -y;

				if (pixelSize != 1f)
				{
					v0 *= pixelSize;
					v1 *= pixelSize;
				}

				x += (mTempChar.width + spacingX);

				// Print the highlight
				if (highlight != null)
				{
					if (start > index || end <= index)
					{
						if (highlighting)
						{
							// Finish the highlight
							highlighting = false;
							highlight.Add(last1);
							highlight.Add(last0);
						}
					}
					else if (!highlighting)
					{
						// Start the highlight
						highlighting = true;
						highlight.Add(new Vector3(v0.x, v0.y));
						highlight.Add(new Vector3(v0.x, v1.y));
					}
				}

				// Save what the character ended with
				last0 = new Vector2(v1.x, v0.y);
				last1 = new Vector2(v1.x, v1.y);
			}
		}

		// Ensure we always have a caret
		if (caret != null)
		{
			if (!caretSet)
			{
				caret.Add(new Vector3(x - 1f, -y - fs));
				caret.Add(new Vector3(x - 1f, -y));
				caret.Add(new Vector3(x + 1f, -y));
				caret.Add(new Vector3(x + 1f, -y - fs));
			}

			if (NGUIText.alignment != TextAlignment.Left)
				NGUIText.Align(caret, caretOffset, x - NGUIText.spacingX);
		}

		// Close the selection
		if (highlight != null)
		{
			if (highlighting)
			{
				// Finish the highlight
				highlight.Add(last1);
				highlight.Add(last0);
			}
			else if (start < index && end == index)
			{
				// Happens when highlight ends on an empty line. Highlight it with a thin line.
				highlight.Add(new Vector3(x, -y - fs));
				highlight.Add(new Vector3(x, -y));
				highlight.Add(new Vector3(x + 2f, -y));
				highlight.Add(new Vector3(x + 2f, -y - fs));
			}

			// Align the highlight
			if (NGUIText.alignment != TextAlignment.Left && highlightOffset < highlight.size)
				NGUIText.Align(highlight, highlightOffset, x - NGUIText.spacingX);
		}
	}
#endif // DYNAMIC_FONT
}
