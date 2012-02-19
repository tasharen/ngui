//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This is a script used to keep the game object scaled to 2/(Screen.height).
/// If you use it, be sure to NOT use UIOrthoCamera at the same time.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Root")]
public class UIRoot : MonoBehaviour
{
	public bool automatic = true;
	public int manualHeight = 800;

	Transform mTrans;

	void Start ()
	{
		mTrans = transform;

		UIOrthoCamera oc = GetComponentInChildren<UIOrthoCamera>();
		
		if (oc != null)
		{
			Debug.LogWarning("UIRoot should not be active at the same time as UIOrthoCamera. Disabling UIOrthoCamera.", oc);
			Camera cam = oc.gameObject.GetComponent<Camera>();
			oc.enabled = false;
			if (cam != null) cam.orthographicSize = 1f;
		}
	}

	void Update ()
	{
		manualHeight = Mathf.Max(2, automatic ? Screen.height : manualHeight);

		float size = 2f / manualHeight;
		Vector3 ls = mTrans.localScale;

		if (!Mathf.Approximately(ls.x, size) ||
			!Mathf.Approximately(ls.y, size) ||
			!Mathf.Approximately(ls.z, size))
		{
			mTrans.localScale = new Vector3(size, size, size);
		}
	}
}