using UnityEngine;

/// <summary>
/// Example script that can be used to show tooltips.
/// </summary>

[AddComponentMenu("NGUI/Examples/Tooltip")]
public class UITooltip : MonoBehaviour
{
	static UITooltip mInstance;

	public UILabel text;
	public UISlicedSprite background;

	float mTarget = 0f;
	float mCurrent = 0f;

	UIWidget[] mWidgets;

	void Awake () { mInstance = this; }
	void OnDestroy () { mInstance = null; }
	void Start () { mWidgets = GetComponentsInChildren<UIWidget>(); SetAlpha(0f); }

	void Update ()
	{
		if (mCurrent != mTarget)
		{
			mCurrent = Mathf.Lerp(mCurrent, mTarget, Time.deltaTime * 8f);
			if (Mathf.Abs(mCurrent - mTarget) < 0.001f) mCurrent = mTarget;
			SetAlpha(mCurrent);
		}
	}

	void SetAlpha (float val)
	{
		foreach (UIWidget w in mWidgets)
		{
			Color c = w.color;
			c.a = val;
			w.color = c;
		}
	}

	void SetText (string tooltipText)
	{
		if (tooltipText != null)
		{
			mTarget = 1f;
			if (text != null) text.text = tooltipText;
		}
		else mTarget = 0f;
	}

	static public void ShowText (string tooltipText) { if (mInstance != null) mInstance.SetText(tooltipText); }

	static public void ShowItem (InvGameItem item)
	{
		if (item != null)
		{
			InvBaseItem bi = item.baseItem;

			if (bi != null)
			{
				string t = item.name + "\n";
				t += bi.slot + "\n";
				t += bi.description;
				ShowText(t);
				return;
			}
		}
		if (mInstance != null) mInstance.mTarget = 0f;
	}
}