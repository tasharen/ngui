using UnityEngine;

/// <summary>
/// This script has been deprecated.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Deprecated/Grow On Hover")]
public class UIGrowOnHover : MonoBehaviour
{
	public float duration = 0.25f;
	public Vector3 amount = new Vector3(1.1f, 1.1f, 1.1f);

	void Awake ()
	{
		Debug.LogWarning(GetType() + " is obsolete. Replacing with UIButtonScale instead.");
		UIButtonScale bs = gameObject.GetComponent<UIButtonScale>();
		if (bs == null) bs = gameObject.AddComponent<UIButtonScale>();
		bs.hover = amount;
		bs.duration = duration;
		DestroyImmediate(this);
	}
}