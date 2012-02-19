//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Text;

/// <summary>
/// Helper class that takes care of loading BMFont's glyph information from the specified byte array.
/// This functionality is not a part of BMFont anymore because Flash export option can't handle System.IO functions.
/// </summary>

public static class BMFontReader
{
	/// <summary>
	/// Helper function that retrieves the string value of the key=value pair.
	/// </summary>

	static string GetString (string s)
	{
		int idx = s.IndexOf('=');
		return (idx == -1) ? "" : s.Substring(idx + 1);
	}

	/// <summary>
	/// Helper function that retrieves the integer value of the key=value pair.
	/// </summary>

	static int GetInt (string s)
	{
		int val = 0;
		string text = GetString(s);
#if UNITY_FLASH
		try { val = int.Parse(text); } catch (System.Exception) { }
#else
		int.TryParse(text, out val);
#endif
		return val;
	}

	/// <summary>
	/// Reload the font data.
	/// </summary>

	static public void Load (BMFont font, string name, byte[] bytes)
	{
		font.Clear();

		if (bytes != null)
		{
			ByteReader reader = new ByteReader(bytes);
			char[] separator = new char[] { ' ' };

			while (reader.canRead)
			{
				string line = reader.ReadLine();
				if (string.IsNullOrEmpty(line)) break;
				string[] split = line.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);

				if (split[0] == "char")
				{
					// Expected data style:
					// char id=13 x=506 y=62 width=3 height=3 xoffset=-1 yoffset=50 xadvance=0 page=0 chnl=15

					if (split.Length > 8)
					{
						int id = GetInt(split[1]);
						BMGlyph glyph = font.GetGlyph(id, true);

						if (glyph != null)
						{
							glyph.x			= GetInt(split[2]);
							glyph.y			= GetInt(split[3]);
							glyph.width		= GetInt(split[4]);
							glyph.height	= GetInt(split[5]);
							glyph.offsetX	= GetInt(split[6]);
							glyph.offsetY	= GetInt(split[7]);
							glyph.advance	= GetInt(split[8]);
						}
						else Debug.Log("Char: " + split[1] + " (" + id + ") is NULL");
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

					if (split.Length > 3)
					{
						int first  = GetInt(split[1]);
						int second = GetInt(split[2]);
						int amount = GetInt(split[3]);

						BMGlyph glyph = font.GetGlyph(second, true);
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

					if (split.Length > 5)
					{
						font.charSize	= GetInt(split[1]);
						font.baseOffset = GetInt(split[2]);
						font.texWidth	= GetInt(split[3]);
						font.texHeight	= GetInt(split[4]);

						int pages = GetInt(split[5]);

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
				else if (split[0] == "page")
				{
					// Expected data style:
					// page id=0 file="textureName.png"

					if (split.Length > 2)
					{
						font.spriteName = GetString(split[2]).Replace("\"", "");
						font.spriteName = font.spriteName.Replace(".png", "");
						font.spriteName = font.spriteName.Replace(".tga", "");
					}
				}
			}
		}
	}
}