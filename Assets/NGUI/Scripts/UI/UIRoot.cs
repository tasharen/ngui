//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is a script used to keep the game object scaled to 2/(Screen.height).
/// If you use it, be sure to NOT use UIOrthoCamera at the same time.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Root")]
public class UIRoot : MonoBehaviour
{
	static List<UIRoot> mRoots = new List<UIRoot>();

	/// <summary>
	/// List of all UIRoots present in the scene.
	/// </summary>

	static public List<UIRoot> list { get { return mRoots; } }

	public bool automatic = true;
	public int manualHeight = 800;

	Transform mTrans;

	void Awake () { mTrans = transform; mRoots.Add(this); }
	void OnDestroy () { mRoots.Remove(this); }

	void Start ()
	{
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
		if (mTrans != null)
		{
			manualHeight = Mathf.Max(2, automatic ? Screen.height : manualHeight);

			float size = 2f / manualHeight;
			Vector3 ls = mTrans.localScale;

			if (!(Mathf.Abs(ls.x - size) <= float.Epsilon) ||
				!(Mathf.Abs(ls.y - size) <= float.Epsilon) ||
				!(Mathf.Abs(ls.z - size) <= float.Epsilon))
			{
				mTrans.localScale = new Vector3(size, size, size);
			}
		}
	}

	/// <summary>
	/// Broadcast the specified message to the entire UI.
	/// </summary>

	static public void Broadcast (string funcName)
	{
		for (int i = 0, imax = mRoots.Count; i < imax; ++i)
		{
			UIRoot root = mRoots[i];
			if (root != null) root.BroadcastMessage(funcName, SendMessageOptions.DontRequireReceiver);
		}
	}

	/// <summary>
	/// Broadcast the specified message to the entire UI.
	/// </summary>

	static public void Broadcast (string funcName, object param)
	{
		if (param == null)
		{
			// More on this: http://answers.unity3d.com/questions/55194/suggested-workaround-for-sendmessage-bug.html
			Debug.LogError("SendMessage is bugged when you try to pass 'null' in the parameter field. It behaves as if no parameter was specified.");
		}
		else
		{
			for (int i = 0, imax = mRoots.Count; i < imax; ++i)
			{
				UIRoot root = mRoots[i];
				if (root != null) root.BroadcastMessage(funcName, param, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}