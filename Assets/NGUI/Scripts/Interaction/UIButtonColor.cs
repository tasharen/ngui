using UnityEngine;

/// <summary>
/// Simple example script of how a button can be colored when the mouse hovers over it or it gets pressed.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Color")]
public class UIButtonColor : MonoBehaviour
{
	public Transform target;
	public Color hover = new Color(0.8f, 1f, 0.6f, 1f);
	public Color pressed = Color.grey;
	public float duration = 0.2f;

	Color mColor;

	void Start ()
	{
		if (target == null) target = transform;
		UIWidget widget = target.GetComponent<UIWidget>();

		if (widget != null)
		{
			mColor = widget.color;
		}
		else
		{
			Renderer ren = renderer;

			if (ren != null)
			{
				mColor = ren.material.color;
			}
			else
			{
				Light lt = light;

				if (lt != null)
				{
					mColor = lt.color;
				}
				else
				{
					Debug.Log(NGUITools.GetHierarchy(gameObject) + " has nothing for UIButtonColor to color");
					enabled = false;
				}
			}
		}
	}

	void OnPress (bool isPressed)
	{
		TweenColor.Begin(target.gameObject, duration, isPressed ? pressed : mColor);
	}

	void OnHover (bool isOver)
	{
		TweenColor.Begin(target.gameObject, duration, isOver ? hover : mColor);
	}
}