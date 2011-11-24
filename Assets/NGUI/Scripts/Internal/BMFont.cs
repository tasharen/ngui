using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// BMFont reader. C# implementation of http://www.angelcode.com/products/bmfont/
/// </summary>

public class BMFont
{
	BMGlyph[] mGlyphs = null;
	int mSize = 0;		// How much to move the cursor when moving to the next line
	int mBase = 0;		// Offset from the top of the line to the base of each character
	int mWidth = 0;		// Original width of the texture
	int mHeight = 0;	// Original height of the texture

	public bool isValid		{ get { return mGlyphs != null; } }
	public int charSize		{ get { return mSize; } }
	public int baseOffset	{ get { return mBase; } }
	public int texWidth		{ get { return mWidth; } }
	public int texHeight	{ get { return mHeight; } }

	/// <summary>
	/// Helper function that retrieves the value of the key=value pair.
	/// </summary>

	int GetValue (string s)
	{
		int idx = s.IndexOf('=');
		if (idx == -1) return 0;
		int val = 0;
		int.TryParse(s.Substring(idx + 1), out val);
		return val;
	}

	/// <summary>
	/// Helper function that retrieves the specified glyph, creating it if necessary.
	/// </summary>

	BMGlyph GetGlyph (int index, bool createIfMissing)
	{
		// Start with a standard UTF-8 character set
		if (mGlyphs == null)
		{
			if (!createIfMissing) return null;
			mGlyphs = new BMGlyph[256];
		}

		// If necessary, upgrade to a unicode character set
		if (index >= mGlyphs.Length)
		{
			if (!createIfMissing || index > 65535) return null;
			BMGlyph[] glyphs = new BMGlyph[65535];
			for (int i = 0; i < 256; ++i) glyphs[i] = mGlyphs[i];
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
	/// Reload the font data.
	/// </summary>

	public void Load (string name, byte[] bytes)
	{
		mGlyphs = null;

		if (bytes != null)
		{
			MemoryStream stream = new MemoryStream(bytes);
			StreamReader reader = new StreamReader(stream);

			char[] separator = new char[1] {' '};

			while (stream.Position < stream.Length)
			{
				string line = reader.ReadLine();
				if (string.IsNullOrEmpty(line)) break;
				string[] split = line.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);

				if (split[0] == "char")
				{
					// Expected data style:
					// char id=13 x=506 y=62 width=3 height=3 xoffset=-1 yoffset=50 xadvance=0 page=0 chnl=15

					if (split.Length == 11)
					{
						int id = GetValue(split[1]);
						BMGlyph glyph = GetGlyph(id, true);
						
						if (glyph != null)
						{
							glyph.x = GetValue(split[2]);
							glyph.y = GetValue(split[3]);
							glyph.width = GetValue(split[4]);
							glyph.height = GetValue(split[5]);
							glyph.offsetX = GetValue(split[6]);
							glyph.offsetY = GetValue(split[7]);
							glyph.advance = GetValue(split[8]);
						}
					}
					else
					{
						Debug.LogError("Unexpected number of entries for the 'char' field (" +
							name + ", " + split.Length + "):\n" + line);
						break;
					}
				}
				else if (split[0] == "kerning")
				{
					// Expected data style:
					// kerning first=84 second=244 amount=-5 

					if (split.Length == 4)
					{
						int first  = GetValue(split[1]);
						int second = GetValue(split[2]);
						int amount = GetValue(split[3]);

						BMGlyph glyph = GetGlyph(second, true);
						if (glyph != null) glyph.SetKerning(first, amount);
					}
					else
					{
						Debug.LogError("Unexpected number of entries for the 'kerning' field (" +
							name + ", " + split.Length + "):\n" + line);
						break;
					}
				}
				else if (split[0] == "common")
				{
					// Expected data style:
					// common lineHeight=64 base=51 scaleW=512 scaleH=512 pages=1 packed=0 alphaChnl=1 redChnl=4 greenChnl=4 blueChnl=4

					if (split.Length == 11)
					{
						mSize = GetValue(split[1]);
						mBase = GetValue(split[2]);
						mWidth = GetValue(split[3]);
						mHeight = GetValue(split[4]);
						int pages = GetValue(split[5]);

						if (pages != 1)
						{
							Debug.LogError("Font '" + name + "' must be created with only 1 texture, not " + pages);
							break;
						}
					}
					else
					{
						Debug.LogError("Unexpected number of entries for the 'common' field (" +
							name + ", " + split.Length + "):\n" + line);
						break;
					}
				}
			}

			reader.Close();
			stream.Close();
			stream.Dispose();
		}
	}
}