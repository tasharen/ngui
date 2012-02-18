using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Localization manager is able to parse localization information from text assets.
/// </summary>

[AddComponentMenu("NGUI/Internal/Localization")]
public class Localization : MonoBehaviour
{
	static Localization mInst;

	/// <summary>
	/// Language the localization manager will start with.
	/// </summary>

	public string startingLanguage;

	/// <summary>
	/// Available list of languages.
	/// </summary>

	public TextAsset[] languages;

	Dictionary<string, string> mDictionary = new Dictionary<string, string>();
	string mLanguage;

	/// <summary>
	/// Determine the starting language.
	/// </summary>

	void Awake ()
	{
		if (mInst == null)
		{
			mInst = this;
			
			if (string.IsNullOrEmpty(startingLanguage))
			{
				startingLanguage = PlayerPrefs.GetString("Language");
			}
		}
	}

	/// <summary>
	/// Remove the instance reference.
	/// </summary>

	void OnDestroy () { if (mInst == this) mInst = null; }

	/// <summary>
	/// Load the specified asset and activate the localization.
	/// </summary>

	void Load (TextAsset asset)
	{
		mLanguage = asset.name;
		PlayerPrefs.SetString("Language", mLanguage);
		ByteReader reader = new ByteReader(asset);
		mDictionary = reader.ReadDictionary();
		NGUITools.Broadcast("Localize");
	}

	/// <summary>
	/// Name of the currently active language.
	/// </summary>

	static public string currentLanguage
	{
		get
		{
			return (mInst != null) ? mInst.mLanguage : null;
		}
		set
		{
			if (mInst != null && mInst.languages != null && mInst.mLanguage != value)
			{
				if (string.IsNullOrEmpty(value))
				{
					mInst.mDictionary.Clear();
				}
				else
				{
					foreach (TextAsset asset in mInst.languages)
					{
						if (asset != null && asset.name == value)
						{
							mInst.Load(asset);
							return;
						}
					}
				}
				PlayerPrefs.DeleteKey("Language");
				mInst.startingLanguage = null;
			}
		}
	}

	/// <summary>
	/// Localize the specified value.
	/// </summary>

	static public string Get (string key)
	{
		if (mInst == null) return key;

		// If we haven't loaded a dictionary yet, see if we have something to load
		if (mInst.mDictionary.Count == 0 && mInst.languages != null &&
			!string.IsNullOrEmpty(mInst.startingLanguage))
		{
			// We have a starting language -- use it
			currentLanguage = mInst.startingLanguage;
		}
		string val;
		return (mInst.mDictionary.TryGetValue(key, out val)) ? val : key;
	}
}