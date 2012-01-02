using UnityEngine;

/// <summary>
/// Simple checkbox functionality.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Checkbox")]
public class UICheckbox : UISend
{
	public bool startsChecked = true;

	public MonoBehaviour[] controlledComponents;

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
				UpdateComponents();
			}
		}
	}

	void Start ()
	{
		mChecked = startsChecked;
		Send(mChecked ? 1 : 0);
		UpdateComponents();
	}

	void OnClick () { isChecked = !isChecked; }

	void UpdateComponents ()
	{
		if (controlledComponents != null)
		{
			foreach (MonoBehaviour mb in controlledComponents)
			{
				mb.enabled = mChecked;
			}
		}
	}
}