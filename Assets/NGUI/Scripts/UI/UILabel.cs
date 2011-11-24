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
		mFont.Print(mText, color, verts, uvs, cols);
	}
}