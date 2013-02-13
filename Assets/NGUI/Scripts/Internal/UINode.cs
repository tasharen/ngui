//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

#if UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UIPanel creates one of these records for each child transform under it.
/// This makes it possible to watch for transform changes, and if something does
/// change -- rebuild the buffer as necessary.
/// </summary>

public class UINode
{
	int mVisibleFlag = -1;

	public Transform trans;			// Managed transform
	public UIWidget widget;			// Widget on this transform, if any

#if UNITY_3 || UNITY_4_0
	public bool lastActive = false;	// Last active state
	public Vector3 lastPos;			// Last local position, used to see if it has changed
	public Quaternion lastRot;		// Last local rotation
	public Vector3 lastScale;		// Last local scale

	GameObject mGo;
#else
	float mLastAlpha = 0f;
#endif

	public int changeFlag = -1;		// -1 = not checked, 0 = not changed, 1 = changed

	/// <summary>
	/// -1 = not initialized, 0 = not visible, 1 = visible.
	/// </summary>

	public int visibleFlag
	{
		get
		{
			return (widget != null) ? widget.visibleFlag : mVisibleFlag;
		}
		set
		{
			if (widget != null) widget.visibleFlag = value;
			else mVisibleFlag = value;
		}
	}

	/// <summary>
	/// Must always have a transform.
	/// </summary>

	public UINode (Transform t)
	{
		trans = t;
#if UNITY_3 || UNITY_4_0
		lastPos = trans.localPosition;
		lastRot = trans.localRotation;
		lastScale = trans.localScale;
		mGo = t.gameObject;
#endif
	}

	/// <summary>
	/// Check to see if the local transform has changed since the last time this function was called.
	/// </summary>

	public bool HasChanged ()
	{
#if UNITY_3 || UNITY_4_0
		bool isActive = NGUITools.GetActive(mGo) && (widget == null || (widget.enabled && widget.isVisible));

		if (lastActive != isActive || (isActive &&
			(lastPos != trans.localPosition ||
			 lastRot != trans.localRotation ||
			 lastScale != trans.localScale)))
		{
			lastActive = isActive;
			lastPos = trans.localPosition;
			lastRot = trans.localRotation;
			lastScale = trans.localScale;
			return true;
		}
#else
		if (widget != null && widget.finalAlpha != mLastAlpha)
		{
			mLastAlpha = widget.finalAlpha;
			trans.hasChanged = false;
			return true;
		}
		else if (trans.hasChanged)
		{
			trans.hasChanged = false;
			return true;
		}
#endif
		return false;
	}
}
