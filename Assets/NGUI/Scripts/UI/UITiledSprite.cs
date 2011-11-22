using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Widget that tiles a part of the texture atlas, filling the specified size in pixels.
/// Used best with repeating tileable backgrounds.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Tiled)")]
public class UITiledSprite : UIWidget
{
	// Size of the area in pixels
	public Vector2 size = new Vector2(100f, 100f);

	// Sprite's rectangle within the atlas (in pixels)
	public Rect textureRect;

	Vector2 mSize;
	Rect mRect;

	public override bool OnUpdate ()
	{
		if (mSize != size || mRect != textureRect)
		{
			mSize = size;
			mRect = textureRect;
			return true;
		}
		return false;
	}

	public override void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		Texture tex = material.mainTexture;

		if (tex != null)
		{
			float width  = Mathf.Abs(mRect.width  / mSize.x);
			float height = Mathf.Abs(mRect.height / mSize.y);

			Vector2 min = new Vector2(mRect.xMin / tex.width, mRect.yMin / tex.height);
			Vector2 max = new Vector2(mRect.xMax / tex.width, mRect.yMax / tex.height);
			Vector2 clipped = max;

			float y = 0f;

			while (y < 1f)
			{
				float x = 0f;
				clipped.x = max.x;

				float y2 = y + height;

				if (y2 > 1f)
				{
					clipped.y = min.y + (max.y - min.y) * (1f - y) / (y2 - y);
					y2 = 1f;
				}

				while (x < 1f)
				{
					float x2 = x + width;

					if (x2 > 1f)
					{
						clipped.x = min.x + (max.x - min.x) * (1f - x) / (x2 - x);
						x2 = 1f;
					}

					verts.Add(new Vector3(x2, -y, 0f));
					verts.Add(new Vector3(x2, -y2, 0f));
					verts.Add(new Vector3(x, -y2, 0f));
					verts.Add(new Vector3(x, -y, 0f));

					uvs.Add(new Vector2(clipped.x, 1f - min.y));
					uvs.Add(new Vector2(clipped.x, 1f - clipped.y));
					uvs.Add(new Vector2(min.x, 1f - clipped.y));
					uvs.Add(new Vector2(min.x, 1f - min.y));

					cols.Add(color);
					cols.Add(color);
					cols.Add(color);
					cols.Add(color);

					x += width;
				}
				y += height;
			}
		}
	}
}