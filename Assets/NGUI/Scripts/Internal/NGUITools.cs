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
	/// Play the specified audio clip.
	/// </summary>

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
	/// Parse a RrGgBb color encoded in the string.
	/// </summary>

	static public Color ParseColor (string text, int offset)
	{
		int r = (NGUIMath.HexToDecimal(text[offset])	 << 4) | NGUIMath.HexToDecimal(text[offset + 1]);
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
	/// Add a collider to the game object containing one or more widgets.
	/// </summary>

	static public void AddWidgetCollider (GameObject go)
	{
		if (go != null)
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

			Bounds b = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
			box.isTrigger = true;
			box.center = b.center;
			box.size = b.size; // Need 3D colliders? Try this: new Vector3(b.size.x, b.size.y, Mathf.Max(1f, b.size.z));
		}
	}

	/// <summary>
	/// Want to swap a low-res atlas for a hi-res one? Just use this function.
	/// </summary>

	static public void ReplaceAtlas (UIAtlas before, UIAtlas after)
	{
		UISprite[] sprites = Resources.FindObjectsOfTypeAll(typeof(UISprite)) as UISprite[];
		
		foreach (UISprite sprite in sprites)
		{
			if (sprite.atlas == before)
			{
				sprite.atlas = after;
			}
		}

		UILabel[] labels = Resources.FindObjectsOfTypeAll(typeof(UILabel)) as UILabel[];

		foreach (UILabel lbl in labels)
		{
			if (lbl.font != null && lbl.font.atlas == before)
			{
				lbl.font.atlas = after;
			}
		}
	}

	/// <summary>
	/// Want to swap a low-res font for a hi-res one? This is the way.
	/// </summary>

	static public void ReplaceFont (UIFont before, UIFont after)
	{
		UILabel[] labels = Resources.FindObjectsOfTypeAll(typeof(UILabel)) as UILabel[];

		foreach (UILabel lbl in labels)
		{
			if (lbl.font == before)
			{
				lbl.font = after;
			}
		}
	}

	#region Deprecated functions
	[System.Obsolete("Use NGUIMath.HexToDecimal instead")]
	static public int HexToDecimal (char ch) { return NGUIMath.HexToDecimal(ch); }

	[System.Obsolete("Use NGUIMath.ColorToInt instead")]
	static public int ColorToInt (Color c) { return NGUIMath.ColorToInt(c); }

	[System.Obsolete("Use NGUIMath.IntToColor instead")]
	static public Color IntToColor (int val) { return NGUIMath.IntToColor(val); }

	[System.Obsolete("Use NGUIMath.HexToColor instead")]
	static public Color HexToColor (uint val) { return NGUIMath.HexToColor(val); }

	[System.Obsolete("Use NGUIMath.ConvertToTexCoords instead")]
	static public Rect ConvertToTexCoords (Rect rect, int width, int height) { return NGUIMath.ConvertToTexCoords(rect, width, height); }

	[System.Obsolete("Use NGUIMath.ConvertToPixels instead")]
	static public Rect ConvertToPixels (Rect rect, int width, int height, bool round) { return NGUIMath.ConvertToPixels(rect, width, height, round); }

	[System.Obsolete("Use NGUIMath.MakePixelPerfect instead")]
	static public Rect MakePixelPerfect (Rect rect) { return NGUIMath.MakePixelPerfect(rect); }

	[System.Obsolete("Use NGUIMath.MakePixelPerfect instead")]
	static public Rect MakePixelPerfect (Rect rect, int width, int height) { return NGUIMath.MakePixelPerfect(rect, width, height); }

	[System.Obsolete("Use NGUIMath.ApplyHalfPixelOffset instead")]
	static public Vector3 ApplyHalfPixelOffset (Vector3 pos) { return NGUIMath.ApplyHalfPixelOffset(pos); }

	[System.Obsolete("Use NGUIMath.ApplyHalfPixelOffset instead")]
	static public Vector3 ApplyHalfPixelOffset (Vector3 pos, Vector3 scale) { return NGUIMath.ApplyHalfPixelOffset(pos, scale); }

	[System.Obsolete("Use NGUIMath.ConstrainRect instead")]
	static public Vector2 ConstrainRect (Vector2 minRect, Vector2 maxRect, Vector2 minArea, Vector2 maxArea) { return NGUIMath.ConstrainRect(minRect, maxRect, minArea, maxArea); }

	[System.Obsolete("Use NGUIMath.CalculateAbsoluteWidgetBounds instead")]
	static public Bounds CalculateAbsoluteWidgetBounds (Transform trans) { return NGUIMath.CalculateAbsoluteWidgetBounds(trans); }

	[System.Obsolete("Use NGUIMath.CalculateRelativeWidgetBounds instead")]
	static public Bounds CalculateRelativeWidgetBounds (Transform root, Transform child) { return NGUIMath.CalculateRelativeWidgetBounds(root, child); }

	[System.Obsolete("Use NGUIMath.CalculateRelativeWidgetBounds instead")]
	static public Bounds CalculateRelativeWidgetBounds (Transform trans) { return NGUIMath.CalculateRelativeWidgetBounds(trans); }
	#endregion
}