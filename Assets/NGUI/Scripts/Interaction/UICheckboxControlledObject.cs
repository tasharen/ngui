using UnityEngine;

/// <summary>
/// Example script showing how to activate or deactivate a game object when OnActivate event is received.
/// OnActivate event is sent out by the UICheckbox script.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Checkbox Controlled Object")]
public class UICheckboxControlledObject : MonoBehaviour
{
	public GameObject target;
	public bool inverse = false;

	void OnActivate (bool isActive)
	{
		if (target != null) target.SetActiveRecursively(inverse ? !isActive : isActive);
	}
}