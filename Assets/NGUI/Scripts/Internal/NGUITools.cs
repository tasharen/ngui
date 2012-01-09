using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Helper class containing generic functions used throughout the UI library.
/// </summary>

static public class NGUITools
{
	static AudioListener mListener;

	/// <summary>
	/// Play the specifid audio clip.
	/// </summary>
	/// <param name="clip"></param>

	static public void PlaySound (AudioClip clip) { PlaySound(clip, 1f); }

	/// <summary>
	/// Play the specified audio clip with the specified volume.
	/// </summary>

	static public void PlaySound (AudioClip clip, float volume)
	{
#if UNITY_3_4
		if (clip != null)
#else
		// NOTE: There seems to be a bug with PlayOneShot in Flash using Unity 3.5 b6
		if (clip != null && Application.platform != RuntimePlatform.FlashPlayer)
#endif
		{
			if (mListener == null)
			{
				mListener = GameObject.FindObjectOfType(typeof(AudioListener)) as AudioListener;
				if (mListener == null) mListener = Camera.main.gameObject.AddComponent<AudioListener>();
			}

			AudioSource source = mListener.audio;
			if (source == null) source = mListener.gameObject.AddComponent<AudioSource>();

			source.PlayOneShot(clip, volume);
		}
	}

	/// <summary>
	/// Same as Random.Range, but the returned value is >= min and <= max.
	/// Unity's Random.Range is < max instead, unless min == max.
	/// This means Range(0,1) produces 0 instead of 0 or 1. That's unacceptable.
	/// </summary>

