using UnityEngine;

/// <summary>
/// Example script showing how to activate or deactivate a MonoBehaviour when OnActivate event is received.
/// OnActivate event is sent out by the UICheckbox script.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Checkbox Controlled Component")]
public class UICheckboxControlledComponent : MonoBehaviour
{
	public MonoBehaviour target;
	public bool inverse = false;

	void OnActivate (bool isActive)
	{
		if (enabled && target != null) target.enabled = inverse ? !isActive : isActive;
	}
}