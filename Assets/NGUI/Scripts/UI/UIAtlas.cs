using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// UI Atlas contains a collection of sprites inside one large texture atlas.
/// </summary>

[AddComponentMenu("NGUI/UI/Atlas")]
public class UIAtlas : MonoBehaviour
{
	[System.Serializable]
	public class Sprite
	{
		public string name = "Unity Bug";
		public Rect outer = new Rect(0f, 0f, 1f, 1f);
		public Rect inner = new Rect(0f, 0f, 1f, 1f);

		// Padding is needed for trimmed sprites and is relative to sprite width and height
		public float paddingLeft	= 0f;
		public float paddingRight	= 0f;
		public float paddingTop		= 0f;
		public float paddingBottom	= 0f;

		public bool hasPadding { get { return paddingLeft != 0f || paddingRight != 0f || paddingTop != 0f || paddingBottom != 0f; } }
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
#if UNITY_FLASH // Unity 3.5b6 is bugged when SerializeField is mixed with prefabs (after LoadLevel)
	public Coordinates mCoordinates = Coordinates.Pixels;
#else
	[SerializeField] Coordinates mCoordinates = Coordinates.Pixels;
#endif

	/// <summary>
	/// Texture used by the atlas.
	/// </summary>

	public Texture texture { get { return material != null ? material.mainTexture as Texture : null; } }

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
						s.outer = NGUIMath.ConvertToTexCoords(s.outer, tex.width, tex.height);
						s.inner = NGUIMath.ConvertToTexCoords(s.inner, tex.width, tex.height);
					}
					else
					{
						s.outer = NGUIMath.ConvertToPixels(s.outer, tex.width, tex.height, true);
						s.inner = NGUIMath.ConvertToPixels(s.inner, tex.width, tex.height, true);
					}
				}
			}
		}
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
				// string.Equals doesn't seem to work with Flash export
				if (!string.IsNullOrEmpty(s.name) && name == s.name)
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

	virtual public List<string> GetListOfSprites ()
	{
		List<string> list = new List<string>();
		foreach (Sprite s in sprites) if (s != null && !string.IsNullOrEmpty(s.name)) list.Add(s.name);
		list.Sort();
		return list;
	}

	/// <summary>
	/// Mark all widgets associated with this atlas as having changed.
	/// </summary>

	public void MarkAsDirty ()
	{
		UISprite[] sprites = Resources.FindObjectsOfTypeAll(typeof(UISprite)) as UISprite[];

		foreach (UISprite sp in sprites)
		{
			if (sp.atlas == this)
			{
				sp.atlas = null;
				sp.atlas = this;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(sp);
#endif
			}
		}

		UIFont[] fonts = Resources.FindObjectsOfTypeAll(typeof(UIFont)) as UIFont[];

		foreach (UIFont font in fonts)
		{
			if (font.atlas == this)
			{
				font.atlas = null;
				font.atlas = this;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(font);
#endif
			}
		}

		UILabel[] labels = Resources.FindObjectsOfTypeAll(typeof(UILabel)) as UILabel[];

		foreach (UILabel lbl in labels)
		{
			if (lbl.font != null && lbl.font.atlas == this)
			{
				UIFont font = lbl.font;
				lbl.font = null;
				lbl.font = font;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(lbl);
#endif
			}
		}
	}
}