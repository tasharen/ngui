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

	/// <summary>
	/// When printing text, a lot of additional data must be passed in. In order to save allocations,
	/// this data is not passed at all, but is rather set in a single place before calling the functions that use it.
	/// </summary>

	public class Settings
	{
		public int size = 16;
		public FontStyle style = FontStyle.Normal;
		public TextAlignment alignment = TextAlignment.Left;
		public Color tint = Color.white;
		
		public int lineWidth = 1000000;
		public int lineHeight = 1000000;
		public int maxLines = 0;

		public bool gradient = false;
		public Color gradientBottom = Color.white;
		public Color gradientTop = Color.white;

		public bool encoding = false;
		public int spacingX = 0;
		public int spacingY = 0;
		public bool premultiply = false;
		public SymbolStyle symbolStyle;
	}

	/// <summary>
	/// This value contains a variety of properties that define how the text is printed.
	/// </summary>

	static public Settings current = new Settings();

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
	/// Align the vertices to be right or center-aligned given the line width specified by NGUIText.current.lineWidth.
	/// </summary>

	static public void Align (BetterList<Vector3> verts, int indexOffset, int pos)
	{
		if (current.alignment != TextAlignment.Left)
		{
			float offset = 0f;

			if (current.alignment == TextAlignment.Right)
			{
				offset = current.lineWidth - pos;
				if (offset < 0f) offset = 0f;
			}
			else
			{
				// Centered alignment
				offset = Mathf.RoundToInt((current.lineWidth - pos) * 0.5f);
				if (offset < 0f) offset = 0f;

				// Keep it pixel-perfect
				if ((current.lineWidth & 1) == 1) offset += 0.5f;
			}

			Vector3 temp;

			for (int i = indexOffset; i < verts.size; ++i)
			{
				temp = verts.buffer[i];
				temp.x += offset;
				verts.buffer[i] = temp;
			}
		}
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
			if (current.encoding) text = StripSymbols(text);

			// Ensure we have characters to work with
			font.RequestCharactersInTexture(text, current.size, current.style);

			float x = 0;
			float y = 0;
			float maxX = 0f;
			float lineHeight = current.size + current.spacingY;
			float spacingX = current.spacingX;
			int chars = text.Length;;

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

				if (font.GetCharacterInfo(c, out mTempChar, current.size, current.style))
					x += mTempChar.width + spacingX;
			}

			// Padding is always between characters, so it's one less than the number of characters
			v.x = ((x > maxX) ? x : maxX);
			v.y = (y + current.size);
		}
		return v;
	}

	/// <summary>
	/// Calculate the character index offset required to print the end of the specified text.
	/// NOTE: This function assumes that the text has been stripped of all symbols.
	/// </summary>

	static public int CalculateOffsetToFit (Font font, string text)
	{
		if (font == null || string.IsNullOrEmpty(text) || current.lineWidth < 1) return 0;

		// Ensure we have the characters to work with
		font.RequestCharactersInTexture(text, current.size, current.style);

		int textLength = text.Length;
		int remainingWidth = current.lineWidth;
		int currentCharacterIndex = textLength;

		while (currentCharacterIndex > 0 && remainingWidth > 0)
		{
			char c = text[--currentCharacterIndex];
			if (font.GetCharacterInfo(c, out mTempChar, current.size, current.style))
				remainingWidth -= (int)mTempChar.width;
		}

		if (remainingWidth < 0) ++currentCharacterIndex;
		return currentCharacterIndex;
	}

	/// <summary>
	/// Text wrapping functionality. The 'width' and 'height' should be in pixels.
	/// </summary>

	static public bool WrapText (Font font, string text, out string finalText)
	{
		if (current.lineWidth < 1 || current.lineHeight < 1 || string.IsNullOrEmpty(text))
		{
			finalText = "";
			return false;
		}

		int height = (current.maxLines > 0) ? Mathf.Min(current.lineHeight, current.size * current.maxLines) : current.lineHeight;
		int maxLineCount = (current.maxLines > 0) ? current.maxLines : 1000000;
		int sum = current.size + current.spacingY;
		maxLineCount = (sum > 0) ? Mathf.Min(maxLineCount, height / sum) : 0;

		if (maxLineCount == 0)
		{
			finalText = "";
			return false;
		}

		// Ensure that we have the required characters to work with
		if (font != null) font.RequestCharactersInTexture(text, current.size, current.style);

		StringBuilder sb = new StringBuilder();
		int textLength = text.Length;
		int remainingWidth = current.lineWidth;
		int previousChar = 0;
		int start = 0;
		int offset = 0;
		bool lineIsEmpty = true;
		int lineCount = 1;

		// Run through all characters
		for (; offset < textLength; ++offset)
		{
			char ch = text[offset];

			// New line character -- start a new line
			if (ch == '\n')
			{
				if (lineCount == maxLineCount) break;
				remainingWidth = current.lineWidth;

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
			if (!font.GetCharacterInfo(ch, out mTempChar, current.size, current.style)) continue;

			int glyphWidth = current.spacingX + Mathf.RoundToInt(mTempChar.width);
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
						remainingWidth = current.lineWidth;
					}
					else
					{
						start = offset;
						remainingWidth = current.lineWidth - glyphWidth;
					}
					previousChar = 0;
				}
				else
				{
					// Skip all spaces before the word
					while (start < textLength && text[start] == ' ') ++start;

					// Revert the position to the beginning of the word and reset the line
					lineIsEmpty = true;
					remainingWidth = current.lineWidth;
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
		return (offset == textLength) || (lineCount <= Mathf.Min(current.maxLines, maxLineCount));
	}

	static Color32 s_c0, s_c1;

	/// <summary>
	/// Print the specified text into the buffers.
	/// </summary>

	static public void Print (Font font, string text, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		if (font == null || string.IsNullOrEmpty(text)) return;

		float baseline = 0f;

		// We need to know the baseline first
		font.RequestCharactersInTexture("j", current.size, current.style);
		font.GetCharacterInfo('j', out mTempChar, current.size, current.style);
		baseline = current.size + mTempChar.vert.yMax;

		// Ensure that the text we're about to print exists in the font's texture
		font.RequestCharactersInTexture(text, current.size, current.style);

		// Start with the white tint
		mColors.Add(Color.white);

		int x = 0;
		int y = 0;
		int maxX = 0;
		int lineHeight = current.size + current.spacingY;
		int indexOffset = verts.size;
		Vector3 v0 = Vector3.zero, v1 = Vector3.zero;
		Vector2 u0 = Vector2.zero, u1 = Vector2.zero;
		Color gb = current.tint * current.gradientBottom;
		Color gt = current.tint * current.gradientTop;
		Color32 uc = current.tint;
		int textLength = text.Length;

		for (int i = 0; i < textLength; ++i)
		{
			char c = text[i];

			if (c == '\n')
			{
				if (x > maxX) maxX = x;

				if (current.alignment != TextAlignment.Left)
				{
					Align(verts, indexOffset, x - NGUIText.current.spacingX);
					indexOffset = verts.size;
				}

				x = 0;
				y += lineHeight;
				continue;
			}

			if (c < ' ') continue;

			// Color changing symbol
			if (current.encoding && ParseSymbol(text, ref i, mColors, current.premultiply))
			{
				Color fc = current.tint * mColors[mColors.size - 1];
				uc = current.tint;

				if (current.gradient)
				{
					gb = current.gradientBottom * fc;
					gt = current.gradientTop * fc;
				}
				--i;
				continue;
			}

			if (!font.GetCharacterInfo(c, out mTempChar, current.size, current.style))
				continue;

			v0.x =  (x + mTempChar.vert.xMin);
			v0.y = -(y - mTempChar.vert.yMax + baseline);

			v1.x = v0.x + mTempChar.vert.width;
			v1.y = v0.y - mTempChar.vert.height;

			u0.x = mTempChar.uv.xMin;
			u0.y = mTempChar.uv.yMin;
			u1.x = mTempChar.uv.xMax;
			u1.y = mTempChar.uv.yMax;

			x += (int)mTempChar.width + current.spacingX;

			verts.Add(new Vector3(v1.x, v0.y));
			verts.Add(new Vector3(v0.x, v0.y));
			verts.Add(new Vector3(v0.x, v1.y));
			verts.Add(new Vector3(v1.x, v1.y));

			if (mTempChar.flipped)
			{
				uvs.Add(new Vector2(u0.x, u1.y));
				uvs.Add(new Vector2(u0.x, u0.y));
				uvs.Add(new Vector2(u1.x, u0.y));
				uvs.Add(new Vector2(u1.x, u1.y));
			}
			else
			{
				uvs.Add(new Vector2(u1.x, u0.y));
				uvs.Add(new Vector2(u0.x, u0.y));
				uvs.Add(new Vector2(u0.x, u1.y));
				uvs.Add(new Vector2(u1.x, u1.y));
			}

			if (current.gradient)
			{
				float min = current.size - (-mTempChar.vert.yMax + baseline);
				float max = min - (mTempChar.vert.height);

				min /= current.size;
				max /= current.size;

				s_c0 = Color.Lerp(gb, gt, min);
				s_c1 = Color.Lerp(gb, gt, max);

				cols.Add(s_c0);
				cols.Add(s_c0);
				cols.Add(s_c1);
				cols.Add(s_c1);
			}
			else for (int b = 0; b < 4; ++b) cols.Add(uc);
		}

		if (current.alignment != TextAlignment.Left && indexOffset < verts.size)
		{
			Align(verts, indexOffset, x - current.spacingX);
			indexOffset = verts.size;
		}
		mColors.Clear();
	}
#endif // DYNAMIC_FONT
}
