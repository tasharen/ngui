//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Extended progress bar that has a draggable thumb.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/NGUI Slider")]
public class UISlider : UIProgressBar
{
	enum Direction
	{
		Horizontal,
		Vertical,
		Upgraded,
	}

	/// <summary>
	/// Object that acts as a thumb.
	/// </summary>

	public Transform thumb;
	
	// Deprecated functionality. Use 'foregroundWidget' instead.
	[HideInInspector][SerializeField] Transform foreground;

	// Deprecated functionality
	[HideInInspector][SerializeField] float rawValue = 1f; // Use 'value'
	[HideInInspector][SerializeField] Direction direction = Direction.Upgraded; // Use 'fillDirection'
	[HideInInspector][SerializeField] bool mInverted = false;

	[System.Obsolete("Use 'value' instead")]
	public float sliderValue { get { return this.value; } set { this.value = value; } }

	[System.Obsolete("Use 'fillDirection' instead")]
	public bool inverted { get { return isInverted; } set { } }

	/// <summary>
	/// Upgrade from legacy functionality.
	/// </summary>

	protected override void Upgrade ()
	{
		if (direction != Direction.Upgraded)
		{
			mValue = rawValue;

			if (foreground != null)
				mFG = foreground.GetComponent<UIWidget>();

			if (direction == Direction.Horizontal)
			{
				mFill = mInverted ? FillDirection.RightToLeft : FillDirection.LeftToRight;
			}
			else
			{
				mFill = mInverted ? FillDirection.TopToBottom : FillDirection.BottomToTop;
			}
			direction = Direction.Upgraded;
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}
	}

	/// <summary>
	/// Make sure the thumb also responds to events.
	/// </summary>

	protected override void OnStart ()
	{
		if (thumb != null && thumb.collider != null)
		{
			UIEventListener fgl = UIEventListener.Get(thumb.gameObject);
			fgl.onPress += OnPressForeground;
			fgl.onDrag += OnDragForeground;
		}
	}

	/// <summary>
	/// Update the slider's foreground and position the thumb accordingly.
	/// </summary>

	public override void ForceUpdate ()
	{
		base.ForceUpdate();

		if (mFG != null && thumb != null)
		{
			Vector3[] corners = mFG.worldCorners;

			if (isHorizontal)
			{
				if (mSprite != null && mSprite.type == UISprite.Type.Filled)
				{
					Vector3 v0 = Vector3.Lerp(corners[0], corners[1], 0.5f);
					Vector3 v1 = Vector3.Lerp(corners[2], corners[3], 0.5f);
					thumb.position = Vector3.Lerp(v0, v1, isInverted ? 1f - value : value);
				}
				else
				{
					thumb.position = isInverted ?
						Vector3.Lerp(corners[0], corners[1], 0.5f) :
						Vector3.Lerp(corners[2], corners[3], 0.5f);
				}
			}
			else
			{
				if (mSprite != null && mSprite.type == UISprite.Type.Filled)
				{
					Vector3 v0 = Vector3.Lerp(corners[0], corners[3], 0.5f);
					Vector3 v1 = Vector3.Lerp(corners[1], corners[2], 0.5f);
					thumb.position = Vector3.Lerp(v0, v1, isInverted ? 1f - value : value);
				}
				else
				{
					thumb.position = isInverted ?
						Vector3.Lerp(corners[0], corners[3], 0.5f) :
						Vector3.Lerp(corners[1], corners[2], 0.5f);
				}
			}
		}
	}
}
