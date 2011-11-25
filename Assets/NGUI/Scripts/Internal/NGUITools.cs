using UnityEngine;
using System;
using System.Collections.Generic;

static public class NGUITools
{
	/// <summary>
	/// Finds the component on the current game object or its parents.
	/// </summary>

	static public T FindInParents<T> (Transform trans) where T : Component
	{
		T comp = null;

		while (comp == null && trans != null)
		{
			comp = trans.GetComponent<T>();
			trans = trans.parent;
		}
		return comp;
	}

	/// <summary>
	/// Returns the hierarchy of the object in a human-readable format.
	/// </summary>

	static public string GetHierarchy (GameObject obj)
	{
		string path = obj.name;

		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			path = obj.name + "/" + path;
		}
		return "\"" + path + "\"";
	}

	/// <summary>
	/// Convert a hexadecimal character to its decimal value.
	/// </summary>

	static int HexToDecimal (char ch)
	{
		switch (ch)
		{
			case '0': return 0x0;
			case '1': return 0x1;
			case '2': return 0x2;
			case '3': return 0x3;
			case '4': return 0x4;
			case '5': return 0x5;
			case '6': return 0x6;
			case '7': return 0x7;
			case '8': return 0x8;
			case '9': return 0x9;
			case 'a':
			case 'A': return 0xA;
			case 'b':
			case 'B': return 0xB;
			case 'c':
			case 'C': return 0xC;
			case 'd':
			case 'D': return 0xD;
			case 'e':
			case 'E': return 0xE;
			case 'f':
			case 'F': return 0xF;
		}
		return 0xF;
	}

	/// <summary>
	/// Parse a color encoded in the string.
	/// </summary>

	static Color ParseColor (string text, int offset)
	{
		int r = (HexToDecimal(text[offset])		<< 4) | HexToDecimal(text[offset + 1]);
		int g = (HexToDecimal(text[offset + 2]) << 4) | HexToDecimal(text[offset + 3]);
		int b = (HexToDecimal(text[offset + 4]) << 4) | HexToDecimal(text[offset + 5]);
		float f = 1f / 255f;
		return new Color(f * r, f * g, f * b);
	}

	/// <summary>
	/// Parse an embedded symbol, such as [FFAA00] (set color) or [-] (undo color change)
	/// </summary>

	static public int ParseSymbol (string text, int index, Stack<Color> colors)
	{
		int length = text.Length;

		if (index + 2 < length)
		{
			if (text[index + 1] == '-')
			{
				if (text[index + 2] == ']')
				{
					if (colors != null && colors.Count > 1) colors.Pop();
					return 3;
				}
			}
			else if (index + 7 < length)
			{
				if (text[index + 7] == ']')
				{
					if (colors != null) colors.Push(ParseColor(text, index + 1));
					return 8;
				}
			}
		}
		return 0;
	}

	/// <summary>
	/// Runs through the specified string and removes all color-encoding symbols.
	/// </summary>

	static public string StripSymbols (string text)
	{
		text = text.Replace("\\n", "");

		for (int i = 0, imax = text.Length; i < imax; )
		{
			char c = text[i];

			if (c == '[')
			{
				int retVal = ParseSymbol(text, i, null);

				if (retVal > 0)
				{
					text = text.Remove(i, retVal);
					imax = text.Length;
					continue;
				}
			}
			++i;
		}
		return text;
	}
}