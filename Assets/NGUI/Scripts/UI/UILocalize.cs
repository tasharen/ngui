using UnityEngine;

/// <summary>
/// Simple script that lets you localize a UIWidget.
/// </summary>

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/UI/Localize")]
public class UILocalize : MonoBehaviour
{
	/// <summary>
	/// Localization key.
	/// </summary>

	public string key;

	/// <summary>
	/// Perform the localization of the widget.
	/// </summary>

	public void Localize ()
	{
		if (!string.IsNullOrEmpty(key))
		{
			UIWidget w = GetComponent<UIWidget>();
			UILabel lbl = w as UILabel;
			UISprite sp = w as UISprite;

			if (lbl != null)
			{
				lbl.text = Localization.Get(key);
			}
			else if (sp != null)
			{
				sp.spriteName = Localization.Get(key);
			}
		}
	}

	void Start () { Localize(); }
}