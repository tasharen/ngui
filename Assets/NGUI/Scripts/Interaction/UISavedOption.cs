//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;

/// <summary>
/// Attach this script to a popup list, the parent of a group of toggles, or to a toggle itself to save its state.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Saved Option")]
public class UISavedOption : MonoBehaviour
{
	[Tooltip("PlayerPrefs-stored key for this option.")]
	public string keyName;

	[Tooltip("By default, NGUITools functions are used for get/set operations. If you want to use PlayerPrefs instead, turn this on.")]
	public bool usePlayerPrefs = false;

	string key { get { return (string.IsNullOrEmpty(keyName)) ? "NGUI State: " + name : keyName; } }

	UIPopupList mList;
	UIToggle mCheck;
	UIProgressBar mSlider;

	/// <summary>
	/// Cache the components and register a listener callback.
	/// </summary>

	void Awake ()
	{
		mList = GetComponent<UIPopupList>();
		mCheck = GetComponent<UIToggle>();
		mSlider = GetComponent<UIProgressBar>();
	}

	/// <summary>
	/// Load and set the state of the toggles.
	/// </summary>

	void OnEnable ()
	{
		if (mList != null)
		{
			EventDelegate.Add(mList.onChange, SaveSelection);
			var s = usePlayerPrefs ? PlayerPrefs.GetString(key) : NGUITools.GetString(key);
			if (!string.IsNullOrEmpty(s)) mList.value = s;
		}
		else if (mCheck != null)
		{
			EventDelegate.Add(mCheck.onChange, SaveState);
			mCheck.value = (usePlayerPrefs ? (PlayerPrefs.GetInt(key, mCheck.startsActive ? 1 : -1) == 1) : (NGUITools.GetInt(key, mCheck.startsActive ? 1 : -1) == 1));
		}
		else if (mSlider != null)
		{
			EventDelegate.Add(mSlider.onChange, SaveProgress);
			mSlider.value = usePlayerPrefs ? PlayerPrefs.GetFloat(key, mSlider.value) : NGUITools.GetFloat(key, mSlider.value);
		}
		else
		{
			var s = usePlayerPrefs ? PlayerPrefs.GetString(key) : NGUITools.GetString(key);
			var toggles = GetComponentsInChildren<UIToggle>(true);

			for (int i = 0, imax = toggles.Length; i < imax; ++i)
			{
				UIToggle ch = toggles[i];
				ch.value = (ch.name == s);
			}
		}
	}

	/// <summary>
	/// Save the state on destroy.
	/// </summary>

	void OnDisable ()
	{
		if (mCheck != null) EventDelegate.Remove(mCheck.onChange, SaveState);
		else if (mList != null) EventDelegate.Remove(mList.onChange, SaveSelection);
		else if (mSlider != null) EventDelegate.Remove(mSlider.onChange, SaveProgress);
		else
		{
			var toggles = GetComponentsInChildren<UIToggle>(true);

			for (int i = 0, imax = toggles.Length; i < imax; ++i)
			{
				var ch = toggles[i];

				if (ch.value)
				{
					if (usePlayerPrefs) PlayerPrefs.SetString(key, ch.name);
					else NGUITools.SetString(key, ch.name);
					break;
				}
			}
		}
	}

	/// <summary>
	/// Save the selection.
	/// </summary>

	public void SaveSelection () { if (usePlayerPrefs) PlayerPrefs.SetString(key, UIPopupList.current.value); else NGUITools.SetString(key, UIPopupList.current.value); }

	/// <summary>
	/// Save the state.
	/// </summary>

	public void SaveState () { if (usePlayerPrefs) PlayerPrefs.SetInt(key, UIToggle.current.value ? 1 : -1); else NGUITools.SetInt(key, UIToggle.current.value ? 1 : -1); }

	/// <summary>
	/// Save the current progress.
	/// </summary>

	public void SaveProgress () { if (usePlayerPrefs) PlayerPrefs.SetFloat(key, UIProgressBar.current.value); else NGUITools.SetFloat(key, UIProgressBar.current.value); }
}
