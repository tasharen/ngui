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
		mFont.Print(mText, Vector2.one, color, verts, uvs, cols);
	}
}