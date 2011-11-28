using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Label")]
public class UILabel : UIWidget
{
	[SerializeField] UIFont mFont;
	[SerializeField] string mText = "";
	[SerializeField] bool mEncoding = true;

	/// <summary>
	/// Set the font used by this label.
	/// </summary>

	public UIFont font
	{
		get
		{
			return mFont;
		}
		set
		{
			if (mFont != value)
			{
				mFont = value;
				material = mFont.material;
			}
		}
	}

	/// <summary>
	/// Text that's being displayed by the label.
	/// </summary>

	public string text
	{
		get
		{
			return mText;
		}
		set
		{
			if (value != null && !string.Equals(mText, value))
			{
				mText = value;
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Whether this label will support color encoding in the format of [RRGGBB] and new line in the form of a "\\n" string.
	/// </summary>

	public bool supportEncoding
	{
		get
		{
			return mEncoding;
		}
		set
		{
			if (mEncoding != value)
			{
				mEncoding = value;
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Text is pixel-perfect when its scale matches the size.
	/// </summary>

	override public void MakePixelPerfect ()
	{
		if (mFont != null)
		{
			Vector3 pos = cachedTransform.localPosition;
			pos.x = Mathf.RoundToInt(pos.x);
			pos.y = Mathf.RoundToInt(pos.y);
			pos.z = Mathf.RoundToInt(pos.z);
			cachedTransform.localPosition = pos;

			Vector3 scale = cachedTransform.localScale;
			scale.x = mFont.size;
			scale.y = scale.x;
			scale.z = 1f;
			cachedTransform.localScale = scale;
		}
		else base.MakePixelPerfect();
	}

	/// <summary>
	/// Draw the label.
	/// </summary>

	public override void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		int start = verts.Count;
		mFont.Print(mText, color, verts, uvs, cols, mEncoding);

		// If the label should be centered, we need to figure out where the center would be
		if (centered && verts.Count > start)
		{
			float minX = verts[start].x;
			float maxX = minX;
			float minY = verts[start].y;
			float maxY = minY;

			for (int i = start + 1, imax = verts.Count; i < imax; ++i)
			{
				Vector3 v = verts[i];
				if (v.x < minX) minX = v.x;
				if (v.x > maxX) maxX = v.x;
				if (v.y < minY) minY = v.y;
				if (v.y > maxY) maxY = v.y;
			}

			float offsetX = (maxX - minX) * 0.5f;
			float offsetY = (maxY - minY) * 0.5f;

			for (int i = start, imax = verts.Count; i < imax; ++i)
			{
				Vector3 v = verts[i];
				v.x -= offsetX;
				v.y += offsetY;
				verts[i] = v;
			}
		}
	}
}