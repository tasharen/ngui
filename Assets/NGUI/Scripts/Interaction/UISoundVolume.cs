using UnityEngine;

[RequireComponent(typeof(UISlider))]
[AddComponentMenu("NGUI/Interaction/Sound Volume")]
public class UISoundVolume : MonoBehaviour
{
	UISlider mSlider;

	void Awake ()
	{
		mSlider = GetComponent<UISlider>();
		mSlider.sliderValue = NGUITools.soundVolume;
		mSlider.eventReceiver = gameObject;
	}

	void OnSliderChange (float val)
	{
		NGUITools.soundVolume = val;
	}
}