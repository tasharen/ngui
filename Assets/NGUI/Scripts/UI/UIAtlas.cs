using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI Atlas contains a collection of sprites inside one large texture atlas.
/// </summary>

[AddComponentMenu("NGUI/UI/Atlas")]
public class UIAtlas : MonoBehaviour
{
	[System.Serializable]
	public class Sprite
	{
		public string name;
		public Rect outer = new Rect(0f, 0f, 1f, 1f);
		public Rect inner = new Rect(0f, 0f, 1f, 1f);
	}

	/// <summary>
	/// Pixels coordinates are values within the texture specified in pixels. They are more intuitive,
	/// but will likely change if the texture gets resized. TexCoord coordinates range from 0 to 1,
	/// and won't change if the texture is resized. You can switch freely from one to the other prior
	/// to modifying the texture used by the atlas.
	/// </summary>

	public enum Coordinates
	{
		Pixels,
		TexCoords,
	}

	/// <summary>
	/// Material used by this atlas.
	/// </summary>

	public Material material;

	/// <summary>
	/// List of all sprites inside the atlas.
	/// </summary>

	public List<Sprite> sprites = new List<Sprite>();

	// Currently active set of coordinates
	[SerializeField] Coordinates mCoordinates = Coordinates.Pixels;

	/// <summary>
	/// Allows switching of the coordinate system from pixel coordinates to texture coordinates.
	/// </summary>

	public Coordinates coordinates
	{
		get
		{
			return mCoordinates;
		}
		set
		{
			if (mCoordinates != value)
			{
				if (material == null || material.mainTexture == null)
				{
					Debug.LogError("Can't switch coordinates until the atlas material has a valid texture");
					return;
				}

				mCoordinates = value;
				Texture tex = material.mainTexture;

				foreach (Sprite s in sprites)
				{
					if (mCoordinates == Coordinates.TexCoords)
					{
						s.outer = ConvertToTexCoords(s.outer, tex.width, tex.height);
						s.inner = ConvertToTexCoords(s.inner, tex.width, tex.height);
					}
					else
					{
						s.outer = ConvertToPixels(s.outer, tex.width, tex.height);
						s.inner = ConvertToPixels(s.inner, tex.width, tex.height);
					}
				}
			}
		}
	}

	/// <summary>
	/// Helper function.
	/// </summary>

	public static Rect ConvertToTexCoords (Rect rect, int width, int height)
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
	/// Helper function.
	/// </summary>

	public static Rect ConvertToPixels (Rect rect, int width, int height)
	{
		Rect final = rect;

		final.xMin = Mathf.RoundToInt(rect.xMin * width);
		final.xMax = Mathf.RoundToInt(rect.xMax * width);
		final.yMin = Mathf.RoundToInt((1f - rect.yMax) * height);
		final.yMax = Mathf.RoundToInt((1f - rect.yMin) * height);

		return final;
	}

	/// <summary>
	/// Convenience function that retrieves a sprite by name.
	/// </summary>

	public Sprite GetSprite (string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			foreach (Sprite s in sprites)
			{
				if (!string.IsNullOrEmpty(s.name) && string.Equals(s.name, name, System.StringComparison.OrdinalIgnoreCase))
				{
					return s;
				}
			}
		}
		else
		{
			Debug.LogWarning("Expected a valid name, found nothing");
		}
		return null;
	}

	/// <summary>
	/// Convenience function that retrieves a list of all sprite names.
	/// </summary>

	public string[] GetListOfSprites ()
	{
		List<string> list = new List<string>();
		foreach (Sprite s in sprites) if (s != null && !string.IsNullOrEmpty(s.name)) list.Add(s.name);
		return list.ToArray();
	}
}