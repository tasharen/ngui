//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Attach this script to a popup list, the parent of a group of checkboxes, or to a checkbox itself to save its state.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Saved Option")]
public class UISavedOption : MonoBehaviour
{
	public string keyName;

	string key { get { return (string.IsNullOrEmpty(keyName)) ? "NGUI State: " + name : keyName; } }

	UIPopupList mList;
	UICheckbox mCheck;

	/// <summary>
	/// Cache the components and register a listener callback.
	/// </summary>

	void Awake ()
	{
		mList = GetComponent<UIPopupList>();
		mCheck = GetComponent<UICheckbox>();
		if (mList != null) mList.onSelectionChange += SaveSelection;
		if (mCheck != null) mCheck.onStateChange += SaveState;
	}

	/// <summary>
	/// Remove the callback.
	/// </summary>

	void OnDestroy () { if (mList != null) mList.onSelectionChange -= SaveSelection; }

	/// <summary>
	/// Load and set the state of the checkboxes.
	/// </summary>

	void OnEnable ()
	{
		string s = PlayerPrefs.GetString(key);

		if (!string.IsNullOrEmpty(s))
		{
			if (mList != null)
			{
				mList.selection = s;
				return;
			}

			if (mCheck != null)
			{
				mCheck.isChecked = (s == "true");
			}
			else
			{
				UICheckbox[] checkboxes = GetComponentsInChildren<UICheckbox>();

				for (int i = 0, imax = checkboxes.Length; i < imax; ++i)
				{
					UICheckbox ch = checkboxes[i];
					ch.isChecked = (ch.name == s);
				}
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
			UICheckbox[] checkboxes = GetComponentsInChildren<UICheckbox>();

			for (int i = 0, imax = checkboxes.Length; i < imax; ++i)
			{
				UICheckbox ch = checkboxes[i];

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

	void SaveState (bool state) { PlayerPrefs.SetString(key, state ? "true" : "false"); }
}