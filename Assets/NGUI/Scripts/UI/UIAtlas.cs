using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI Atlas contains a collection of sprites inside one large texture atlas.
/// </summary>

[AddComponentMenu("NGUI/Atlas")]
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
				Vector2 size = new Vector2(tex.width, tex.height);

				foreach (Sprite s in sprites)
				{
					if (mCoordinates == Coordinates.TexCoords)
					{
						s.outer = ConvertToTexCoords(s.outer, size);
						s.inner = ConvertToTexCoords(s.inner, size);
					}
					else
					{
						s.outer = ConvertToPixels(s.outer, size);
						s.inner = ConvertToPixels(s.inner, size);
					}
				}
			}
		}
	}

	/// <summary>
	/// Helper function.
	/// </summary>

	public static Rect ConvertToTexCoords (Rect rect, Vector2 texSize)
	{
		if (texSize.x != 0f && texSize.y != 0f)
		{
			rect.xMin = rect.xMin / texSize.x;
			rect.xMax = rect.xMax / texSize.x;
			rect.yMin = 1f - rect.yMin / texSize.y;
			rect.yMax = 1f - rect.yMax / texSize.y;
		}
		return rect;
	}

	/// <summary>
	/// Helper function.
	/// </summary>

	public static Rect ConvertToPixels (Rect rect, Vector2 texSize)
	{
		rect.xMin = Mathf.RoundToInt(rect.xMin * texSize.x);
		rect.xMax = Mathf.RoundToInt(rect.xMax * texSize.x);
		rect.yMin = Mathf.RoundToInt((1f - rect.yMin) * texSize.y);
		rect.yMax = Mathf.RoundToInt((1f - rect.yMax) * texSize.y);
		return rect;
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