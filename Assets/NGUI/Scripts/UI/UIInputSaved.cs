using UnityEngine;

/// <summary>
/// Editable text input field that automatically saves its data to PlayerPrefs.
/// </summary>

[AddComponentMenu("NGUI/UI/Input (Saved)")]
public class UIInputSaved : UIInput
{
	public string playerPrefsField;

	void Start ()
	{
		Init();

		if (!string.IsNullOrEmpty(playerPrefsField) && PlayerPrefs.HasKey(playerPrefsField))
		{
			text = PlayerPrefs.GetString(playerPrefsField);
		}
	}

	void OnApplicationQuit ()
	{
		if (!string.IsNullOrEmpty(playerPrefsField))
		{
			PlayerPrefs.SetString(playerPrefsField, text);
		}
	}
}