//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Label")]
public class UILabel : UIWidget
{
	public enum Effect
	{
		None,
		Shadow,
		Outline,
	}

	[HideInInspector][SerializeField] UIFont mFont;
	[HideInInspector][SerializeField] string mText = "";
	[HideInInspector][SerializeField] int mMaxLineWidth = 0;
	[HideInInspector][SerializeField] bool mEncoding = true;
	[HideInInspector][SerializeField] int mMaxLineCount = 0; // 0 denotes unlimited
	[HideInInspector][SerializeField] bool mPassword = false;
	[HideInInspector][SerializeField] bool mShowLastChar = false;
	[HideInInspector][SerializeField] Effect mEffectStyle = Effect.None;
	[HideInInspector][SerializeField] Color mEffectColor = Color.black;
	[HideInInspector][SerializeField] UIFont.SymbolStyle mSymbols = UIFont.SymbolStyle.Uncolored;
	[HideInInspector][SerializeField] Vector2 mEffectDistance = Vector2.one;

	/// <summary>
	/// Obsolete, do not use. Use 'mMaxLineWidth' instead.
	/// </summary>

	[HideInInspector][SerializeField] float mLineWidth = 0;

	/// <summary>
	/// Obsolete, do not use. Use 'mMaxLineCount' instead
	/// </summary>

	[HideInInspector][SerializeField]bool mMultiline = true;

	bool mShouldBeProcessed = true;
	string mProcessedText = null;

	// Cached values, used to determine if something has changed and thus must be updated
	Vector3 mLastScale = Vector3.one;
	string mLastText = "";
	int mLastWidth = 0;
	bool mLastEncoding = true;
	int mLastCount = 0;
	bool mLastPass = false;
	bool mLastShow = false;
	Effect mLastEffect = Effect.None;
	Vector3 mSize = Vector3.zero;

	/// <summary>
	/// Function used to determine if something has changed (and thus the geometry must be rebuilt)
	/// </summary>

