using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Widget that tiles the sprite repeatedly, fully filling the area.
/// Used best with repeating tileable backgrounds.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Tiled)")]
public class UITiledSprite : UISprite
{
	public override void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		Texture tex = material.mainTexture;
		if (tex == null) return;

		Rect rect = mOuter;

		if (atlas.coordinates == UIAtlas.Coordinates.TexCoords)
		{
			rect = NGUITools.ConvertToPixels(mOuter, tex.width, tex.height, true);
		}

		Vector2 scale = finalScale;
		float width  = Mathf.Abs(rect.width / scale.x);
		float height = Mathf.Abs(rect.height / scale.y);

		// Safety
		if (width < 0.01f) width = 0.01f;
		if (height < 0.01f) height = 0.01f;

		Vector2 min = new Vector2(rect.xMin / tex.width, rect.yMin / tex.height);
		Vector2 max = new Vector2(rect.xMax / tex.width, rect.yMax / tex.height);

		Vector2 clipped = max;

		float y = 0f;

		int start = verts.Count;

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

		if (centered)
		{
			for (int i = start, imax = verts.Count; i < imax; ++i)
			{
				Vector3 v = verts[i];
				v.x -= 0.5f;
				v.y += 0.5f;
				verts[i] = v;
			}
		}
	}
}