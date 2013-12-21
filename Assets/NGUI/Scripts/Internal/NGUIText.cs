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
		public Vector2 v0;
		public Vector2 v1;
		public Vector2 u0;
		public Vector2 u1;
		public float advance = 0f;
		public int channel = 0;
		public bool rotatedUVs = false;
	}

	/// <summary>
	/// When printing text, a lot of additional data must be passed in. In order to save allocations,
	/// this data is not passed at all, but is rather set in a single place before calling the functions that use it.
	/// </summary>

	static public UIFont bitmapFont;
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
	static public float baseline = 0f;
	static public bool useSymbols = false;

	/// <summary>
	/// Recalculate the 'final' values.
	/// </summary>

	static public void Update () { Update(true); }

	/// <summary>
	/// Recalculate the 'final' values.
	/// </summary>

	static public void Update (bool request)
	{
		finalSize = Mathf.RoundToInt(size * pixelDensity);
		useSymbols = (bitmapFont != null && bitmapFont.hasSymbols) && encoding && symbolStyle != SymbolStyle.None;

#if DYNAMIC_FONT
		if (dynamicFont != null && request)
		{
			dynamicFont.RequestCharactersInTexture("j", finalSize, style);
			dynamicFont.GetCharacterInfo('j', out mTempChar, finalSize, style);
			baseline = mTempChar.vert.yMax + finalSize;
		}
#endif
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

	static public BMSymbol GetSymbol (string text, int index, int textLength)
	{
		return (bitmapFont != null) ? bitmapFont.MatchSymbol(text, index, textLength) : null;
	}

	/// <summary>
	/// Get the width of the specified glyph. Returns zero if the glyph could not be retrieved.
	/// </summary>

	static public float GetGlyphWidth (int ch, int prev)
	{
		if (bitmapFont != null)
		{
			BMGlyph bmg = bitmapFont.bmFont.GetGlyph(ch);

			if (bmg != null)
			{
				return (prev != 0) ? bmg.advance + bmg.GetKerning(prev) / pixelDensity : bmg.advance;
			}
		}
#if DYNAMIC_FONT
		else if (dynamicFont != null)
		{
			if (dynamicFont.GetCharacterInfo((char)ch, out mTempChar, finalSize, style))
			{
				return Mathf.Round(mTempChar.width / pixelDensity);
			}
		}
#endif
		return 0f;
	}

	/// <summary>
	/// Get the specified glyph.
	/// </summary>

	static public GlyphInfo GetGlyph (int ch, int prev)
	{
		if (bitmapFont != null)
		{
			BMGlyph bmg = bitmapFont.bmFont.GetGlyph(ch);

			if (bmg != null)
			{
				int kern = (prev != 0) ? bmg.GetKerning(prev) : 0;
				glyph.v0.x = (prev != 0) ? bmg.offsetX + kern : bmg.offsetX;
				glyph.v1.y = -bmg.offsetY;

				glyph.v1.x = glyph.v0.x + bmg.width;
				glyph.v0.y = glyph.v1.y - bmg.height;

				glyph.u0.x = bmg.x;
				glyph.u0.y = bmg.y + bmg.height;

				glyph.u1.x = bmg.x + bmg.width;
				glyph.u1.y = bmg.y;

				glyph.v0 /= pixelDensity;
				glyph.v1 /= pixelDensity;

				glyph.advance = (bmg.advance + kern) / pixelDensity;
				glyph.channel = bmg.channel;
				glyph.rotatedUVs = false;
				return glyph;
			}
		}
#if DYNAMIC_FONT
		else if (dynamicFont != null)
		{
			if (dynamicFont.GetCharacterInfo((char)ch, out mTempChar, finalSize, style))
			{
				glyph.v0.x = mTempChar.vert.xMin;
				glyph.v1.x = glyph.v0.x + mTempChar.vert.width;

				glyph.v0.y = mTempChar.vert.yMax - baseline;
				glyph.v1.y = glyph.v0.y - mTempChar.vert.height;

				glyph.u0.x = mTempChar.uv.xMin;
				glyph.u0.y = mTempChar.uv.yMin;

				glyph.u1.x = mTempChar.uv.xMax;
				glyph.u1.y = mTempChar.uv.yMax;

				glyph.v0.x = Mathf.Round(glyph.v0.x) / pixelDensity;
				glyph.v0.y = Mathf.Round(glyph.v0.y) / pixelDensity;
				glyph.v1.x = Mathf.Round(glyph.v1.x) / pixelDensity;
				glyph.v1.y = Mathf.Round(glyph.v1.y) / pixelDensity;

				glyph.advance = Mathf.Round(mTempChar.width) / pixelDensity;
				glyph.channel = 0;
				glyph.rotatedUVs = mTempChar.flipped;
				return glyph;
			}
		}
#endif
		return null;
	}

	static Color mInvisible = new Color(0f, 0f, 0f, 0f);
	static BetterList<Color> mColors = new BetterList<Color>();
#if DYNAMIC_FONT
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
				int diff = Mathf.RoundToInt((lineWidth - offset) * pixelDensity);
				int intWidth = Mathf.RoundToInt(lineWidth);

				bool oddDiff = (diff & 1) == 1;
				bool oddWidth = (intWidth & 1) == 1;
				if ((oddDiff && !oddWidth) || (!oddDiff && oddWidth))
					padding += 0.5f * pixelDensity;
			}

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

	/// <summary>
	/// Get the printed size of the specified string. The returned value is in pixels.
	/// </summary>

	static public Vector2 CalculatePrintedSize (string text)
	{
		Vector2 v = Vector2.zero;

		if (!string.IsNullOrEmpty(text))
		{
			// When calculating printed size, get rid of all symbols first since they are invisible anyway
			if (encoding) text = StripSymbols(text);

			// Ensure we have characters to work with
			Prepare(text);

			float x = 0f, y = 0f, maxX = 0f;
			float lineHeight = size + spacingY;
			int textLength = text.Length, ch = 0, prev = 0;

			for (int i = 0; i < textLength; ++i)
			{
				ch = text[i];

				// Start a new line
				if (ch == '\n')
				{
					if (x > maxX) maxX = x;
					x = 0f;
					y += lineHeight;
					continue;
				}

				// Skip invalid characters
				if (ch < ' ') continue;

				// See if there is a symbol matching this text
				BMSymbol symbol = useSymbols ? GetSymbol(text, i, textLength) : null;

				if (symbol == null)
				{
					ch = text[i];
					float w = GetGlyphWidth(ch, prev);
					if (w != 0f) x += spacingX + w;
					prev = ch;
				}
				else
				{
					x += spacingX + symbol.advance;
					i += symbol.sequence.Length - 1;
					prev = 0;
				}
			}

			v.x = ((x > maxX) ? x : maxX);
			v.y = (y + size);
		}
		return v;
	}

	static BetterList<float> mSizes = new BetterList<float>();

	/// <summary>
	/// Calculate the character index offset required to print the end of the specified text.
	/// </summary>

	static public int CalculateOffsetToFit (string text)
	{
		if (string.IsNullOrEmpty(text) || lineWidth < 1) return 0;

		Prepare(text);

		int textLength = text.Length, ch = 0, prev = 0;

		for (int i = 0, imax = text.Length; i < imax; ++i)
		{
			// See if there is a symbol matching this text
			BMSymbol symbol = useSymbols ? GetSymbol(text, i, textLength) : null;

			if (symbol == null)
			{
				ch = text[i];
				float w = GetGlyphWidth(ch, prev);
				if (w != 0f) mSizes.Add(spacingX + w);
				prev = ch;
			}
			else
			{
				mSizes.Add(spacingX + symbol.advance);
				for (int b = 0, bmax = symbol.sequence.Length - 1; b < bmax; ++b) mSizes.Add(0);
				i += symbol.sequence.Length - 1;
				prev = 0;
			}
		}

		float remainingWidth = NGUIText.lineWidth;
		int currentCharacterIndex = mSizes.size;

		while (currentCharacterIndex > 0 && remainingWidth > 0)
			remainingWidth -= mSizes[--currentCharacterIndex];

		mSizes.Clear();

		if (remainingWidth < 0) ++currentCharacterIndex;
		return currentCharacterIndex;
	}

	/// <summary>
	/// Get the end of line that would fit into a field of given width.
	/// </summary>

	static public string GetEndOfLineThatFits (string text)
	{
		int textLength = text.Length;
		int offset = CalculateOffsetToFit(text);
		return text.Substring(offset, textLength - offset);
	}

