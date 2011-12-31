using UnityEngine;

/// <summary>
/// Changes the color of the light this script is attached to depending on current state.
/// </summary>

[RequireComponent(typeof(Light))]
[AddComponentMenu("NGUI/Interaction/State Light")]
public class UIStateLight : MonoBehaviour
{
	public int currentState = 0;
	public float animationSpeed = 8f;
	public Color[] colors;

	Light mLight;

	void OnState (int state)
	{
		currentState = state;
	}

	void Start ()
	{
		mLight = light;
	}

	void Update ()
	{
		if (colors == null || colors.Length == 0) return;
		int index = Mathf.Clamp(currentState, 0, colors.Length - 1);
		mLight.color = Color.Lerp(mLight.color, colors[index], Mathf.Clamp01(Time.deltaTime * animationSpeed));
		mLight.enabled = (mLight.color.r + mLight.color.g + mLight.color.b) > 0.01f;
	}
}