	static public int RandomRange (int min, int max)
	{
		if (min == max) return min;
		return UnityEngine.Random.Range(min, max + 1);
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
	/// Convert the specified color to RGBA32 integer format.
	/// </summary>

	static public int ColorToInt (Color c)
	{
		int retVal = 0;
		retVal |= Mathf.RoundToInt(c.r * 255f) << 24;
		retVal |= Mathf.RoundToInt(c.g * 255f) << 16;
		retVal |= Mathf.RoundToInt(c.b * 255f) << 8;
		retVal |= Mathf.RoundToInt(c.a * 255f);
		return retVal;
	}

	/// <summary>
	/// Convert the specified RGBA32 integer to Color.
	/// </summary>

	static public Color IntToColor (int val)
	{
		float inv = 1f / 255f;
		Color c = Color.black;
		c.r = inv * ((val >> 24) & 0xFF);
		c.g = inv * ((val >> 16) & 0xFF);
		c.b = inv * ((val >> 8) & 0xFF);
		c.a = inv * (val & 0xFF);
		return c;
	}

	/// <summary>
	/// Convenience conversion function, allowing hex format (0xRrGgBbAa).
	/// </summary>

	static public Color HexToColor (uint val)
	{
		return IntToColor((int)val);
	}

	/// <summary>
	/// Parse a RrGgBb color encoded in the string.
	/// </summary>

	static public Color ParseColor (string text, int offset)
	{
		int r = (HexToDecimal(text[offset])		<< 4) | HexToDecimal(text[offset + 1]);
		int g = (HexToDecimal(text[offset + 2]) << 4) | HexToDecimal(text[offset + 3]);
		int b = (HexToDecimal(text[offset + 4]) << 4) | HexToDecimal(text[offset + 5]);
		float f = 1f / 255f;
		return new Color(f * r, f * g, f * b);
	}

	/// <summary>
	/// The reverse of ParseColor -- encodes a color in RrGgBb format.
	/// </summary>

	static public string EncodeColor (Color c)
	{
		int i = 0xFFFFFF & (ColorToInt(c) >> 8);
#if UNITY_3_4
		return i.ToString("X6");
#else
		// int.ToString(format) doesn't seem to be supported on Flash as of 3.5 b6 -- it simply silently crashes
		return (Application.platform == RuntimePlatform.FlashPlayer) ? "FFFFFF" : i.ToString("X6");
#endif
	}

	/// <summary>
	/// Parse an embedded symbol, such as [FFAA00] (set color) or [-] (undo color change)
	/// </summary>

	static public int ParseSymbol (string text, int index, List<Color> colors)
	{
		int length = text.Length;

		if (index + 2 < length)
		{
			if (text[index + 1] == '-')
			{
				if (text[index + 2] == ']')
				{
					if (colors != null && colors.Count > 1) colors.RemoveAt(colors.Count - 1);
					return 3;
				}
			}
			else if (index + 7 < length)
			{
				if (text[index + 7] == ']')
				{
					if (colors != null)
					{
						Color c = ParseColor(text, index + 1);
						c.a = colors[colors.Count - 1].a;
						colors.Add(c);
					}
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
		if (text != null)
		{
			text = text.Replace("\\n", "\n");

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
		}
		return text;
	}

	/// <summary>
	/// Convert from top-left based pixel coordinates to bottom-left based UV coordinates.
	/// </summary>

	static public Rect ConvertToTexCoords (Rect rect, int width, int height)
	{
		Rect final = rect;

		if (width != 0f && height != 0f)
		{
			final.xMin = rect.xMin / width;
			final.xMax = rect.xMax / width;
			final.yMin = 1f - rect.yMax / height;
			final.yMax = 1f - rect.yMin / height;
		}
		return final;
	}

	/// <summary>
	/// Convert from bottom-left based UV coordinates to top-left based pixel coordinates.
	/// </summary>

	static public Rect ConvertToPixels (Rect rect, int width, int height, bool round)
	{
		Rect final = rect;

		if (round)
		{
			final.xMin = Mathf.RoundToInt(rect.xMin * width);
			final.xMax = Mathf.RoundToInt(rect.xMax * width);
			final.yMin = Mathf.RoundToInt((1f - rect.yMax) * height);
			final.yMax = Mathf.RoundToInt((1f - rect.yMin) * height);
		}
		else
		{
			final.xMin = rect.xMin * width;
			final.xMax = rect.xMax * width;
			final.yMin = (1f - rect.yMax) * height;
			final.yMax = (1f - rect.yMin) * height;
		}
		return final;
	}

	/// <summary>
	/// Round the pixel rectangle's dimensions.
	/// </summary>

	static public Rect MakePixelPerfect (Rect rect)
	{
		rect.xMin = Mathf.RoundToInt(rect.xMin);
		rect.yMin = Mathf.RoundToInt(rect.yMin);
		rect.xMax = Mathf.RoundToInt(rect.xMax);
		rect.yMax = Mathf.RoundToInt(rect.yMax);
		return rect;
	}

	/// <summary>
	/// Round the texture coordinate rectangle's dimensions.
	/// </summary>

	static public Rect MakePixelPerfect (Rect rect, int width, int height)
	{
		rect = ConvertToPixels(rect, width, height, true);
		rect.xMin = Mathf.RoundToInt(rect.xMin);
		rect.yMin = Mathf.RoundToInt(rect.yMin);
		rect.xMax = Mathf.RoundToInt(rect.xMax);
		rect.yMax = Mathf.RoundToInt(rect.yMax);
		return ConvertToTexCoords(rect, width, height);
	}

	/// <summary>
	/// The much-dreaded half-pixel offset of DirectX9:
	/// http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	/// </summary>

	static public Vector3 ApplyHalfPixelOffset (Vector3 pos)
	{
		RuntimePlatform platform = Application.platform;

		if (platform == RuntimePlatform.WindowsPlayer ||
			platform == RuntimePlatform.WindowsWebPlayer ||
			platform == RuntimePlatform.WindowsEditor)
		{
			pos.x = pos.x - 0.5f;
			pos.y = pos.y + 0.5f;
		}
		return pos;
	}

	/// <summary>
	/// Per-pixel offset taking scale into consideration.
	/// If the scale dimension is an odd number, it won't apply the offset.
	/// This is useful for centered sprites.
	/// </summary>

	static public Vector3 ApplyHalfPixelOffset (Vector3 pos, Vector3 scale)
	{
		RuntimePlatform platform = Application.platform;

		if (platform == RuntimePlatform.WindowsPlayer ||
			platform == RuntimePlatform.WindowsWebPlayer ||
			platform == RuntimePlatform.WindowsEditor)
		{
			if (Mathf.RoundToInt(scale.x) == (Mathf.RoundToInt(scale.x * 0.5f) * 2)) pos.x = pos.x - 0.5f;
			if (Mathf.RoundToInt(scale.y) == (Mathf.RoundToInt(scale.y * 0.5f) * 2)) pos.y = pos.y + 0.5f;
		}
		return pos;
	}

	/// <summary>
	/// Find the camera responsible for drawing the objects on the specified layer.
	/// </summary>

	static public Camera FindCameraForLayer (int layer)
	{
		int layerMask = 1 << layer;
		Camera[] cameras = GameObject.FindSceneObjectsOfType(typeof(Camera)) as Camera[];

		foreach (Camera cam in cameras)
		{
			if ((cam.cullingMask & layerMask) != 0)
			{
				return cam;
			}
		}
		return null;
	}

	/// <summary>
	/// Since Unity doesn't have a Vector*Vector function...
	/// </summary>

	static public Vector3 Multiply (Vector3 a, Vector3 b)
	{
		a.x *= b.x;
		a.y *= b.y;
		a.z *= b.z;
		return a;
	}

	/// <summary>
	/// Add a collider to the game object containing one or more widgets.
	/// </summary>

	static public void AddWidgetCollider (GameObject go)
	{
		Collider col = go.GetComponent<Collider>();
		BoxCollider box = col as BoxCollider;

		if (box == null)
		{
			if (col != null)
			{
				if (Application.isPlaying) GameObject.Destroy(col);
				else GameObject.DestroyImmediate(col);
			}
			box = go.AddComponent<BoxCollider>();
		}

		UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>() as UIWidget[];

		Matrix4x4 mat = go.transform.worldToLocalMatrix;
		Bounds b = new Bounds(Vector3.zero, Vector3.zero);

		foreach (UIWidget w in widgets)
		{
			Vector2 size = w.visibleSize;
			Vector2 offset = w.pivotOffset;
			float x = (offset.x + 0.5f) * size.x;
			float y = (offset.y - 0.5f) * size.y;
			size *= 0.5f;

			b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(x - size.x, y - size.y, 0f))));
			b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(x - size.x, y + size.y, 0f))));
			b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(x + size.x, y - size.y, 0f))));
			b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(x + size.x, y + size.y, 0f))));
		}

		box.isTrigger = true;
		box.center = b.center;
		box.size = b.size; // Need 3D colliders? Try this: new Vector3(b.size.x, b.size.y, Mathf.Max(1f, b.size.z));
	}
}