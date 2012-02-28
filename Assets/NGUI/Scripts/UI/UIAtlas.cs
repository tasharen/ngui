//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

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
	/// NOTE: Kept only for legacy functionality's sake. Do not access directly! Use 'spriteList' instead.
	/// </summary>

	public List<Sprite> sprites = new List<Sprite>();

	// Currently active set of coordinates
	[SerializeField] Coordinates mCoordinates = Coordinates.Pixels;

	/// <summary>
	/// Replacement atlas can be used to completely bypass this atlas, pulling the data from another one instead.
	/// </summary>

	[SerializeField] UIAtlas mReplacement;

	/// <summary>
	/// List of sprites within the atlas.
	/// </summary>

	public List<Sprite> spriteList { get { return (mReplacement != null) ? mReplacement.spriteList : sprites; } }

	/// <summary>
	/// Texture used by the atlas.
	/// </summary>

	public Texture texture { get { return (mReplacement != null) ? mReplacement.texture : (material != null ? material.mainTexture as Texture : null); } }

	/// <summary>
	/// Allows switching of the coordinate system from pixel coordinates to texture coordinates.
	/// </summary>

	public Coordinates coordinates
	{
		get
		{
			return (mReplacement == null) ? mCoordinates : mReplacement.mCoordinates;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.coordinates = value;
			}
			else if (mCoordinates != value)
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
	/// Setting a replacement atlas value will cause everything using this atlas to use the replacement atlas instead.
	/// Suggested use: set up all your widgets to use a dummy atlas that points to the real atlas. Switching that atlas
	/// to another one (for example an HD atlas) is then a simple matter of setting this field on your dummy atlas.
	/// </summary>

	public UIAtlas replacement
	{
		get
		{
			return mReplacement;
		}
		set
		{
			if (mReplacement != value)
			{
				if (mReplacement != null) MarkAsDirty();
				mReplacement = value;
				if (mReplacement != null) MarkAsDirty();
			}
		}
	}

	/// <summary>
	/// Convenience function that retrieves a sprite by name.
	/// </summary>

	public Sprite GetSprite (string name)
	{
		if (mReplacement != null)
		{
			return mReplacement.GetSprite(name);
		}
		else if (!string.IsNullOrEmpty(name))
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

	public List<string> GetListOfSprites ()
	{
		if (mReplacement != null) return mReplacement.GetListOfSprites();
		List<string> list = new List<string>();
		foreach (Sprite s in sprites) if (s != null && !string.IsNullOrEmpty(s.name)) list.Add(s.name);
		list.Sort();
		return list;
	}

	/// <summary>
	/// Helper function that determines whether the atlas uses the specified one, taking replacements into account.
	/// </summary>

	bool References (UIAtlas atlas)
	{
		if (atlas == null) return false;
		if (atlas == this) return true;
		return (mReplacement != null) ? mReplacement.References(atlas) : false;
	}

	/// <summary>
	/// Helper function that determines whether the two atlases are related.
	/// </summary>

	static public bool CheckIfRelated (UIAtlas a, UIAtlas b)
	{
		if (a == null || b == null) return false;
		return a == b || a.References(b) || b.References(a);
	}

	/// <summary>
	/// Mark all widgets associated with this atlas as having changed.
	/// </summary>

	public void MarkAsDirty ()
	{
		UISprite[] list = (UISprite[])GameObject.FindSceneObjectsOfType(typeof(UISprite));

		foreach (UISprite sp in list)
		{
			if (CheckIfRelated(this, sp.atlas))
			{
				sp.atlas = null;
				sp.atlas = this;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(sp);
#endif
			}
		}

		UIFont[] fonts = (UIFont[])GameObject.FindSceneObjectsOfType(typeof(UIFont));

		foreach (UIFont font in fonts)
		{
			if (CheckIfRelated(this, font.atlas))
			{
				font.atlas = null;
				font.atlas = this;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(font);
#endif
			}
		}

		UILabel[] labels = (UILabel[])GameObject.FindSceneObjectsOfType(typeof(UILabel));

		foreach (UILabel lbl in labels)
		{
			if (lbl.font != null && CheckIfRelated(this, lbl.font.atlas))
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