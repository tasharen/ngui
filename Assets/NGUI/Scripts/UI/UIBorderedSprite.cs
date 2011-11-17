using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 9-sliced widget component used to draw large widgets using small textures.
/// Take a look at the following diagram:
/// 
/// +---+------------------+---+
/// | 1 |        2         | 3 |
/// +---+------------------+---+
/// |   |                  |   |
/// |   |                  |   |
/// | 4 |        5         | 6 |
/// |   |                  |   |
/// |   |                  |   |
/// +---+------------------+---+
/// | 7 |        8         | 9 |
/// +---+------------------+---+
/// 
/// When the widget is resized, corners (1, 3, 7, 9) are not stretched at all.
/// Sides (2, 4, 6, 8) are stretched in 1 direction (vertically or horizontally).
/// Center (5) is stretched both vertically and horizontally.
/// 
/// Generally it's a good idea to create the texture within the atlas that keeps this
/// stretching in mind. Smooth gradients work best for the center (5), as an example.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Bordered Sprite")]
public class UIBorderedSprite : UIWidget
{
	public Vector2 size = new Vector2(100f, 100f);
	public Rect outerRect;
	public Rect innerRect;

	Vector2 mSize;
	Rect mSavedOuter;
	Rect mSavedInner;

	public override bool OnUpdate ()
	{
		if (mSize != size || mSavedOuter != outerRect || mSavedInner != innerRect)
		{
			mSize = size;
			mSavedOuter = outerRect;
			mSavedInner = innerRect;
			return true;
		}
		return false;
	}

	protected override void OnMatchScale (Vector3 scale)
	{
		size.x = scale.x;
		size.y = scale.y;
	}

	protected override void OnCopyFrom (UIWidget widget)
	{
		base.OnCopyFrom(widget);

		UIBorderedSprite copy = widget as UIBorderedSprite;

		if (copy != null)
		{
			outerRect = copy.outerRect;
			innerRect = copy.innerRect;
		}
	}

	public override void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		Texture tex = material.mainTexture;

		Vector2[] v  = new Vector2[4];
		Vector2[] uv = new Vector2[4];

		float borderLeft	= mSavedInner.xMin - mSavedOuter.xMin;
		float borderRight	= mSavedOuter.xMax - mSavedInner.xMax;
		float borderTop		= mSavedInner.yMin - mSavedOuter.yMin;
		float borderBottom	= mSavedOuter.yMax - mSavedInner.yMax;

		v[0] = Vector2.zero;
		v[1] = new Vector2(borderLeft / mSize.x, -borderTop / mSize.y);
		v[2] = new Vector2(1.0f - borderRight / mSize.x, -(1.0f - borderBottom / mSize.y));
		v[3] = new Vector2(1f, -1f);

		if (tex != null)
		{
			uv[0] = new Vector2(mSavedOuter.xMin / tex.width, 1.0f - mSavedOuter.yMin / tex.height);
			uv[1] = new Vector2(mSavedInner.xMin / tex.width, 1.0f - mSavedInner.yMin / tex.height);
			uv[2] = new Vector2(mSavedInner.xMax / tex.width, 1.0f - mSavedInner.yMax / tex.height);
			uv[3] = new Vector2(mSavedOuter.xMax / tex.width, 1.0f - mSavedOuter.yMax / tex.height);
		}
		else
		{
			for (int i = 0; i < 4; ++i) uv[i] = Vector2.zero;
		}

		for (int x = 0; x < 3; ++x)
		{
			int x2 = x + 1;

			for (int y = 0; y < 3; ++y)
			{
				int y2 = y + 1;

				verts.Add(new Vector3(v[x2].x, v[y].y, 0f));
				verts.Add(new Vector3(v[x2].x, v[y2].y, 0f));
				verts.Add(new Vector3(v[x].x, v[y2].y, 0f));
				verts.Add(new Vector3(v[x].x, v[y].y, 0f));

				uvs.Add(new Vector2(uv[x2].x, uv[y].y));
				uvs.Add(new Vector2(uv[x2].x, uv[y2].y));
				uvs.Add(new Vector2(uv[x].x, uv[y2].y));
				uvs.Add(new Vector2(uv[x].x, uv[y].y));

				cols.Add(color);
				cols.Add(color);
				cols.Add(color);
				cols.Add(color);
			}
		}
	}
}