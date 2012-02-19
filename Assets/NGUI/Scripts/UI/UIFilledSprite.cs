//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Similar to a regular UISprite, but lets you only display a part of it. Great for progress bars, sliders, and alike.
/// Originally contributed by David Whatley.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Filled)")]
public class UIFilledSprite : UISprite
{
	public enum FillDirection
	{
		TowardRight,
		TowardTop,
		TowardLeft,
		TowardBottom,
	}

	[SerializeField] FillDirection mFillDirection = FillDirection.TowardRight;
	[SerializeField] float mFillAmount = 1.0f;

	/// <summary>
	/// Direction of the cut procedure.
	/// </summary>

	public FillDirection fillDirection
	{
		get
		{
			return mFillDirection;
		}
		set
		{
			if (mFillDirection != value)
			{
				mFillDirection = value;
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Amount of the sprite shown. 0-1 range with 0 being nothing shown, and 1 being the full sprite.
	/// </summary>

	public float fillAmount
	{
		get
		{
			return mFillAmount;
		}
		set
		{
			float val = Mathf.Clamp01(value);

			if (mFillAmount != val)
			{
				mFillAmount = val;
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Virtual function called by the UIScreen that fills the buffers.
	/// </summary>

	override public void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Vector2 uv0 = new Vector2(mOuterUV.xMin, mOuterUV.yMin);
		Vector2 uv1 = new Vector2(mOuterUV.xMax, mOuterUV.yMax);
		Vector2 uvDelta = uv1 - uv0;
		uvDelta *= mFillAmount;

		float x0 = 0f;
		float y0 = 0f;
		float x1 = 1f;
		float y1 = -1f;

		switch (fillDirection)
		{
		case FillDirection.TowardBottom:
			y1 *= mFillAmount;
			uv0.y = uv1.y - uvDelta.y;
			break;
		case FillDirection.TowardTop:
			y0 = -(1f - mFillAmount);
			uv1.y = uv0.y + uvDelta.y;
			break;
		case FillDirection.TowardRight:
			x1 *= mFillAmount;
			uv1.x = uv0.x + uvDelta.x;
			break;
		case FillDirection.TowardLeft:
			x0 = (1 - mFillAmount);
			uv0.x = uv1.x - uvDelta.x;
			break;
		}

		verts.Add(new Vector3(x1, y0, 0f));
		verts.Add(new Vector3(x1, y1, 0f));
		verts.Add(new Vector3(x0, y1, 0f));
		verts.Add(new Vector3(x0, y0, 0f));

		uvs.Add(uv1);
		uvs.Add(new Vector2(uv1.x, uv0.y));
		uvs.Add(uv0);
		uvs.Add(new Vector2(uv0.x, uv1.y));

		cols.Add(color);
		cols.Add(color);
		cols.Add(color);
		cols.Add(color);
	}
}