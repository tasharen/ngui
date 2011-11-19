using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Very simple UI sprite -- a simple quad of specified size, drawn using a part of the texture atlas.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Sprite (Simple)")]
public class UISprite : UIWidget
{
	public override void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		Vector2 uv0 = new Vector2(mOuterUV.xMin, mOuterUV.yMin);
		Vector2 uv1 = new Vector2(mOuterUV.xMax, mOuterUV.yMax);

		if (centered)
		{
			verts.Add(new Vector3(0.5f, 0.5f, 0f));
			verts.Add(new Vector3(0.5f, -0.5f, 0f));
			verts.Add(new Vector3(-0.5f, -0.5f, 0f));
			verts.Add(new Vector3(-0.5f, 0.5f, 0f));
		}
		else
		{
			verts.Add(new Vector3(1f, 0f, 0f));
			verts.Add(new Vector3(1f, -1f, 0f));
			verts.Add(new Vector3(0f, -1f, 0f));
			verts.Add(new Vector3(0f, 0f, 0f));
		}

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