	bool hasChanged
	{
		get
		{
			return mShouldBeProcessed ||
				mLastText		!= text ||
				mLastWidth		!= mMaxLineWidth ||
				mLastEncoding	!= mEncoding ||
				mLastCount		!= mMaxLineCount ||
				mLastPass		!= mPassword ||
				mLastShow		!= mShowLastChar ||
				mLastEffect		!= mEffectStyle;
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
				mShouldBeProcessed	= false;
				mLastText			= text;
				mLastWidth			= mMaxLineWidth;
				mLastEncoding		= mEncoding;
				mLastCount			= mMaxLineCount;
				mLastPass			= mPassword;
				mLastShow			= mShowLastChar;
				mLastEffect			= mEffectStyle;
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
	/// Style used for symbols.
	/// </summary>

	public UIFont.SymbolStyle symbolStyle
	{
		get
		{
			return mSymbols;
		}
		set
		{
			if (mSymbols != value)
			{
				mSymbols = value;
				hasChanged = true;
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
			return mMaxLineCount != 1;
		}
		set
		{
			if ((mMaxLineCount != 1) != value)
			{
				mMaxLineCount = (value ? 0 : 1);
				hasChanged = true;
				if (value) mPassword = false;
			}
		}
	}

	/// <summary>
	/// The max number of lines to be displayed for the label
	/// </summary>

	public int maxLineCount
	{
		get
		{
			return mMaxLineCount;
		}
		set
		{
			if (mMaxLineCount != value)
			{
				mMaxLineCount = Mathf.Max(value, 0);
				hasChanged = true;
				if (value == 1) mPassword = false;
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
				mPassword		= value;
				mMaxLineCount	= 1;
				mEncoding		= false;
				hasChanged		= true;
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
	/// What effect is used by the label.
	/// </summary>

	public Effect effectStyle
	{
		get
		{
			return mEffectStyle;
		}
		set
		{
			if (mEffectStyle != value)
			{
				mEffectStyle = value;
				hasChanged = true;
			}
		}
	}

	/// <summary>
	/// Color used by the effect, if it's enabled.
	/// </summary>

	public Color effectColor
	{
		get
		{
			return mEffectColor;
		}
		set
		{
			if (!mEffectColor.Equals(value))
			{
				mEffectColor = value;
				if (mEffectStyle != Effect.None) hasChanged = true;
			}
		}
	}

	/// <summary>
	/// Effect distance in pixels.
	/// </summary>

	public Vector2 effectDistance
	{
		get
		{
			return mEffectDistance;
		}
		set
		{
			if (mEffectDistance != value)
			{
				mEffectDistance = value;
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
			if (hasChanged) ProcessText();
			return mProcessedText;
		}
	}

	/// <summary>
	/// Retrieve the material used by the font.
	/// </summary>

	public override Material material
	{
		get
		{
			Material mat = base.material;

			if (mat == null)
			{
				mat = (mFont != null) ? mFont.material : null;
				material = mat;
			}
			return mat;
		}
	}

	/// <summary>
	/// Visible size of the widget in local coordinates.
	/// </summary>

	public override Vector2 relativeSize
	{
		get
		{
			if (mFont == null) return Vector3.one;
			if (hasChanged) ProcessText();
			return mSize;
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

		if (!mMultiline)
		{
			mMaxLineCount = 1;
			mMultiline = true;
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
	/// Process the raw text, called when something changes.
	/// </summary>

	void ProcessText ()
	{
		mChanged = true;
		hasChanged = false;
		mLastText = mText;
		mProcessedText = mText.Replace("\\n", "\n");

		if (mPassword)
		{
			mProcessedText = mFont.WrapText(mProcessedText, 100000f, 1, false, UIFont.SymbolStyle.None);

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
			mProcessedText = mFont.WrapText(mProcessedText, mMaxLineWidth / cachedTransform.localScale.x, mMaxLineCount, mEncoding, mSymbols);
		}
		else if (mMaxLineCount > 0)
		{
			mProcessedText = mFont.WrapText(mProcessedText, 100000f, mMaxLineCount, mEncoding, mSymbols);
		}

		mSize = !string.IsNullOrEmpty(mProcessedText) ? mFont.CalculatePrintedSize(mProcessedText, mEncoding, mSymbols) : Vector2.one;
		float scale = cachedTransform.localScale.x;
		mSize.x = Mathf.Max(mSize.x, (mFont != null && scale > 1f) ? lineWidth / scale : 1f);
		mSize.y = Mathf.Max(mSize.y, 1f);
	}

	/// <summary>
	/// Same as MakePixelPerfect(), but only adjusts the position, not the scale.
	/// </summary>

	public void MakePositionPerfect ()
	{
		float pixelSize = (font.atlas != null) ? font.atlas.pixelSize : 1f;
		Vector3 scale = cachedTransform.localScale;

		if (mFont.size == Mathf.RoundToInt(scale.x / pixelSize) &&
			mFont.size == Mathf.RoundToInt(scale.y / pixelSize) &&
			cachedTransform.localRotation == Quaternion.identity)
		{
			Vector2 actualSize = relativeSize * scale.x;

			int x = Mathf.RoundToInt(actualSize.x / pixelSize);
			int y = Mathf.RoundToInt(actualSize.y / pixelSize);

			Vector3 pos = cachedTransform.localPosition;
			pos.x = Mathf.FloorToInt(pos.x / pixelSize);
			pos.y = Mathf.CeilToInt(pos.y / pixelSize);
			pos.z = Mathf.RoundToInt(pos.z);

			if ((x % 2 == 1) && (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom)) pos.x += 0.5f;
			if ((y % 2 == 1) && (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right)) pos.y -= 0.5f;

			pos.x *= pixelSize;
			pos.y *= pixelSize;

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
			float pixelSize = (font.atlas != null) ? font.atlas.pixelSize : 1f;

			Vector3 scale = cachedTransform.localScale;
			scale.x = mFont.size * pixelSize;
			scale.y = scale.x;
			scale.z = 1f;

			Vector2 actualSize = relativeSize * scale.x;

			int x = Mathf.RoundToInt(actualSize.x / pixelSize);
			int y = Mathf.RoundToInt(actualSize.y / pixelSize);

			Vector3 pos = cachedTransform.localPosition;
			pos.x = Mathf.FloorToInt(pos.x / pixelSize);
			pos.y = Mathf.CeilToInt(pos.y / pixelSize);
			pos.z = Mathf.RoundToInt(pos.z);

			if (cachedTransform.localRotation == Quaternion.identity)
			{
				if ((x % 2 == 1) && (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom)) pos.x += 0.5f;
				if ((y % 2 == 1) && (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right)) pos.y -= 0.5f;
			}

			pos.x *= pixelSize;
			pos.y *= pixelSize;

			cachedTransform.localPosition = pos;
			cachedTransform.localScale = scale;
		}
		else base.MakePixelPerfect();
	}

	/// <summary>
	/// Apply a shadow effect to the buffer.
	/// </summary>

#if UNITY_3_5_4
	void ApplyShadow (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, int start, int end, float x, float y)
#else
	void ApplyShadow (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols, int start, int end, float x, float y)
#endif
	{
#if UNITY_3_5_4
		Color c = mEffectColor;
		c.a = c.a * color.a;
		Color col = c;
#else
		Color c = mEffectColor;
		c.a = c.a * color.a;
		Color32 col = c;
#endif
		for (int i = start; i < end; ++i)
		{
			verts.Add(verts.buffer[i]);
			uvs.Add(uvs.buffer[i]);
			cols.Add(cols.buffer[i]);

			Vector3 v = verts.buffer[i];
			v.x += x;
			v.y += y;
			verts.buffer[i] = v;
			cols.buffer[i] = col;
		}
	}

	/// <summary>
	/// Draw the label.
	/// </summary>

#if UNITY_3_5_4
	public override void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
#else
	public override void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
#endif
	{
		if (mFont == null) return;
		MakePositionPerfect();
		Pivot p = pivot;
		int offset = verts.size;

		// Print the text into the buffers
		if (p == Pivot.Left || p == Pivot.TopLeft || p == Pivot.BottomLeft)
		{
			mFont.Print(processedText, color, verts, uvs, cols, mEncoding, mSymbols, UIFont.Alignment.Left, 0);
		}
		else if (p == Pivot.Right || p == Pivot.TopRight || p == Pivot.BottomRight)
		{
			mFont.Print(processedText, color, verts, uvs, cols, mEncoding, mSymbols, UIFont.Alignment.Right,
				Mathf.RoundToInt(relativeSize.x * mFont.size));
		}
		else
		{
			mFont.Print(processedText, color, verts, uvs, cols, mEncoding, mSymbols, UIFont.Alignment.Center,
				Mathf.RoundToInt(relativeSize.x * mFont.size));
		}

		// Apply an effect if one was requested
		if (effectStyle != Effect.None)
		{
			Vector3 scale = cachedTransform.localScale;
			if (scale.x == 0f || scale.y == 0f) return;

			int end = verts.size;
			float pixel =  1f / mFont.size;

			float fx = pixel * mEffectDistance.x;
			float fy = pixel * mEffectDistance.y;

			ApplyShadow(verts, uvs, cols, offset, end, fx, -fy);

			if (effectStyle == Effect.Outline)
			{
				offset = end;
				end = verts.size;

				ApplyShadow(verts, uvs, cols, offset, end, -fx, fy);

				offset = end;
				end = verts.size;

				ApplyShadow(verts, uvs, cols, offset, end, fx, fy);

				offset = end;
				end = verts.size;

				ApplyShadow(verts, uvs, cols, offset, end, -fx, -fy);
			}
		}
	}
}