using UnityEngine;

/// <summary>
/// This script automatically changes the color of the specified sprite based on the value of the slider.
/// </summary>

[ExecuteInEditMode]
[RequireComponent(typeof(UISlider))]
[AddComponentMenu("NGUI/Examples/Slider Colors")]
public class UISliderColors : MonoBehaviour
{
	public UISprite sprite;

	public Color[] colors = new Color[] { Color.red, Color.yellow, Color.green };

	UISlider mSlider;

	void Start () { mSlider = GetComponent<UISlider>(); Update(); }

	void Update ()
	{
		if (sprite == null || colors.Length == 0) return;

		float val = mSlider.sliderValue;
		val *= (colors.Length - 1);
		int startIndex = Mathf.FloorToInt(val);

		if (startIndex < 0)
		{
			sprite.color = colors[0];
		}
		else if (startIndex + 1 < colors.Length)
		{
			float factor = (val - startIndex) / (colors.Length - 2);
			sprite.color = Color.Lerp(colors[startIndex], colors[startIndex + 1], factor);
		}
		else if (startIndex < colors.Length)
		{
			sprite.color = colors[startIndex];
		}
		else sprite.color = colors[colors.Length - 1];
	}
}