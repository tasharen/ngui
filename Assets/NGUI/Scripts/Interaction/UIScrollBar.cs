//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Scroll bar functionality.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/NGUI Scroll Bar")]
public class UIScrollBar : UIProgressBar
{
	enum Direction
	{
		Horizontal,
		Vertical,
		Upgraded,
	}

	// Size of the scroll bar
	[HideInInspector][SerializeField] protected float mSize = 1f;

	// Deprecated functionality
	[HideInInspector][SerializeField] float mScroll = 0f;
	[HideInInspector][SerializeField] Direction mDir = Direction.Upgraded;
	[HideInInspector][SerializeField] bool mInverted = false;

	[System.Obsolete("Use 'value' instead")]
	public float scrollValue { get { return this.value; } set { this.value = value; } }
	
	[System.Obsolete("Use 'fillDirection' instead")]
	public bool inverted { get { return isInverted; } set { } }

	/// <summary>
	/// The size of the foreground bar in percent (0-1 range).
	/// </summary>

	public float barSize
	{
		get
		{
			return mSize;
		}
		set
		{
			float val = Mathf.Clamp01(value);

			if (mSize != val)
			{
				mSize = val;
				mIsDirty = true;

				if (onChange != null)
				{
					current = this;
					EventDelegate.Execute(onChange);
					current = null;
				}
			}
		}
	}

	/// <summary>
	/// Upgrade from legacy functionality.
	/// </summary>

	protected override void Upgrade ()
	{
		if (mDir != Direction.Upgraded)
		{
			mValue = mScroll;

			if (mDir == Direction.Horizontal)
			{
				mFill = mInverted ? FillDirection.RightToLeft : FillDirection.LeftToRight;
			}
			else
			{
				mFill = mInverted ? FillDirection.BottomToTop : FillDirection.TopToBottom;
			}
			mDir = Direction.Upgraded;
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}
	}

	/// <summary>
	/// Make the scroll bar's foreground react to press events.
	/// </summary>

	protected override void OnStart ()
	{
		if (mFG != null && mFG.collider != null && mFG.gameObject != gameObject)
		{
			UIEventListener fgl = UIEventListener.Get(mFG.gameObject);
			fgl.onPress += OnPressForeground;
			fgl.onDrag += OnDragForeground;
			mFG.autoResizeBoxCollider = true;
		}
	}

	/// <summary>
	/// Move the scroll bar to be centered on the specified position.
	/// </summary>

	protected override void CenterOnPos (Vector2 localPos)
	{
		if (mFG == null) return;

		if (isHorizontal)
		{
			float range = (mStartingSize.x - mFG.width);
			float min = mStartingPos.x - range * 0.5f;
			float val = (localPos.x - min) / range;
			value = Mathf.Clamp01((isInverted ? 1f - val : val));
		}
		else
		{
			float range = (mStartingSize.y - mFG.height);
			float min = mStartingPos.y - range * 0.5f;
			float val = (localPos.y - min) / range;
			value = Mathf.Clamp01((isInverted ? 1f - val : val));
		}
	}

	/// <summary>
	/// Update the value of the scroll bar.
	/// </summary>

	public override void ForceUpdate ()
	{
		mIsDirty = false;

		if (mFG != null)
		{
			mSize = Mathf.Clamp01(mSize);
			float val = isInverted ? 1f - value : value;
			Vector3 pos = mStartingPos;

			if (isHorizontal)
			{
				int size = Mathf.RoundToInt(mStartingSize.x * mSize);
				mFG.width = ((size & 1) == 1) ? size + 1 : size;
				float diff = (mStartingSize.x - mFG.width) * 0.5f;
				pos.x = Mathf.Round(Mathf.Lerp(pos.x - diff, pos.x + diff, val));
			}
			else
			{
				int size = Mathf.RoundToInt(mStartingSize.y * mSize);
				mFG.height = ((size & 1) == 1) ? size + 1 : size;
				float diff = (mStartingSize.y - mFG.height) * 0.5f;
				pos.y = Mathf.Round(Mathf.Lerp(pos.y - diff, pos.y + diff, val));
			}
			mFG.cachedTransform.localPosition = pos;
		}
	}
}
