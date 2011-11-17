using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Very simple UI sprite -- a simple quad of specified size, drawn using a part of the texture atlas.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Sprite (Simple)")]
public class UISprite : UIWidget
{
	public bool centered = false;
	public Rect textureRect;

	protected Rect mSaved;
	protected bool mCentered;

	public override bool OnUpdate ()
	{
		if (mSaved != textureRect || mCentered != centered)
		{
			mSaved = textureRect;
			mCentered = centered;
			return true;
		}
		return false;
	}

	protected override void OnMatchScale (Vector3 scale)
	{
		textureRect.width = scale.x;
		textureRect.height = scale.y;
	}

	public override void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		Texture tex = material.mainTexture;

		Vector2 uv0 = (tex != null) ? new Vector2(mSaved.xMin / tex.width, 1.0f - mSaved.yMin / tex.height) : Vector2.zero;
		Vector2 uv1 = (tex != null) ? new Vector2(mSaved.xMax / tex.width, 1.0f - mSaved.yMax / tex.height) : Vector2.zero;

		if (mCentered)
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

		uvs.Add(new Vector2(uv1.x, uv0.y));
		uvs.Add(uv1);
		uvs.Add(new Vector2(uv0.x, uv1.y));
		uvs.Add(uv0);

		cols.Add(color);
		cols.Add(color);
		cols.Add(color);
		cols.Add(color);
	}
}