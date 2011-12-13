using UnityEngine;

/// <summary>
/// Editable text input field.
/// </summary>

[AddComponentMenu("NGUI/UI/Input (Basic)")]
public class UIInput : MonoBehaviour
{
	public UILabel label;
	public int maxChars = 0;
	public string caratChar = "|";

	string mText = "";
	bool mSelected = false;

#if UNITY_IPHONE || UNITY_ANDROID
	iPhoneKeyboard mKeyboard;
#endif

	/// <summary>
	/// Input field's current text value.
	/// </summary>

	public string text
	{
		get
		{
			if (mSelected) return mText;
			return (label != null) ? label.text : "";
		}
		set
		{
			mText = value;

			if (label != null)
			{
				label.supportEncoding = false;
				label.text = mSelected ? value + caratChar : value;
			}
		}
	}

	/// <summary>
	/// Labels used for input shouldn't support color encoding.
	/// </summary>

	void Awake ()
	{
		if (label == null) label = GetComponentInChildren<UILabel>();
		if (label != null)
		{
			label.supportEncoding = false;
			label.multiLine = false;
		}
	}

	/// <summary>
	/// Selection event, sent by UICamera.
	/// </summary>

	void OnSelect (bool selected)
	{
		if (label != null && mSelected != selected && enabled && gameObject.active)
		{
			mSelected = selected;

			if (mSelected)
			{
				mText = label.text;

#if UNITY_IPHONE || UNITY_ANDROID
				if (Application.platform == RuntimePlatform.IPhonePlayer ||
					Application.platform == RuntimePlatform.Android)
				{
					mKeyboard = iPhoneKeyboard.Open(mText);
				}
				else
#endif
				{
					label.text = mText + caratChar;
				}
			}
#if UNITY_IPHONE || UNITY_ANDROID
			else if (mKeyboard != null)
			{
				mKeyboard.active = false;
			}
#endif
			else
			{
				label.text = mText;
			}
		}
	}

#if UNITY_IPHONE || UNITY_ANDROID
	/// <summary>
	/// Update the text and the label by grabbing it from the iOS/Android keyboard.
	/// </summary>

	void Update()
	{
		if (mKeyboard != null)
		{
			mText = mKeyboard.text;
			UpdateLabel();

			if (mKeyboard.done)
			{
				mKeyboard = null;
				mSelected = false;
			}
		}
	}
#endif

	/// <summary>
	/// Input event, sent by UICamera.
	/// </summary>

	void OnInput (string input)
	{
		if (mSelected && enabled && gameObject.active)
		{
			// Mobile devices handle input in Update()
			if (Application.platform == RuntimePlatform.Android) return;
			if (Application.platform == RuntimePlatform.IPhonePlayer) return;

			foreach (char c in input)
			{
				if (c == '\b')
				{
					// Backspace
					if (mText.Length > 0) mText = mText.Substring(0, mText.Length - 1);
				}
				else if (c == '\r' || c == '\n')
				{
					// Enter
					OnSelect(false);
					return;
				}
				else
				{
					// All other characters get appended to the text
					mText += c;
				}
			}

			// Ensure that we don't exceed the maximum length
			UpdateLabel();
		}
	}

	/// <summary>
	/// Update the visual text label, capping it at maxChars correctly.
	/// </summary>

	void UpdateLabel ()
	{
		if (maxChars > 0 && mText.Length > maxChars) mText = mText.Substring(0, maxChars);
		label.text = mSelected ? (mText + caratChar) : mText;
	}
}