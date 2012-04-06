//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// BMFont reader. C# implementation of http://www.angelcode.com/products/bmfont/
/// </summary>

[System.Serializable]
public class BMFont
{
	[SerializeField] BMGlyph[] mGlyphs = null;	// Prior to version 1.84, glyphs were stored in one large array
	[SerializeField] int mSize = 0;				// How much to move the cursor when moving to the next line
	[SerializeField] int mBase = 0;				// Offset from the top of the line to the base of each character
	[SerializeField] int mWidth = 0;			// Original width of the texture
	[SerializeField] int mHeight = 0;			// Original height of the texture
	[SerializeField] string mSpriteName;

	// List of serialized glyphs (1.84 and up)
	[SerializeField] List<BMGlyph> mSaved = new List<BMGlyph>();

	// Actual glyphs that we'll be working with are stored in a dictionary, making the lookup faster (1.84 and up)
	Dictionary<int, BMGlyph> mDict = new Dictionary<int, BMGlyph>();

	/// <summary>
	/// Whether the font can be used.
	/// </summary>

	public bool isValid { get { return (mSaved.Count > 0) || LegacyCheck(); } }

	/// <summary>
	/// Size of this font (for example 32 means 32 pixels).
	/// </summary>

	public int charSize { get { return mSize; } set { mSize = value; } }

	/// <summary>
	/// Base offset applied to characters.
	/// </summary>

	public int baseOffset { get { return mBase; } set { mBase = value; } }

	/// <summary>
	/// Original width of the texture.
	/// </summary>

	public int texWidth { get { return mWidth; } set { mWidth = value; } }

	/// <summary>
	/// Original height of the texture.
	/// </summary>

	public int texHeight { get { return mHeight; } set { mHeight = value; } }

	/// <summary>
	/// Number of valid glyphs.
	/// </summary>

	public int glyphCount { get { return isValid ? mSaved.Count : 0; } }

	/// <summary>
	/// Original name of the sprite that the font is expecting to find (usually the name of the texture).
	/// </summary>

	public string spriteName { get { return mSpriteName; } set { mSpriteName = value; } }

	/// <summary>
	/// Before version 1.84 all glyphs were stored in a single array. This was great, unless the glyphs happened to be
	/// outside of the 256 range, resulting in 65k characters, even if the font only used a few glyphs outside that range.
	/// This resulted in slower loading performance on mobile devices with a slow disk access.
	/// This function will convert pre-184 data to post-184 data. It will eventually be removed, so please don't use it.
	/// </summary>

	public bool LegacyCheck ()
	{
		if (mGlyphs != null && mGlyphs.Length > 0)
		{
			for (int i = 0, imax = mGlyphs.Length; i < imax; ++i)
			{
				BMGlyph bmg = mGlyphs[i];

				if (bmg != null)
				{
					bmg.index = i;
					mSaved.Add(bmg);
					mDict.Add(i, bmg);
				}
			}
			mGlyphs = null;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Helper function that calculates the ideal size of the array given an index.
	/// </summary>

	int GetArraySize (int index)
	{
		if (index < 256) return 256;
		if (index < 65536) return 65536;
		if (index < 262144) return 262144;
		return 0;
	}

	/// <summary>
	/// Helper function that retrieves the specified glyph, creating it if necessary.
	/// </summary>

	public BMGlyph GetGlyph (int index, bool createIfMissing)
	{
		// Get the requested glyph
		BMGlyph glyph = null;

		if (mDict.Count == 0)
		{
			// Legacy check for pre-1.84 fonts
			if (mSaved.Count == 0)
			{
				LegacyCheck();
			}
			else
			{
				// Populate the dictionary for faster access
				for (int i = 0, imax = mSaved.Count; i < imax; ++i)
				{
					BMGlyph bmg = mSaved[i];
					mDict.Add(bmg.index, bmg);
				}
			}
		}

		// Saved check is here so that the function call is not needed if it's true
		if (!mDict.TryGetValue(index, out glyph) && createIfMissing)
		{
			glyph = new BMGlyph();
			glyph.index = index;
			mSaved.Add(glyph);
			mDict.Add(index, glyph);
		}
		return glyph;
	}

	/// <summary>
	/// Read access to glyphs.
	/// </summary>

	public BMGlyph GetGlyph (int index) { return GetGlyph(index, false); }

	/// <summary>
	/// Clear the glyphs.
	/// </summary>

	public void Clear ()
	{
		mGlyphs = null;
		mDict.Clear();
		mSaved.Clear();
	}

	/// <summary>
	/// Trim the glyphs, ensuring that they will never go past the specified bounds.
	/// </summary>

	public void Trim (int xMin, int yMin, int xMax, int yMax)
	{
		if (isValid)
		{
			for (int i = 0, imax = mSaved.Count; i < imax; ++i)
			{
				BMGlyph glyph = mSaved[i];
				if (glyph != null) glyph.Trim(xMin, yMin, xMax, yMax);
			}
		}
	}
}