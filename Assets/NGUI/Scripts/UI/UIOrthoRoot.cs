using UnityEngine;

/// <summary>
/// This is a script used to keep the game object scaled to 2/(Screen.height).
/// If you use it, be sure to NOT use UIOrthoCamera at the same time.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Ortho Root")]
public class UIOrthoRoot : MonoBehaviour
{
	Transform mTrans;

	void Start ()
	{
		mTrans = transform;

		UIOrthoCamera oc = GetComponentInChildren<UIOrthoCamera>();
		
		if (oc != null)
		{
			Debug.LogWarning("UIOrthoRoot should not be active at the same time as UIOrthoCamera. Disabling UIOrthoCamera.", oc);
			Camera cam = oc.gameObject.GetComponent<Camera>();
			oc.enabled = false;
			if (cam != null) cam.orthographicSize = 1f;
		}
	}

	void Update ()
	{
		float size = 2f / Screen.height;
		Vector3 ls = mTrans.localScale;

		if (!Mathf.Approximately(ls.x, size) ||
			!Mathf.Approximately(ls.y, size) ||
			!Mathf.Approximately(ls.z, size))
		{
			mTrans.localScale = new Vector3(size, size, size);
		}
	}
}