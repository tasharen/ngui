//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Attach this script to a popup list, the parent of a group of togglees, or to a toggle itself to save its state.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Saved Option")]
public class UISavedOption : MonoBehaviour
{
	/// <summary>
	/// PlayerPrefs-stored key for this option.
	/// </summary>

	public string keyName;

	string key { get { return (string.IsNullOrEmpty(keyName)) ? "NGUI State: " + name : keyName; } }

	UIPopupList mList;
	UIToggle mCheck;

	/// <summary>
	/// Cache the components and register a listener callback.
	/// </summary>

	void Awake ()
	{
		mList = GetComponent<UIPopupList>();
		mCheck = GetComponent<UIToggle>();
		if (mList != null) mList.onSelectionChange += SaveSelection;
		if (mCheck != null) mCheck.onStateChange += SaveState;
	}

	/// <summary>
	/// Remove the callback.
	/// </summary>

	void OnDestroy ()
	{
		if (mCheck != null) mCheck.onStateChange -= SaveState;
		if (mList != null) mList.onSelectionChange -= SaveSelection;
	}

	/// <summary>
	/// Load and set the state of the togglees.
	/// </summary>

	void OnEnable ()
	{
		if (mList != null)
		{
			string s = PlayerPrefs.GetString(key);
			if (!string.IsNullOrEmpty(s)) mList.selection = s;
			return;
		}

		if (mCheck != null)
		{
			mCheck.isChecked = (PlayerPrefs.GetInt(key, 1) != 0);
		}
		else
		{
			string s = PlayerPrefs.GetString(key);
			UIToggle[] togglees = GetComponentsInChildren<UIToggle>(true);

			for (int i = 0, imax = togglees.Length; i < imax; ++i)
			{
				UIToggle ch = togglees[i];
				ch.isChecked = (ch.name == s);
			}
		}
	}

	/// <summary>
	/// Save the state on destroy.
	/// </summary>

	void OnDisable ()
	{
		if (mCheck == null && mList == null)
		{
			UIToggle[] togglees = GetComponentsInChildren<UIToggle>(true);

			for (int i = 0, imax = togglees.Length; i < imax; ++i)
			{
				UIToggle ch = togglees[i];

				if (ch.isChecked)
				{
					SaveSelection(ch.name);
					break;
				}
			}
		}
	}

	/// <summary>
	/// Save the selection.
	/// </summary>

	void SaveSelection (string selection) { PlayerPrefs.SetString(key, selection); }

	/// <summary>
	/// Save the state.
	/// </summary>

	void SaveState (bool state) { PlayerPrefs.SetInt(key, state ? 1 : 0); }
}
