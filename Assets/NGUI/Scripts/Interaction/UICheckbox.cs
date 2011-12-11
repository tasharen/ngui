using UnityEngine;

/// <summary>
/// Simple checkbox functionality.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Checkbox")]
public class UICheckbox : UISend
{
	public bool startsChecked = true;

	bool mChecked = true;

	public bool isChecked
	{
		get
		{
			return mChecked;
		}
		set
		{
			if (mChecked != value)
			{
				mChecked = value;
				Send(mChecked ? 1 : 0);
			}
		}
	}

	void Start ()
	{
		mChecked = startsChecked;
		Send(mChecked ? 1 : 0);
	}

	void OnClick () { isChecked = !isChecked; }
}