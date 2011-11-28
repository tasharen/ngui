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
[AddComponentMenu("NGUI/UI/Sprite (Sliced)")]
public class UISlicedSprite : UISprite
{
	protected Rect mInner;
	protected Rect mInnerUV;

	/// <summary>
	/// Inner set of UV coordinates.
	/// </summary>

	public Rect innerUV { get { return mInnerUV; } }

	/// <summary>
	/// Update the texture UVs used by the widget.
	/// </summary>

	override protected void UpdateUVs ()
	{
		if (mSprite != null && (mInner != mSprite.inner || mOuter != mSprite.outer))
		{
			Texture2D tex = mainTexture;

			if (tex != null)
			{
				mInner = mSprite.inner;
				mOuter = mSprite.outer;

				mInnerUV = mInner;
				mOuterUV = mOuter;

				if (atlas.coordinates == UIAtlas.Coordinates.Pixels)
				{
					mInnerUV = UIAtlas.ConvertToTexCoords(mInnerUV, tex.width, tex.height);
					mOuterUV = UIAtlas.ConvertToTexCoords(mOuterUV, tex.width, tex.height);
				}
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Sliced sprite shouldn't inherit the sprite's changes to this function.
	/// </summary>

	override public void MakePixelPerfect ()
	{
		Vector3 pos = cachedTransform.localPosition;
		pos.x = Mathf.RoundToInt(pos.x);
		pos.y = Mathf.RoundToInt(pos.y);
		pos.z = Mathf.RoundToInt(pos.z);
		cachedTransform.localPosition = pos;

		Vector3 scale = cachedTransform.localScale;
		scale.x = Mathf.RoundToInt(scale.x);
		scale.y = Mathf.RoundToInt(scale.y);
		scale.z = 1f;
		cachedTransform.localScale = scale;
	}

	/// <summary>
	/// Draw the widget.
	/// </summary>

	override public void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		if (mOuterUV == mInnerUV)
		{
			base.OnFill(verts, uvs, cols);
			return;
		}

		Vector2[] v  = new Vector2[4];
		Vector2[] uv = new Vector2[4];

		Texture tex = mainTexture;

		v[0] = Vector2.zero;
		v[1] = v[0];
		v[2] = new Vector2(1f, -1f);
		v[3] = v[2];

		if (tex != null)
		{
			float borderLeft	= mInnerUV.xMin - mOuterUV.xMin;
			float borderRight	= mOuterUV.xMax - mInnerUV.xMax;
			float borderTop		= mOuterUV.yMin - mInnerUV.yMin;
			float borderBottom	= mInnerUV.yMax - mOuterUV.yMax;

			Vector3 scale = cachedTransform.localScale;
			Vector2 sz = new Vector2(scale.x / tex.width, scale.y / tex.height);
			v[1] += new Vector2(borderLeft / sz.x, borderTop / sz.y);
			v[2] -= new Vector2(borderRight / sz.x, borderBottom / sz.y);

			uv[0] = new Vector2(mOuterUV.xMin, mOuterUV.yMax);
			uv[1] = new Vector2(mInnerUV.xMin, mInnerUV.yMax);
			uv[2] = new Vector2(mInnerUV.xMax, mInnerUV.yMin);
			uv[3] = new Vector2(mOuterUV.xMax, mOuterUV.yMin);
		}
		else
		{
			// No texture -- just use zeroed out texture coordinates
			for (int i = 0; i < 4; ++i) uv[i] = Vector2.zero;
		}

		if (centered)
		{
			Vector2 offset = new Vector2(-0.5f, 0.5f);
			for (int i = 0; i < 4; ++i) v[i] += offset;
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