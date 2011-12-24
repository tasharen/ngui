using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Glyph structure used by BMFont. For more information see http://www.angelcode.com/products/bmfont/
/// </summary>

[System.Serializable]
public class BMGlyph
{
	public struct Kerning
	{
		public int previousChar;
		public int amount;
	}

	public int x;		// Offset from the left side of the texture to the left side of the glyph
	public int y;		// Offset from the top of the texture to the top of the glyph
	public int width;	// Glyph's width in pixels
	public int height;	// Glyph's height in pixels
	public int offsetX;	// Offset to apply to the cursor's left position before drawing this glyph
	public int offsetY; // Offset to apply to the cursor's top position before drawing this glyph
	public int advance;	// How much to move the cursor after printing this character

	public List<Kerning> kerning;

	/// <summary>
	/// Retrieves the special amount by which to adjust the cursor position, given the specified previous character.
	/// </summary>

	public int GetKerning (int previousChar)
	{
		if (kerning != null)
		{
			foreach (Kerning k in kerning)
			{
				if (k.previousChar == previousChar)
				{
					return k.amount;
				}
			}
		}
		return 0;
	}

	/// <summary>
	/// Add a new kerning entry to the character (or adjust an existing one).
	/// </summary>

	public void SetKerning (int previousChar, int amount)
	{
		if (kerning == null) kerning = new List<Kerning>();

		for (int i = 0; i < kerning.Count; ++i)
		{
			if (kerning[i].previousChar == previousChar)
			{
				Kerning k = kerning[i];
				k.amount = amount;
				kerning[i] = k;
				return;
			}
		}

		Kerning ker = new Kerning();
		ker.previousChar = previousChar;
		ker.amount = amount;
		kerning.Add(ker);
	}
}