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
	/// MemoryStream.ReadLine has an interesting oddity: it doesn't always advance the stream's position by the correct amount:
	/// http://social.msdn.microsoft.com/Forums/en-AU/Vsexpressvcs/thread/b8f7837b-e396-494e-88e1-30547fcf385f
	/// Solution? Custom line reader with the added benefit of not having to use streams at all.
	/// </summary>

	static string ReadLine (byte[] buffer, ref int offset)
	{
		int max = buffer.Length;

		// Skip empty characters
		while (offset < max && buffer[offset] < 32) ++offset;

		int end = offset;

		if (end < max)
		{
			for (;;)
			{
				if (end < max)
				{
					int ch = buffer[end++];
					if (ch != '\n' && ch != '\r') continue;
				}

				string line = Encoding.UTF8.GetString(buffer, offset, end - offset - 1);
				offset = end;
				return line;
			}
		}
		offset = max;
		return null;
	}

	/// <summary>
	/// Reload the font data.
	/// </summary>

	static public void Load (BMFont font, string name, byte[] bytes)
	{
		font.Clear();

		if (bytes != null)
		{
			int offset = 0;
			char[] separator = new char[1] { ' ' };

			while (offset < bytes.Length)
			{
				string line = ReadLine(bytes, ref offset);
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
					}
				}
			}
		}
	}
}