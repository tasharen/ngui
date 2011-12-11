using UnityEngine;

/// <summary>
/// Simple checkbox functionality.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Checkbox")]
public class UICheckbox : UISend
{
	public bool isChecked = true;

	void Start () { Send(isChecked ? 1 : 0); }

	void OnClick ()
	{
		isChecked = !isChecked;
		Send(isChecked ? 1 : 0);
	}
}