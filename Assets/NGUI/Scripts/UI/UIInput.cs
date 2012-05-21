//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Editable text input field.
/// </summary>

[AddComponentMenu("NGUI/UI/Input (Basic)")]
public class UIInput : MonoBehaviour
{
	public delegate char Validator (string currentText, char nextChar);

	public enum KeyboardType
	{
		Default = 0,
		ASCIICapable = 1,
		NumbersAndPunctuation = 2,
		URL = 3,
		NumberPad = 4,
		PhonePad = 5,
		NamePhonePad = 6,
		EmailAddress = 7,
	}

	/// <summary>
	/// Current input, available inside OnSubmit callbacks.
	/// </summary>

	static public UIInput current;

	/// <summary>
	/// Text label modified by this input.
	/// </summary>

	public UILabel label;

	/// <summary>
	/// Maximum number of characters allowed before input no longer works.
	/// </summary>

	public int maxChars = 0;

	/// <summary>
	/// Visual carat character appended to the end of the text when typing.
	/// </summary>

	public string caratChar = "|";

	/// <summary>
	/// Delegate used for validation.
	/// </summary>

	public Validator validator;

	/// <summary>
	/// Type of the touch screen keyboard used on iOS and Android devices.
	/// </summary>

	public KeyboardType type = KeyboardType.Default;

	/// <summary>
	/// Whether this input field should hide its text.
	/// </summary>

	public bool isPassword = false;

	/// <summary>
	/// Color of the label when the input field has focus.
	/// </summary>

	public Color activeColor = Color.white;

	/// <summary>
	/// Event receiver that will be notified when the input field submits its data (enter gets pressed).
	/// </summary>

	public GameObject eventReceiver;

	/// <summary>
	/// Function that will be called on the event receiver when the input field submits its data.
	/// </summary>

	public string functionName = "OnSubmit";

	string mText = "";
	string mDefaultText = "";
	Color mDefaultColor = Color.white;

#if UNITY_IPHONE || UNITY_ANDROID
#if UNITY_3_4
	iPhoneKeyboard mKeyboard;
#else
	TouchScreenKeyboard mKeyboard;
#endif
#else
	string mLastIME = "";
#endif

	/// <summary>
	/// Input field's current text value.
	/// </summary>

	public string text
	{
		get
		{
			return mText;
		}
		set
		{
			mText = value;

			if (label != null)
			{
				if (string.IsNullOrEmpty(value)) value = mDefaultText;

				label.supportEncoding = false;
				label.text = selected ? value + caratChar : value;
				label.showLastPasswordChar = selected;
				label.color = (selected || value != mDefaultText) ? activeColor : mDefaultColor;
			}
		}
	}

	/// <summary>
	/// Whether the input is currently selected.
	/// </summary>

	public bool selected
	{
		get
		{
			return UICamera.selectedObject == gameObject;
		}
		set
		{
			if (!value && UICamera.selectedObject == gameObject) UICamera.selectedObject = null;
			else if (value) UICamera.selectedObject = gameObject;
		}
	}

	/// <summary>
	/// Labels used for input shouldn't support color encoding.
	/// </summary>

	protected void Init ()
	{
		if (label == null) label = GetComponentInChildren<UILabel>();
		if (label != null)
		{
			mDefaultText = label.text;
			mDefaultColor = label.color;
			label.supportEncoding = false;
		}
	}

	/// <summary>
	/// Initialize everything on awake.
	/// </summary>

	void Awake () { Init(); }

	/// <summary>
	/// If the object is currently highlighted, it should also be selected.
	/// </summary>

	void OnEnable () { if (UICamera.IsHighlighted(gameObject)) OnSelect(true); }

	/// <summary>
	/// Remove the selection.
	/// </summary>

	void OnDisable () { if (UICamera.IsHighlighted(gameObject)) OnSelect(false); }

	/// <summary>
	/// Selection event, sent by UICamera.
	/// </summary>

