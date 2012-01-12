using UnityEngine;

/// <summary>
/// Attaching this to an object lets you activate tweener components on other objects.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Tween")]
public class UIButtonTween : MonoBehaviour
{
	public GameObject onHover;
	public GameObject onPress;
	public GameObject onClick;
	public bool toggleOnClick = false;
	public bool inReverse = false;

	void OnHover (bool isOver)
	{
		if (onHover != null)
		{
			Tweener[] tween = onHover.GetComponents<Tweener>();
			foreach (Tweener tw in tween) tw.Activate(!isOver);
		}
	}

	void OnPress (bool isPressed)
	{
		if (onPress != null)
		{
			Tweener[] tween = onPress.GetComponents<Tweener>();
			foreach (Tweener tw in tween) tw.Activate(!isPressed);
		}
	}

	void OnClick ()
	{
		if (onClick != null)
		{
			Tweener[] tween = onClick.GetComponents<Tweener>();

			foreach (Tweener tw in tween)
			{
				if (toggleOnClick) tw.Toggle();
				else tw.Activate(inReverse);
			}
		}
	}
}