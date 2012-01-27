using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Label")]
public class UILabel : UIWidget
{
#if UNITY_FLASH // Unity 3.5b6 is bugged when SerializeField is mixed with prefabs (after LoadLevel)
	public UIFont mFont;
	public string mText = "";
	public bool mEncoding = true;
	public float mLineWidth = 0;
	public bool mMultiline = true;
	public bool mPassword = false;
	public bool mShowLastChar = false;
#else
	[SerializeField] UIFont mFont;
	[SerializeField] string mText = "";
	[SerializeField] bool mEncoding = true;
	[SerializeField] float mLineWidth = 0;
	[SerializeField] bool mMultiline = true;
	[SerializeField] bool mPassword = false;
	[SerializeField] bool mShowLastChar = false;
#endif

	bool mShouldBeProcessed = true;
	string mProcessedText = null;
	float mLastSize = 0f;

	/// <summary>
	/// Set the font used by this label.
	/// </summary>

	public UIFont font
	{
		get
		{
			return mFont;
		}
		set
		{
			if (mFont != value)
			{
				mFont = value;
				material = (mFont != null) ? mFont.material : null;
				mChanged = true;
				mShouldBeProcessed = true;
				MarkAsChanged();
			}
		}
	}

	/// <summary>
	/// Text that's being displayed by the label.
	/// </summary>

	public string text
	{
		get
		{
			return mText;
		}
		set
		{
			if (value != null && mText != value)
			{
				mText = value;
				mChanged = true;
				mShouldBeProcessed = true;
			}
		}
	}

	/// <summary>
	/// Whether this label will support color encoding in the format of [RRGGBB] and new line in the form of a "\\n" string.
	/// </summary>

	public bool supportEncoding
	{
		get
		{
			return mEncoding;
		}
		set
		{
			if (mEncoding != value)
			{
				mEncoding = value;
				mChanged = true;
				mShouldBeProcessed = true;
				if (value) mPassword = false;
			}
		}
	}

	/// <summary>
	/// Maximum width of the label in pixels.
	/// </summary>

	public float lineWidth
	{
		get
		{
			return mLineWidth;
		}
		set
		{
			if (mLineWidth != value)
			{
				mLineWidth = value;
				mChanged = true;
				mShouldBeProcessed = true;
			}
		}
	}

	/// <summary>
	/// Whether the label supports multiple lines.
	/// </summary>

	public bool multiLine
	{
		get
		{
			return mMultiline;
		}
		set
		{
			if (mMultiline != value)
			{
				mMultiline = value;
				mChanged = true;
				mShouldBeProcessed = true;
				if (value) mPassword = false;
			}
		}
	}

	/// <summary>
	/// Whether the label's contents should be hidden
	/// </summary>

	public bool password
	{
		get
		{
			return mPassword;
		}
		set
		{
			if (mPassword != value)
			{
				mPassword = value;
				mMultiline = false;
				mEncoding = false;
				mChanged = true;
				mShouldBeProcessed = true;
			}
		}
	}

	/// <summary>
	/// Whether the last character of a password field will be shown
	/// </summary>

	public bool showLastPasswordChar
	{
		get
		{
			return mShowLastChar;
		}
		set
		{
			if (mShowLastChar != value)
			{
				mShowLastChar = value;
				mChanged = true;
				mShouldBeProcessed = true;
			}
		}
	}

	/// <summary>
	/// Returns the processed version of 'text', with new line characters, line wrapping, etc.
	/// </summary>

	public string processedText
	{
		get
		{
			// If the height changes, we should re-process the text
			if (mLineWidth > 0f)
			{
				float size = cachedTransform.localScale.y;

				if (mLastSize != size)
				{
					mLastSize = size;
					mShouldBeProcessed = true;
				}
			}

			// Process the text if necessary
			if (mShouldBeProcessed)
			{
				mShouldBeProcessed = false;
				mProcessedText = mText.Replace("\\n", "\n");

				if (mPassword)
				{
					mProcessedText = mFont.WrapText(mProcessedText, 100000f, false, false);

					string hidden = "";

					if (mShowLastChar)
					{
						for (int i = 1, imax = mProcessedText.Length; i < imax; ++i) hidden += "*";
						if (mProcessedText.Length > 0) hidden += mProcessedText[mProcessedText.Length - 1];
					}
					else
					{
						for (int i = 0, imax = mProcessedText.Length; i < imax; ++i) hidden += "*";
					}
					mProcessedText = hidden;
				}
				else if (mLineWidth > 0f)
				{
					mProcessedText = mFont.WrapText(mProcessedText, mLineWidth / cachedTransform.localScale.y, mMultiline, mEncoding);
				}
				else if (!mMultiline)
				{
					mProcessedText = mFont.WrapText(mProcessedText, 100000f, false, mEncoding);
				}
			}
			return mProcessedText;
		}
	}

	/// <summary>
	/// UILabel needs additional processing when something changes.
	/// </summary>

	public override void MarkAsChanged ()
	{
		mShouldBeProcessed = true;
		base.MarkAsChanged();
	}

	/// <summary>
	/// Text is pixel-perfect when its scale matches the size.
	/// </summary>

	public override void MakePixelPerfect ()
	{
		if (mFont != null)
		{
			Vector3 pos = cachedTransform.localPosition;
			pos.x = Mathf.RoundToInt(pos.x);
			pos.y = Mathf.RoundToInt(pos.y);
			pos.z = Mathf.RoundToInt(pos.z);

			if (mFont.size % 2 == 1)
			{
				if (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom) pos.x += 0.5f;
				if (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right) pos.y -= 0.5f;
			}
			cachedTransform.localPosition = pos;

			Vector3 scale = cachedTransform.localScale;
			scale.x = mFont.size;
			scale.y = scale.x;
			scale.z = 1f;
			cachedTransform.localScale = scale;
		}
		else base.MakePixelPerfect();
	}

	/// <summary>
	/// Visible size of the widget in local coordinates.
	/// </summary>

	public override Vector2 relativeSize
	{
		get
		{
			Vector3 size = (mFont != null) ? mFont.CalculatePrintedSize(processedText, mEncoding) : Vector2.zero;
			if (mFont.size > 0) size.x = Mathf.Max(size.x, lineWidth / mFont.size);
			return size;
		}
	}

	/// <summary>
	/// Draw the label.
	/// </summary>

	public override void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
#if !UNITY_FLASH
		// Unity 3.5b6 is bugged as of 3.5b6 and evaluates null checks to 'true' after Application.LoadLevel
		if (mFont == null) return;
#endif
		Pivot p = pivot;

		// Print the text into the buffers
		if (p == Pivot.Left || p == Pivot.TopLeft || p == Pivot.BottomLeft)
		{
			mFont.Print(processedText, color, verts, uvs, cols, mEncoding, UIFont.Alignment.Left, 0);
		}
		else if (p == Pivot.Right || p == Pivot.TopRight || p == Pivot.BottomRight)
		{
			mFont.Print(processedText, color, verts, uvs, cols, mEncoding, UIFont.Alignment.Right,
				Mathf.RoundToInt((lineWidth > 0f) ? lineWidth : relativeSize.x * mFont.size));
		}
		else
		{
			mFont.Print(processedText, color, verts, uvs, cols, mEncoding, UIFont.Alignment.Center,
				Mathf.RoundToInt((lineWidth > 0f) ? lineWidth : relativeSize.x * mFont.size));
		}
	}
}