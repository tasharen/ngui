using UnityEngine;

/// <summary>
/// Changes the color of the widget based on the currently active state.
/// Tip: Use UISendState to change the state of a remote widget that has UIStateColors attached.
/// </summary>

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/Interaction/State Colors")]
public class UIStateColors : MonoBehaviour
{
	public int currentState = 0;
	public float animationSpeed = 8f;
	public Color[] colors;

	UIWidget mWidget;

	void OnState (int state)
	{
		currentState = state;
	}

	void Start ()
	{
		mWidget = GetComponent<UIWidget>();
	}

	void Update ()
	{
		if (colors == null || colors.Length == 0) return;
		int index = Mathf.Clamp(currentState, 0, colors.Length - 1);
		mWidget.color = Color.Lerp(mWidget.color, colors[index], Mathf.Clamp01(Time.deltaTime * animationSpeed));
	}
}