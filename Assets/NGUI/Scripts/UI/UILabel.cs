using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Label")]
public class UILabel : UIWidget
{
	[SerializeField] UIFont mFont;
	[SerializeField] string mText = "";

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
				mIsDirty = true;
				Refresh();
			}
		}
	}

	/// <summary>
	/// Draw the label.
	/// </summary>

	public override void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		Vector2 uv0 = Vector2.zero;
		Vector2 uv1 = Vector2.one;

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