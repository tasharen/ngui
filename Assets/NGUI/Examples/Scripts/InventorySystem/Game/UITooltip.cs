using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example script that can be used to show tooltips.
/// </summary>

[AddComponentMenu("NGUI/Examples/Tooltip")]
public class UITooltip : MonoBehaviour
{
	static UITooltip mInstance;

	public UILabel text;
	public UISlicedSprite background;
	public bool scalingTransitions = true;

	Transform mTrans;
	float mTarget = 0f;
	float mCurrent = 0f;
	Vector3 mPos;
	Vector3 mSize;

	UIWidget[] mWidgets;

	void Awake () { mInstance = this; }
	void OnDestroy () { mInstance = null; }

	/// <summary>
	/// Get a list of widgets underneath the tooltip.
	/// </summary>

	void Start ()
	{
		mTrans = transform;
		mWidgets = GetComponentsInChildren<UIWidget>();
		mPos = mTrans.localPosition;
		mSize = mTrans.localScale;
		SetAlpha(0f);
	}

	/// <summary>
	/// Update the tooltip's alpha based on the target value.
	/// </summary>

	void Update ()
	{
		if (mCurrent != mTarget)
		{
			mCurrent = Mathf.Lerp(mCurrent, mTarget, Time.deltaTime * 10f);
			if (Mathf.Abs(mCurrent - mTarget) < 0.001f) mCurrent = mTarget;
			SetAlpha(mCurrent * mCurrent);

			if (scalingTransitions)
			{
				Vector3 offset = mSize * 0.25f;
				offset.y = -offset.y;

				Vector3 pos = Vector3.Lerp(mPos - offset, mPos, mCurrent);
				pos = NGUITools.ApplyHalfPixelOffset(pos);

				mTrans.localPosition = pos;
				mTrans.localScale = Vector3.one * (1.5f - mCurrent * 0.5f);
			}
		}
	}

	/// <summary>
	/// Set the alpha of all widgets.
	/// </summary>

	void SetAlpha (float val)
	{
		foreach (UIWidget w in mWidgets)
		{
			Color c = w.color;
			c.a = val;
			w.color = c;
		}
	}

	/// <summary>
	/// Set the tooltip's text to the specified string.
	/// </summary>

	void SetText (string tooltipText)
	{
		if (tooltipText != null)
		{
			mTarget = 1f;
			
			if (text != null) text.text = tooltipText;

			// Orthographic camera positioning is trivial
			mPos = Input.mousePosition;
			mPos.x -= Screen.width * 0.5f;
			mPos.y -= Screen.height * 0.5f;
			mTrans.localPosition = NGUITools.ApplyHalfPixelOffset(mPos);

			// An alternative UIAnchor-based approach that will work even with a non-orthographic camera
			//UIAnchor anchor = mTrans.GetComponent<UIAnchor>();
			//anchor.offset = Input.mousePosition;
			//anchor.Update();

			if (background != null)
			{
				Transform backgroundTrans = background.transform;

				if (text != null && text.font != null)
				{
					Transform textTrans = text.transform;
					Vector3 offset = textTrans.localPosition;
					Vector3 textScale = textTrans.localScale;
					mSize = text.font.CalculatePrintedSize(tooltipText, true);

					mSize.x *= textScale.x;
					mSize.y *= textScale.y;

					mSize.x += offset.x * 2f;
					mSize.y -= offset.y * 2f;
					mSize.z = 1f;

					backgroundTrans.localScale = mSize;
				}
			}
		}
		else mTarget = 0f;
	}

	/// <summary>
	/// Show a tooltip with the specified text.
	/// </summary>

	static public void ShowText (string tooltipText) { if (mInstance != null) mInstance.SetText(tooltipText); }

	/// <summary>
	/// Show a tooltip with the tooltip text for the specified item.
	/// </summary>

	static public void ShowItem (InvGameItem item)
	{
		if (item != null)
		{
			InvBaseItem bi = item.baseItem;

			if (bi != null)
			{
				string t = "[" + NGUITools.EncodeColor(item.color) + "]" + item.name + "[-]\n";
				t += "[AFAFAF]Level " + item.itemLevel + " " + bi.slot;

				List<InvStat> stats = item.CalculateStats();

				foreach (InvStat stat in stats)
				{
					if (stat.amount == 0) continue;

					if (stat.amount < 0)
					{
						t += "\n[FF0000]" + stat.amount;
					}
					else
					{
						t += "\n[00FF00]+" + stat.amount;
					}

					if (stat.modifier == InvStat.Modifier.Percent) t += "%";
					t += " " + stat.id;
					t += "[-]";
				}

				if (!string.IsNullOrEmpty(bi.description)) t += "\n[FF9900]" + bi.description;
				ShowText(t);
				return;
			}
		}
		if (mInstance != null) mInstance.mTarget = 0f;
	}
}