#if DYNAMIC_FONT
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
#endif

	/// <summary>
	/// Text wrapping functionality. The 'width' and 'height' should be in pixels.
	/// </summary>

	static public bool WrapText (string text, out string finalText)
	{
		if (lineWidth < 1 || lineHeight < 1)
		{
			finalText = "";
			return false;
		}

		if (string.IsNullOrEmpty(text)) text = " ";

		float height = (maxLines > 0) ? Mathf.Min(lineHeight, size * maxLines) : lineHeight;
		float sum = size + spacingY;
		int maxLineCount = (maxLines > 0) ? maxLines : 1000000;
		maxLineCount = Mathf.FloorToInt((sum > 0) ? Mathf.Min(maxLineCount, height / sum) : 0);

		if (maxLineCount == 0)
		{
			finalText = "";
			return false;
		}

		Prepare(text);

		StringBuilder sb = new StringBuilder();
		int textLength = text.Length;
		float remainingWidth = lineWidth;
		int start = 0, offset = 0, lineCount = 1, prev = 0;
		bool lineIsEmpty = true;

		// Run through all characters
		for (; offset < textLength; ++offset)
		{
			char ch = text[offset];

			// New line character -- start a new line
			if (ch == '\n')
			{
				if (lineCount == maxLineCount) break;
				remainingWidth = lineWidth;

				// Add the previous word to the final string
				if (start < offset) sb.Append(text.Substring(start, offset - start + 1));
				else sb.Append(ch);

				lineIsEmpty = true;
				++lineCount;
				start = offset + 1;
				prev = 0;
				continue;
			}

			// If this marks the end of a word, add it to the final string.
			if (ch == ' ' && prev != ' ' && start < offset)
			{
				sb.Append(text.Substring(start, offset - start + 1));
				lineIsEmpty = false;
				start = offset + 1;
				prev = ch;
			}

			// When encoded symbols such as [RrGgBb] or [-] are encountered, skip past them
			if (encoding && ParseSymbol(text, ref offset)) { --offset; continue; }

			// See if there is a symbol matching this text
			BMSymbol symbol = useSymbols ? GetSymbol(text, offset, textLength) : null;

			// Calculate how wide this symbol or character is going to be
			float glyphWidth;

			if (symbol == null)
			{
				// Find the glyph for this character
				float w = GetGlyphWidth(ch, prev);
				if (w == 0f) continue;
				glyphWidth = spacingX + w;
			}
			else glyphWidth = spacingX + symbol.advance;

			// Reduce the width
			remainingWidth -= glyphWidth;

			// Doesn't fit?
			if (remainingWidth < 0f)
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
						remainingWidth = lineWidth;
					}
					else
					{
						start = offset;
						remainingWidth = lineWidth - glyphWidth;
					}
					prev = 0;
				}
				else
				{
					// Skip all spaces before the word
					while (start < textLength && text[start] == ' ') ++start;

					// Revert the position to the beginning of the word and reset the line
					lineIsEmpty = true;
					remainingWidth = lineWidth;
					offset = start - 1;
					prev = 0;

					if (lineCount++ == maxLineCount) break;
					EndLine(ref sb);
					continue;
				}
			}
			else prev = ch;

			// Advance the offset past the symbol
			if (symbol != null)
			{
				offset += symbol.length - 1;
				prev = 0;
			}
		}

		if (start < offset) sb.Append(text.Substring(start, offset - start));
		finalText = sb.ToString();
		return (offset == textLength) || (lineCount <= Mathf.Min(maxLines, maxLineCount));
	}

	static Color32 s_c0, s_c1;

	/// <summary>
	/// Print the specified text into the buffers.
	/// </summary>

	static public void Print (string text, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		if (string.IsNullOrEmpty(text)) return;

		int indexOffset = verts.size;
		float lineHeight = size + spacingY;

		Prepare(text);

		// Start with the white tint
		mColors.Add(Color.white);

		int ch = 0, prev = 0;
		float x = 0f, y = 0f, maxX = 0f;
		float sizeF = size;

		Color gb = tint * gradientBottom;
		Color gt = tint * gradientTop;
		Color32 uc = tint;
		int textLength = text.Length;
		
		Rect uvRect = new Rect();
		float invX = 0f, invY = 0f;

		if (bitmapFont != null)
		{
			uvRect = bitmapFont.uvRect;
			invX = uvRect.width / bitmapFont.texWidth;
			invY = uvRect.height / bitmapFont.texHeight;
		}

		for (int i = 0; i < textLength; ++i)
		{
			ch = text[i];

			if (ch == '\n')
			{
				if (x > maxX) maxX = x;

				if (alignment != TextAlignment.Left)
				{
					Align(verts, indexOffset, x - spacingX);
					indexOffset = verts.size;
				}

				x = 0;
				y += lineHeight;
				prev = 0;
				continue;
			}

			if (ch < ' ')
			{
				prev = ch;
				continue;
			}

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

			// See if there is a symbol matching this text
			BMSymbol symbol = useSymbols ? GetSymbol(text, i, textLength) : null;

			if (symbol == null)
			{
				GlyphInfo glyph = GetGlyph(ch, prev);
				if (glyph == null) continue;
				prev = ch;

				if (ch == ' ')
				{
					x += spacingX + glyph.advance;
					continue;
				}

				// Texture coordinates
				if (uvs != null)
				{
					if (bitmapFont != null)
					{
						glyph.u0.x = uvRect.xMin + invX * glyph.u0.x;
						glyph.u1.x = uvRect.xMin + invX * glyph.u1.x;
						glyph.u0.y = uvRect.yMax - invY * glyph.u0.y;
						glyph.u1.y = uvRect.yMax - invY * glyph.u1.y;
					}

					if (glyph.rotatedUVs)
					{
						uvs.Add(glyph.u0);
						uvs.Add(new Vector2(glyph.u1.x, glyph.u0.y));
						uvs.Add(glyph.u1);
						uvs.Add(new Vector2(glyph.u0.x, glyph.u1.y));
					}
					else
					{
						uvs.Add(glyph.u0);
						uvs.Add(new Vector2(glyph.u0.x, glyph.u1.y));
						uvs.Add(glyph.u1);
						uvs.Add(new Vector2(glyph.u1.x, glyph.u0.y));
					}
				}

				// Vertex colors
				if (cols != null)
				{
					if (glyph.channel == 0 || glyph.channel == 15)
					{
						if (gradient)
						{
							float min = sizeF + glyph.v0.y;
							float max = sizeF + glyph.v1.y;

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
					else
					{
						// Packed fonts come as alpha masks in each of the RGBA channels.
						// In order to use it we need to use a special shader.
						//
						// Limitations:
						// - Effects (drop shadow, outline) will not work.
						// - Should not be a part of the atlas (eastern fonts rarely are anyway).
						// - Lower color precision

						Color col = uc;

						col *= 0.49f;

						switch (glyph.channel)
						{
							case 1: col.b += 0.51f; break;
							case 2: col.g += 0.51f; break;
							case 4: col.r += 0.51f; break;
							case 8: col.a += 0.51f; break;
						}

						for (int b = 0; b < 4; ++b) cols.Add(col);
					}
				}

				glyph.v0.x += x;
				glyph.v1.x += x;
				glyph.v0.y -= y;
				glyph.v1.y -= y;

				x += spacingX + glyph.advance;

				verts.Add(glyph.v0);
				verts.Add(new Vector3(glyph.v0.x, glyph.v1.y));
				verts.Add(glyph.v1);
				verts.Add(new Vector3(glyph.v1.x, glyph.v0.y));
			}
			else // Symbol exists
			{
				float v0x = x + symbol.offsetX;
				float v1x = v0x + symbol.width;
				float v1y = -(y + symbol.offsetY);
				float v0y = v1y - symbol.height;

				verts.Add(new Vector3(v0x, v0y));
				verts.Add(new Vector3(v0x, v1y));
				verts.Add(new Vector3(v1x, v1y));
				verts.Add(new Vector3(v1x, v0y));

				x += spacingX + symbol.advance;
				i += symbol.length - 1;
				prev = 0;

				if (uvs != null)
				{
					Rect uv = symbol.uvRect;

					float u0x = uv.xMin;
					float u0y = uv.yMin;
					float u1x = uv.xMax;
					float u1y = uv.yMax;

					uvs.Add(new Vector2(u0x, u0y));
					uvs.Add(new Vector2(u0x, u1y));
					uvs.Add(new Vector2(u1x, u1y));
					uvs.Add(new Vector2(u1x, u0y));
				}

				if (cols != null)
				{
					if (symbolStyle == SymbolStyle.Colored)
					{
						for (int b = 0; b < 4; ++b) cols.Add(uc);
					}
					else
					{
						Color32 col = Color.white;
						col.a = uc.a;
						for (int b = 0; b < 4; ++b) cols.Add(col);
					}
				}
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

	static public void PrintCharacterPositions (string text, BetterList<Vector3> verts, BetterList<int> indices)
	{
		if (string.IsNullOrEmpty(text)) text = " ";

		Prepare(text);

		float x = 0f, y = 0f, maxX = 0f, halfSize = size * 0.5f;
		float lineHeight = size + spacingY;
		int textLength = text.Length, indexOffset = verts.size, ch = 0, prev = 0;

		for (int i = 0; i < textLength; ++i)
		{
			ch = text[i];

			verts.Add(new Vector3(x, -y - halfSize));
			indices.Add(i);

			if (ch == '\n')
			{
				if (x > maxX) maxX = x;

				if (alignment != TextAlignment.Left)
				{
					Align(verts, indexOffset, x - spacingX);
					indexOffset = verts.size;
				}

				x = 0;
				y += lineHeight;
				prev = 0;
				continue;
			}
			else if (ch < ' ')
			{
				prev = 0;
				continue;
			}

			if (encoding && ParseSymbol(text, ref i))
			{
				--i;
				continue;
			}

			// See if there is a symbol matching this text
			BMSymbol symbol = useSymbols ? GetSymbol(text, i, textLength) : null;

			if (symbol == null)
			{
				float w = GetGlyphWidth(ch, prev);

				if (w != 0f)
				{
					x += w + spacingX;
					verts.Add(new Vector3(x, -y - halfSize));
					indices.Add(i + 1);
					prev = ch;
				}
			}
			else
			{
				x += symbol.advance + spacingX;
				verts.Add(new Vector3(x, -y - halfSize));
				indices.Add(i + 1);
				i += symbol.sequence.Length - 1;
				prev = 0;
			}
		}

		if (alignment != TextAlignment.Left && indexOffset < verts.size)
			Align(verts, indexOffset, x - spacingX);
	}

	/// <summary>
	/// Print the caret and selection vertices. Note that it's expected that 'text' has been stripped clean of symbols.
	/// </summary>

	static public void PrintCaretAndSelection (string text, int start, int end, BetterList<Vector3> caret, BetterList<Vector3> highlight)
	{
		if (string.IsNullOrEmpty(text)) text = " ";

		Prepare(text);

		int caretPos = end;

		if (start > end)
		{
			end = start;
			start = caretPos;
		}

		float x = 0f, y = 0f, maxX = 0f, fs = size;
		float lineHeight = size + spacingY;
		int caretOffset = (caret != null) ? caret.size : 0;
		int highlightOffset = (highlight != null) ? highlight.size : 0;
		int textLength = text.Length, index = 0, ch = 0, prev = 0;
		bool highlighting = false, caretSet = false;

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

			ch = text[index];

			if (ch == '\n')
			{
				// Used for alignment purposes
				if (x > maxX) maxX = x;

				// Align the caret
				if (caret != null && caretSet)
				{
					if (NGUIText.alignment != TextAlignment.Left)
						NGUIText.Align(caret, caretOffset, x - spacingX);
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
						NGUIText.Align(highlight, highlightOffset, x - spacingX);
						highlightOffset = highlight.size;
					}
				}

				x = 0;
				y += lineHeight;
				prev = 0;
				continue;
			}
			else if (ch < ' ')
			{
				prev = 0;
				continue;
			}

			if (encoding && ParseSymbol(text, ref index, mColors, premultiply))
			{
				--index;
				continue;
			}

			// See if there is a symbol matching this text
			BMSymbol symbol = useSymbols ? GetSymbol(text, index, textLength) : null;
			float w = (symbol != null) ? symbol.advance : GetGlyphWidth(ch, prev);

			if (w != 0f)
			{
				float v0x = x;
				float v1x = x + w;
				float v0y = -y - fs;
				float v1y = -y;

				x += w + spacingX;

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
						highlight.Add(new Vector3(v0x, v0y));
						highlight.Add(new Vector3(v0x, v1y));
					}
				}

				// Save what the character ended with
				last0 = new Vector2(v1x, v0y);
				last1 = new Vector2(v1x, v1y);
				prev = ch;
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
				NGUIText.Align(caret, caretOffset, x - spacingX);
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
				NGUIText.Align(highlight, highlightOffset, x - spacingX);
		}
	}
}
