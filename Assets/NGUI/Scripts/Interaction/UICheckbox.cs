using UnityEngine;

/// <summary>
/// Simple checkbox functionality. If 'option' is enabled, checking this checkbox will uncheck all other checkboxes with the same parent.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Checkbox")]
public class UICheckbox : MonoBehaviour
{
	public UISprite checkedSprite;
	public GameObject eventReceiver;
	public bool startsChecked = true;
	public bool option = false;

	bool mChecked = true;
	Transform mTrans;

	/// <summary>
	/// Whether the checkbox is checked.
	/// </summary>

	public bool isChecked
	{
		get
		{
			return mChecked;
		}
		set
		{
			if (mChecked != value && (!option || value))
			{
				Set(value);
			}
		}
	}

	/// <summary>
	/// Activate the initial state.
	/// </summary>

	void Start ()
	{
		mTrans = transform;
		if (eventReceiver == null) eventReceiver = gameObject;
		Set(startsChecked);
	}

	/// <summary>
	/// Check or uncheck on click.
	/// </summary>

	void OnClick () { if (enabled) isChecked = !isChecked; }

	/// <summary>
	/// Fade out or fade in the checkmark and notify the target of OnChecked event.
	/// </summary>

	void Set (bool state)
	{
		// Uncheck all other checkboxes
		if (option && state)
		{
			UICheckbox[] cbs = mTrans.parent.GetComponentsInChildren<UICheckbox>();
			foreach (UICheckbox cb in cbs) if (cb != this) cb.Set(false);
		}

		// Remember the state
		mChecked = state;

		// Tween the color of the checkmark
		if (checkedSprite != null)
		{
			Color c = checkedSprite.color;
			c.a = mChecked ? 1f : 0f;
			TweenColor.Begin(checkedSprite.gameObject, 0.2f, c);
		}

		// Send out the event notification
		if (eventReceiver != null) eventReceiver.SendMessage("OnActivate", mChecked, SendMessageOptions.DontRequireReceiver);
	}
}