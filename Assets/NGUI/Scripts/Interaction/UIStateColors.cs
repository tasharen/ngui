using UnityEngine;

/// <summary>
/// Changes the color of the widget, renderer or light based on the currently active state.
/// </summary>

[AddComponentMenu("NGUI/Interaction/State Colors")]
public class UIStateColors : MonoBehaviour
{
	public int currentState = 0;
	public float duration = 0.2f;
	public Color[] colors;

	void OnState (int state)
	{
		if (currentState != state)
		{
			currentState = state;
			if (colors == null || colors.Length == 0) return;
			int index = Mathf.Clamp(currentState, 0, colors.Length - 1);
			TweenColor.Begin(gameObject, duration, colors[index]);
		}
	}
}