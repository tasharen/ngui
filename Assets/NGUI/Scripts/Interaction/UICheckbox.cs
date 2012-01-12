using UnityEngine;

/// <summary>
/// Simple checkbox functionality.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Checkbox")]
public class UICheckbox : MonoBehaviour
{
	public UISprite checkedSprite;
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
				UpdateComponents();
			}
		}
	}

	void Start ()
	{
		mChecked = startsChecked;
		UpdateComponents();
	}

	void OnClick () { isChecked = !isChecked; }

	void UpdateComponents ()
	{
		if (checkedSprite != null)
		{
			TweenColor.Begin(checkedSprite.gameObject, 0.2f, mChecked ? Color.white : Color.black);
		}

		if (controlledComponents != null)
		{
			foreach (MonoBehaviour mb in controlledComponents)
			{
				mb.enabled = mChecked;
			}
		}
	}
}