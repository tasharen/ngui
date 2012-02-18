using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Label")]
public class UILabel : UIWidget
{
	[SerializeField] UIFont mFont;
	[SerializeField] string mText = "";
	[SerializeField] int mMaxLineWidth = 0;
	[SerializeField] bool mEncoding = true;
	[SerializeField] bool mMultiline = true;
	[SerializeField] bool mPassword = false;
	[SerializeField] bool mShowLastChar = false;

	/// <summary>
	/// Obsolete, do not use. Use 'mMaxLineWidth' instead.
	/// </summary>

	[SerializeField] float mLineWidth = 0;

	bool mShouldBeProcessed = true;
	string mProcessedText = null;

	// Cached values, used to determine if something has changed and thus must be updated
	Vector3 mLastScale = Vector3.one;
	string mLastText = "";
	int mLastWidth = 0;
	bool mLastEncoding = true;
	bool mLastMulti = true;
	bool mLastPass = false;
	bool mLastShow = false;

	/// <summary>
	/// Function used to determine if something has changed (and thus the geometry must be rebuilt)
	/// </summary>

	bool hasChanged
	{
		get
		{
			return mShouldBeProcessed ||
				mLastText != text ||
				mLastWidth != mMaxLineWidth ||
				mLastEncoding != mEncoding ||
				mLastMulti != mMultiline ||
				mLastPass != mPassword ||
				mLastShow != mShowLastChar;
		}
		set
		{
			if (value)
			{
				mChanged = true;
				mShouldBeProcessed = true;
			}
			else
			{
				mShouldBeProcessed = false;
				mLastText = text;
				mLastWidth = mMaxLineWidth;
				mLastEncoding = mEncoding;
				mLastMulti = mMultiline;
				mLastPass = mPassword;
				mLastShow = mShowLastChar;
			}
		}
	}

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
				hasChanged = true;
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
				hasChanged = true;
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
				hasChanged = true;
				if (value) mPassword = false;
			}
		}
	}

	/// <summary>
	/// Maximum width of the label in pixels.
	/// </summary>

	public int lineWidth
	{
		get
		{
			return mMaxLineWidth;
		}
		set
		{
			if (mMaxLineWidth != value)
			{
				mMaxLineWidth = value;
				hasChanged = true;
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
				hasChanged = true;
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
				hasChanged = true;
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
				hasChanged = true;
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
			if (mLastScale != cachedTransform.localScale)
			{
				mLastScale = cachedTransform.localScale;
				mShouldBeProcessed = true;
			}

			// Process the text if necessary
			if (hasChanged)
			{
				mChanged = true;
				hasChanged = false;
				mLastText = mText;
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
				else if (mMaxLineWidth > 0)
				{
					mProcessedText = mFont.WrapText(mProcessedText, mMaxLineWidth / cachedTransform.localScale.x, mMultiline, mEncoding);
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
	/// Visible size of the widget in local coordinates.
	/// </summary>

	public override Vector2 relativeSize
	{
		get
		{
			Vector3 size = (mFont != null) ? mFont.CalculatePrintedSize(processedText, mEncoding) : Vector2.one;
			float scale = cachedTransform.localScale.x;
			size.x = Mathf.Max(size.x, (mFont != null && scale > 1f) ? lineWidth / scale : 1f);
			size.y = Mathf.Max(size.y, 1f);
			return size;
		}
	}

	/// <summary>
	/// Legacy functionality support.
	/// </summary>

	protected override void OnStart ()
	{
		if (mLineWidth > 0f)
		{
			mMaxLineWidth = Mathf.RoundToInt(mLineWidth);
			mLineWidth = 0f;
		}
	}

	/// <summary>
	/// UILabel needs additional processing when something changes.
	/// </summary>

	public override void MarkAsChanged ()
	{
		hasChanged = true;
		base.MarkAsChanged();
	}

	/// <summary>
	/// Same as MakePixelPerfect(), but only adjusts the position, not the scale.
	/// </summary>

	public void MakePositionPerfect ()
	{
		Vector3 scale = cachedTransform.localScale;

		if (mFont.size == Mathf.RoundToInt(scale.x) && mFont.size == Mathf.RoundToInt(scale.y) &&
			cachedTransform.localRotation == Quaternion.identity)
		{
			Vector2 actualSize = relativeSize * scale.x;

			int x = Mathf.RoundToInt(actualSize.x);
			int y = Mathf.RoundToInt(actualSize.y);

			Vector3 pos = cachedTransform.localPosition;
			pos.x = Mathf.RoundToInt(pos.x);
			pos.y = Mathf.RoundToInt(pos.y);
			pos.z = Mathf.RoundToInt(pos.z);

			if ((x % 2 == 1) && (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom)) pos.x += 0.5f;
			if ((y % 2 == 1) && (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right)) pos.y -= 0.5f;

			if (cachedTransform.localPosition != pos) cachedTransform.localPosition = pos;
		}
	}

	/// <summary>
	/// Text is pixel-perfect when its scale matches the size.
	/// </summary>

	public override void MakePixelPerfect ()
	{
		if (mFont != null)
		{
			Vector3 scale = cachedTransform.localScale;
			scale.x = mFont.size;
			scale.y = scale.x;
			scale.z = 1f;

			Vector2 actualSize = relativeSize * scale.x;

			int x = Mathf.RoundToInt(actualSize.x);
			int y = Mathf.RoundToInt(actualSize.y);

			Vector3 pos = cachedTransform.localPosition;
			pos.x = Mathf.RoundToInt(pos.x);
			pos.y = Mathf.RoundToInt(pos.y);
			pos.z = Mathf.RoundToInt(pos.z);

			if (cachedTransform.localRotation == Quaternion.identity)
			{
				if ((x % 2 == 1) && (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom)) pos.x += 0.5f;
				if ((y % 2 == 1) && (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right)) pos.y -= 0.5f;
			}
			cachedTransform.localPosition = pos;
			cachedTransform.localScale = scale;
		}
		else base.MakePixelPerfect();
	}

	/// <summary>
	/// Draw the label.
	/// </summary>

	public override void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		if (mFont == null) return;
		MakePositionPerfect();
		Pivot p = pivot;

		// Print the text into the buffers
		if (p == Pivot.Left || p == Pivot.TopLeft || p == Pivot.BottomLeft)
		{
			mFont.Print(processedText, color, verts, uvs, cols, mEncoding, UIFont.Alignment.Left, 0);
		}
		else if (p == Pivot.Right || p == Pivot.TopRight || p == Pivot.BottomRight)
		{
			mFont.Print(processedText, color, verts, uvs, cols, mEncoding, UIFont.Alignment.Right,
				(lineWidth > 0) ? lineWidth : Mathf.RoundToInt(relativeSize.x * mFont.size));
		}
		else
		{
			mFont.Print(processedText, color, verts, uvs, cols, mEncoding, UIFont.Alignment.Center,
				(lineWidth > 0) ? lineWidth : Mathf.RoundToInt(relativeSize.x * mFont.size));
		}
	}
}