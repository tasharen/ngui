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

	Transform mTrans;

	/// <summary>
	/// If set to 'true', the UIRoot will be scaled automatically as the resolution changes, and will keep the UI under it
	/// pixel-perfect. If set to 'false', 'manualHeight' is used instead, and the UI will scale proportionally to screen's height,
	/// always remaining the same relative size regardless of the resolution changes.
	/// </summary>

	public bool automatic = true;

	/// <summary>
	/// Height of the screen when 'automatic' is turned off.
	/// </summary>

	public int manualHeight = 800;

	/// <summary>
	/// If the screen height goes below this value, it will be as if 'automatic' is turned off with the 'manualHeight' set to this value.
	/// </summary>

	public int minimumHeight = 320;

	/// <summary>
	/// If the screen height goes above this value, it will be as if 'automatic' is turned off with the 'manualHeight' set to this value.
	/// </summary>

	public int maximumHeight = 1080;

	/// <summary>
	/// UI Root's active height, based on the size of the screen.
	/// </summary>

	public int activeHeight
	{
		get
		{
			int height = Mathf.Max(2, Screen.height);
			if (height < minimumHeight) return minimumHeight;
			if (height > maximumHeight) return maximumHeight;
			else if (automatic) return height;
			return manualHeight;
		}
	}

	/// <summary>
	/// Pixel size adjustment. Most of the time it's at 1, unless 'automatic' is turned off.
	/// </summary>

	public float pixelSizeAdjustment
	{
		get
		{
			float height = Screen.height;
			if (height < minimumHeight) return minimumHeight / height;
			if (height > maximumHeight) return maximumHeight / height;
			return automatic ? 1f : manualHeight / height;
		}
	}

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
			float size = 2f / activeHeight;
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