using UnityEngine;

/// <summary>
/// Attach this script to the parent of a group of checkboxes, or to a checkbox itself to save its state.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Saved Option")]
public class UISavedOption : MonoBehaviour
{
	/// <summary>
	/// Load and set the state of the checkboxes.
	/// </summary>

	void OnEnable ()
	{
		string s = PlayerPrefs.GetString("NGUI State: " + name);
		
		if (!string.IsNullOrEmpty(s))
		{
			UICheckbox[] checkboxes = GetComponentsInChildren<UICheckbox>();

			foreach (UICheckbox ch in checkboxes)
			{
				UIEventListener.Add(ch.gameObject).onClick -= Save;
				ch.isChecked = (ch.name == s);
				UIEventListener.Add(ch.gameObject).onClick += Save;
			}
		}
	}

	/// <summary>
	/// Save the state.
	/// </summary>

	void Save (GameObject go)
	{
		UICheckbox[] checkboxes = GetComponentsInChildren<UICheckbox>();
		
		foreach (UICheckbox ch in checkboxes)
		{
			if (ch.isChecked)
			{
				PlayerPrefs.SetString("NGUI State: " + name, ch.name);
				break;
			}
		}
	}

	/// <summary>
	/// Save the state on destroy.
	/// </summary>

	void OnDestroy () { Save(null); }
}