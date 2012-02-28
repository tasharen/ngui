//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Unity doesn't keep the values of static variables after scripts change get recompiled. One way around this
/// is to store the references in PlayerPrefs -- retrieve them at start, and save them whenever something changes.
/// </summary>

public class UISettings
{
	static bool mLoaded = false;
	static UIFont mFont;
	static UIAtlas mAtlas;
	static TextAsset mFontData;
	static Texture2D mFontTexture;
	static string mFontName = "New Font";
	static string mAtlasName = "New Atlas";
	static bool mPreview = true;

	static Object GetObject (string name)
	{
		int assetID = PlayerPrefs.GetInt(name, -1);
		return (assetID != -1) ? EditorUtility.InstanceIDToObject(assetID) : null;
	}

	static void Load ()
	{
		mLoaded			= true;
		mFontName		= PlayerPrefs.GetString("NGUI Font Name");
		mAtlasName		= PlayerPrefs.GetString("NGUI Atlas Name");
		mFontData		= GetObject("NGUI Font Asset") as TextAsset;
		mFontTexture	= GetObject("NGUI Font Texture") as Texture2D;
		mFont			= GetObject("NGUI Font") as UIFont;
		mAtlas			= GetObject("NGUI Atlas") as UIAtlas;
		mPreview		= PlayerPrefs.GetInt("NGUI Preview") == 0;
	}

	static void Save ()
	{
		PlayerPrefs.SetString("NGUI Font Name", mFontName);
		PlayerPrefs.SetString("NGUI Atlas Name", mAtlasName);
		PlayerPrefs.SetInt("NGUI Font Asset", (mFontData != null) ? mFontData.GetInstanceID() : -1);
		PlayerPrefs.SetInt("NGUI Font Texture", (mFontTexture != null) ? mFontTexture.GetInstanceID() : -1);
		PlayerPrefs.SetInt("NGUI Font", (mFont != null) ? mFont.GetInstanceID() : -1);
		PlayerPrefs.SetInt("NGUI Atlas", (mAtlas != null) ? mAtlas.GetInstanceID() : -1);
		PlayerPrefs.SetInt("NGUI Preview", mPreview ? 0 : 1);
	}

	/// <summary>
	/// Default font used by NGUI.
	/// </summary>

	static public UIFont font
	{
		get
		{
			if (!mLoaded) Load();
			return mFont;
		}
		set
		{
			if (mFont != value)
			{
				mFont = value;
				mFontName = (mFont != null) ? mFont.name : "New Font";
				Save();
			}
		}
	}

	/// <summary>
	/// Default atlas used by NGUI.
	/// </summary>

	static public UIAtlas atlas
	{
		get
		{
			if (!mLoaded) Load();
			return mAtlas;
		}
		set
		{
			if (mAtlas != value)
			{
				mAtlas = value;
				mAtlasName = (mAtlas != null) ? mAtlas.name : "New Atlas";
				Save();
			}
		}
	}

	/// <summary>
	/// Name of the font, used by the Font Maker.
	/// </summary>

	static public string fontName { get { if (!mLoaded) Load(); return mFontName; } set { if (mFontName != value) { mFontName = value; Save(); } } }

	/// <summary>
	/// Data used to create the font, used by the Font Maker.
	/// </summary>

	static public TextAsset fontData { get { if (!mLoaded) Load(); return mFontData; } set { if (mFontData != value) { mFontData = value; Save(); } } }

	/// <summary>
	/// Texture used to create the font, used by the Font Maker.
	/// </summary>

	static public Texture2D fontTexture { get { if (!mLoaded) Load(); return mFontTexture; } set { if (mFontTexture != value) { mFontTexture = value; Save(); } } }

	/// <summary>
	/// Name of the atlas, used by the Atlas maker.
	/// </summary>

	static public string atlasName { get { if (!mLoaded) Load(); return mAtlasName; } set { if (mAtlasName != value) { mAtlasName = value; Save(); } } }

	/// <summary>
	/// Whether the texture preview will be shown.
	/// </summary>

	static public bool texturePreview { get { if (!mLoaded) Load(); return mPreview; } set { if (mPreview != value) { mPreview = value; Save(); } } }
}