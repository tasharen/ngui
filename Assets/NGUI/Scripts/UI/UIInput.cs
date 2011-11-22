using UnityEngine;

/// <summary>
/// Editable text input field.
/// </summary>

[AddComponentMenu("NGUI/UI/Input (Basic)")]
public class UIInput : MonoBehaviour
{
	public TextMesh textMesh;

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
			return (textMesh != null) ? textMesh.text : "";
		}
		set
		{
			mText = value;
			if (textMesh != null) textMesh.text = mSelected ? value + "|" : value;
		}
	}

	void OnSelect (bool selected)
	{
		if (textMesh != null && mSelected != selected && enabled && gameObject.active)
		{
			mSelected = selected;

			if (mSelected)
			{
				mText = textMesh.text;

#if UNITY_IPHONE || UNITY_ANDROID
				if (Application.platform == RuntimePlatform.IPhonePlayer ||
					Application.platform == RuntimePlatform.Android)
				{
					mKeyboard = iPhoneKeyboard.Open(mText);
				}
				else
#endif
				{
					textMesh.text = mText + "|";
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
				textMesh.text = mText;
			}
		}
	}

	void OnInput (string input)
	{
		if (mSelected && enabled && gameObject.active)
		{
#if UNITY_IPHONE || UNITY_ANDROID
			if (mKeyboard != null && mKeyboard.done)
			{
				mSelected = false;
				mKeyboard = null;
				mText = "";
			}
#endif
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
			textMesh.text = mSelected ? (mText + "|") : mText;
		}
	}
}