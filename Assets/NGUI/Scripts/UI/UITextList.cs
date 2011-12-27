using UnityEngine;
using System.Collections.Generic;

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

	public Style style = Style.Text;
	public UILabel textLabel;
	public float maxWidth = 0f;
	public float maxHeight = 0f;
	public int maxEntries = 50;

	// Text list is made up of paragraphs
	class Paragraph
	{
		public string text;		// Original text
		public string[] lines;	// Split lines
	}

	char[] mSeparator = new char[] { '\n' };
	List<Paragraph> mParagraphs = new List<Paragraph>();
	float mScroll = 0f;
	bool mSelected = false;
	int mTotalLines = 0;

	/// <summary>
	/// Add a new paragraph.
	/// </summary>

	public void Add (string text) { Add(text, true); }

	/// <summary>
	/// Add a new paragraph.
	/// </summary>

	void Add (string text, bool updateVisible)
	{
		Paragraph ce = null;

		if (mParagraphs.Count < maxEntries)
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
		
		if (textLabel != null && textLabel.font != null)
		{
			// Rebuild the line
			ce.lines = textLabel.font.WrapText(ce.text, maxWidth / textLabel.transform.localScale.y, true, true).Split(mSeparator);

			// Recalculate the total number of lines
			mTotalLines = 0;
			foreach (Paragraph p in mParagraphs) mTotalLines += p.lines.Length;
		}

		// Update the visible text
		if (updateVisible) UpdateVisibleText();
	}

	/// <summary>
	/// Automatically find the values if none were specified.
	/// </summary>

	void Awake ()
	{
		if (textLabel == null) textLabel = GetComponentInChildren<UILabel>();
		if (textLabel != null) textLabel.lineWidth = 0f;

		Collider col = collider;

		if (col != null)
		{
			// Automatically set the width and height based on the collider
			if (maxHeight <= 0f) maxHeight = col.bounds.size.y / transform.lossyScale.y;
			if (maxWidth  <= 0f) maxWidth  = col.bounds.size.x / transform.lossyScale.x;
		}
	}

	/// <summary>
	/// Scrolling support.
	/// </summary>

	void Update ()
	{
		if (mSelected)
		{
			float scroll = Input.GetAxis("Mouse ScrollWheel");
			
			if (scroll != 0f)
			{
				scroll *= (style == Style.Chat) ? 10f : -10f;
				mScroll = Mathf.Max(0f, mScroll + scroll);
				UpdateVisibleText();
			}
		}
	}

	/// <summary>
	/// Remember whether the widget is selected.
	/// </summary>

	void OnSelect (bool selected)
	{
		mSelected = selected;
	}

	/// <summary>
	/// Refill the text label based on what's currently visible.
	/// </summary>

	void UpdateVisibleText ()
	{
		if (textLabel != null)
		{
			UIFont font = textLabel.font;

			if (font != null)
			{
				int lines = 0;
				int maxLines = maxHeight > 0 ? Mathf.FloorToInt(maxHeight / font.size) : 100000;
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

				string final = "";

				foreach (Paragraph p in mParagraphs)
				{
					foreach (string s in p.lines)
					{
						if (offset > 0)
						{
							--offset;
						}
						else
						{
							if (final.Length > 0) final += "\n";
							final += s;
							++lines;
							if (lines >= maxLines) break;
						}
					}
					if (lines >= maxLines) break;
				}
				textLabel.text = final;
			}
		}
	}
}