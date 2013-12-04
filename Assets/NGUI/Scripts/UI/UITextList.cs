//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Text list can be used with a UILabel to create a scrollable multi-line text field that's
/// easy to add new entries to. Optimal use: chat window.
/// </summary>

[AddComponentMenu("NGUI/UI/Text List")]
public class UITextList : MonoBehaviour
{
	public enum Style
	{
		Text,
		Chat,
	}

	/// <summary>
	/// Label the contents of which will be modified with the chat entries.
	/// </summary>

	public UILabel textLabel;

	/// <summary>
	/// Text style. Text entries go top to bottom. Chat entries go bottom to top.
	/// </summary>

	public Style style = Style.Text;

	/// <summary>
	/// Maximum number of chat log entries to keep before discarding them.
	/// </summary>

	public int history = 50;

	// Text list is made up of paragraphs
	protected class Paragraph
	{
		public string text;		// Original text
		public string[] lines;	// Split lines
	}

	protected char[] mSeparator = new char[] { '\n' };
	protected BetterList<Paragraph> mParagraphs = new BetterList<Paragraph>();
	protected float mScroll = 0f;
	protected int mTotalLines = 0;
	protected int mLastWidth = 0;
	protected int mLastHeight = 0;

	/// <summary>
	/// Whether the text list is usable.
	/// </summary>

	bool isValid { get { return textLabel != null && textLabel.ambigiousFont != null; } }

	/// <summary>
	/// Clear the text.
	/// </summary>

	public void Clear ()
	{
		mParagraphs.Clear();
		UpdateVisibleText();
	}

	/// <summary>
	/// Automatically find the values if none were specified.
	/// </summary>

	void Awake ()
	{
		if (textLabel == null)
			textLabel = GetComponentInChildren<UILabel>();
	}

	/// <summary>
	/// Keep an eye on the size of the label, and if it changes -- rebuild everything.
	/// </summary>

	void Update ()
	{
		if (isValid)
		{
			if (textLabel.width != mLastWidth || textLabel.height != mLastHeight)
			{
				mLastWidth = textLabel.width;
				mLastHeight = textLabel.height;
				Rebuild();
			}
		}
	}

	/// <summary>
	/// Allow scrolling of the text list.
	/// </summary>

	void OnScroll (float val)
	{
		if (textLabel != null && textLabel.ambigiousFont != null)
		{
			val *= (style == Style.Chat) ? 10f : -10f;
			mScroll = Mathf.Max(0f, mScroll + val);
			UpdateVisibleText();
		}
	}

	/// <summary>
	/// Allow dragging of the text list.
	/// </summary>

	void OnDrag (Vector2 delta)
	{
		if (textLabel != null && textLabel.ambigiousFont != null)
		{
			float val = delta.y * ((style == Style.Chat) ? -1f / textLabel.fontSize : 1f / textLabel.fontSize);
			mScroll = Mathf.Max(0f, mScroll + val);
			UpdateVisibleText();
		}
	}

	/// <summary>
	/// Add a new paragraph.
	/// </summary>

	public void Add (string text) { Add(text, true); }

	/// <summary>
	/// Add a new paragraph.
	/// </summary>

	protected void Add (string text, bool updateVisible)
	{
		Paragraph ce = null;

		if (mParagraphs.size < history)
		{
			ce = new Paragraph();
		}
		else
		{
			ce = mParagraphs[0];
			mParagraphs.RemoveAt(0);
		}

		ce.text = text;
		mParagraphs.Add(ce);
		Rebuild();
	}

	/// <summary>
	/// Rebuild the visible text.
	/// </summary>

	protected void Rebuild ()
	{
		if (isValid)
		{
			UIFont bitmapFont = textLabel.bitmapFont;

			// TODO: Change this to alignment when that's implemented
			textLabel.pivot = (style == Style.Chat) ? UIWidget.Pivot.BottomLeft : UIWidget.Pivot.TopLeft;
			textLabel.overflowMethod = UILabel.Overflow.ClampContent;
			textLabel.UpdateNGUIText();
			NGUIText.current.lineHeight = 1000000;

			mTotalLines = 0;

			for (int i = 0; i < mParagraphs.size; ++i)
			{
				string final;
				Paragraph p = mParagraphs.buffer[i];

				if (bitmapFont != null)
				{
					if (!bitmapFont.WrapText(p.text, out final)) continue;
					p.lines = final.Split('\n');
					mTotalLines += p.lines.Length;
				}
			}

			// Recalculate the total number of lines
			mTotalLines = 0;
			for (int i = 0, imax = mParagraphs.size; i < imax; ++i)
				mTotalLines += mParagraphs.buffer[i].lines.Length;

			// Update the visible text
			UpdateVisibleText();
		}
	}

	/// <summary>
	/// Refill the text label based on what's currently visible.
	/// </summary>

	protected void UpdateVisibleText ()
	{
		if (textLabel != null && textLabel.ambigiousFont != null)
		{
			int lines = 0;
			int maxLines = Mathf.FloorToInt((float)textLabel.height / textLabel.fontSize);
			int offset = Mathf.RoundToInt(mScroll);

			// Don't let scrolling to exceed the visible number of lines
			if (maxLines + offset > mTotalLines)
			{
				offset = Mathf.Max(0, mTotalLines - maxLines);
				mScroll = offset;
			}

			if (style == Style.Chat)
			{
				offset = Mathf.Max(0, mTotalLines - maxLines - offset);
			}

			StringBuilder final = new StringBuilder();

			for (int i = 0, imax = mParagraphs.size; i < imax; ++i)
			{
				Paragraph p = mParagraphs.buffer[i];

				for (int b = 0, bmax = p.lines.Length; b < bmax; ++b)
				{
					string s = p.lines[b];

					if (offset > 0)
					{
						--offset;
					}
					else
					{
						if (final.Length > 0) final.Append("\n");
						final.Append(s);
						++lines;
						if (lines >= maxLines) break;
					}
				}
				if (lines >= maxLines) break;
			}
			textLabel.text = final.ToString();
		}
	}
}