	void OnSelect (bool isSelected)
	{
		if (label != null && enabled && gameObject.active)
		{
			if (isSelected)
			{
				mText = (label.text == mDefaultText) ? "" : label.text;
				label.color = activeColor;
				if (isPassword) label.password = true;

#if UNITY_IPHONE || UNITY_ANDROID
				if (Application.platform == RuntimePlatform.IPhonePlayer ||
					Application.platform == RuntimePlatform.Android)
				{
#if UNITY_3_4
					mKeyboard = iPhoneKeyboard.Open(mText, (iPhoneKeyboardType)((int)type));
#else
					mKeyboard = TouchScreenKeyboard.Open(mText, (TouchScreenKeyboardType)((int)type));
#endif
				}
				else
#endif
				{
					Input.imeCompositionMode = IMECompositionMode.On;
					Transform t = label.cachedTransform;
					Vector3 offset = label.pivotOffset;
					offset.y += label.relativeSize.y;
					offset = t.TransformPoint(offset);
					Input.compositionCursorPos = UICamera.currentCamera.WorldToScreenPoint(offset);
					UpdateLabel();
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
				if (string.IsNullOrEmpty(mText))
				{
					label.text = mDefaultText;
					label.color = mDefaultColor;
					if (isPassword) label.password = false;
				}
				else label.text = mText;

				label.showLastPasswordChar = false;
				Input.imeCompositionMode = IMECompositionMode.Off;
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
			string text = mKeyboard.text;

			if (mText != text)
			{
				mText = "";

				for (int i = 0; i < text.Length; ++i)
				{
					char ch = text[i];
					if (validator != null) ch = validator(mText, ch);
					if (ch != 0) mText += ch;
				}

				if (mText != text) mKeyboard.text = mText;
				UpdateLabel();
			}

			if (mKeyboard.done)
			{
				mKeyboard = null;
				current = this;
				if (eventReceiver == null) eventReceiver = gameObject;
				eventReceiver.SendMessage(functionName, SendMessageOptions.DontRequireReceiver);
				current = null;
				selected = false;
			}
		}
	}
#else
	void Update ()
	{
		if (mLastIME != Input.compositionString)
		{
			mLastIME = Input.compositionString;
			UpdateLabel();
		}
	}
#endif

	/// <summary>
	/// Input event, sent by UICamera.
	/// </summary>

	void OnInput (string input)
	{
		if (selected && enabled && gameObject.active)
		{
			// Mobile devices handle input in Update()
			if (Application.platform == RuntimePlatform.Android) return;
			if (Application.platform == RuntimePlatform.IPhonePlayer) return;

			for (int i = 0, imax = input.Length; i < imax; ++i)
			{
				char c = input[i];

				if (c == '\b')
				{
					// Backspace
					if (mText.Length > 0)
					{
						mText = mText.Substring(0, mText.Length - 1);
						SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (c == '\r' || c == '\n')
				{
					// Enter
					current = this;
					if (eventReceiver == null) eventReceiver = gameObject;
					eventReceiver.SendMessage(functionName, SendMessageOptions.DontRequireReceiver);
					current = null;
					selected = false;
					return;
				}
				else if (c >= ' ')
				{
					// If we have an input validator, validate the input first
					if (validator != null) c = validator(mText, c);

					// If the input is invalid, skip it
					if (c == 0) continue;

					// Append the character and notify the "input changed" listeners.
					mText += c;
					SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
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

		if (label.font != null)
		{
			// Start with the text and append the IME composition and carat chars
			string processed = selected ? (mText + Input.compositionString + caratChar) : mText;

			// Now wrap this text using the specified line width
			label.supportEncoding = false;
			processed = label.font.WrapText(processed, label.lineWidth / label.cachedTransform.localScale.x, true, false, UIFont.SymbolStyle.None);

			if (!label.multiLine)
			{
				// Split it up into lines
				string[] lines = processed.Split(new char[] { '\n' });

				// Only the last line should be visible
				processed = (lines.Length > 0) ? lines[lines.Length - 1] : "";
			}
			// Update the label's visible text
			label.text = processed;
			label.showLastPasswordChar = selected;
		}
	}
}