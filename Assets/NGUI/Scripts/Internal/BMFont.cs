using UnityEngine;

/// <summary>
/// BMFont reader. C# implementation of http://www.angelcode.com/products/bmfont/
/// </summary>

[System.Serializable]
public class BMFont
{
#if UNITY_FLASH // Unity 3.5b6 is bugged when SerializeField is mixed with prefabs (after LoadLevel)
	public BMGlyph[] mGlyphs = null;
	public int mSize = 0;	// How much to move the cursor when moving to the next line
	public int mBase = 0;	// Offset from the top of the line to the base of each character
	public int mWidth = 0;	// Original width of the texture
	public int mHeight = 0;	// Original height of the texture
#else
	[SerializeField] BMGlyph[] mGlyphs = null;
	[SerializeField] int mSize = 0;		// How much to move the cursor when moving to the next line
	[SerializeField] int mBase = 0;		// Offset from the top of the line to the base of each character
	[SerializeField] int mWidth = 0;	// Original width of the texture
	[SerializeField] int mHeight = 0;	// Original height of the texture
#endif

	public bool isValid		{ get { return mGlyphs != null && mGlyphs.Length > 0; } }
	public int charSize		{ get { return mSize; } set { mSize = value; } }
	public int baseOffset	{ get { return mBase; } set { mBase = value; } }
	public int texWidth		{ get { return mWidth; } set { mWidth = value; } }
	public int texHeight	{ get { return mHeight; } set { mHeight = value; } }
	public int glyphCount	{ get { return mGlyphs == null ? 0 : mGlyphs.Length; } }

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
		// Start with a standard UTF-8 character set
		if (mGlyphs == null)
		{
			if (!createIfMissing) return null;
			int size = GetArraySize(index);
			if (size == 0) return null;
			mGlyphs = new BMGlyph[size];
		}

		// If necessary, upgrade to a unicode character set
		if (index >= mGlyphs.Length)
		{
			if (!createIfMissing) return null;
			int size = GetArraySize(index);
			if (size == 0) return null;
			BMGlyph[] glyphs = new BMGlyph[size];
			for (int i = 0; i < mGlyphs.Length; ++i) glyphs[i] = mGlyphs[i];
			mGlyphs = glyphs;
		}

		// Get the requested glyph
		BMGlyph glyph = mGlyphs[index];

		// If the glyph doesn't exist, create it
		if (glyph == null && createIfMissing)
		{
			glyph = new BMGlyph();
			mGlyphs[index] = glyph;
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

	public void Clear () { mGlyphs = null; }
}