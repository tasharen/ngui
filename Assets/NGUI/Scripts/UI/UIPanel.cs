using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI Panel acts as a separator in the UI drawing process. Every widget that's
/// a child of the game object that the UI Panel resides on will be drawn together.
/// </summary>

[AddComponentMenu("NGUI/UI/Panel")]
public class UIPanel : MonoBehaviour
{
	[SerializeField] bool mHidden = true;

	// List of UI Screens created on hidden and invisible game objects
	List<UIDrawCall> mDrawCalls = new List<UIDrawCall>();

	/// <summary>
	/// Retrieve the list of all active draw calls, removing inactive ones in the process.
	/// </summary>

	public List<UIDrawCall> drawCalls
	{
		get
		{
			for (int i = mDrawCalls.Count; i > 0; )
			{
				UIDrawCall dc = mDrawCalls[--i];

				if (dc == null)
				{
					mDrawCalls.RemoveAt(i);
				}
				else if (dc.widgets == 0)
				{
					if (Application.isPlaying) Destroy(dc);
					else DestroyImmediate(dc);
					mDrawCalls.RemoveAt(i);
				}
			}
			return mDrawCalls;
		}
	}

	/// <summary>
	/// Whether the panel's generated geometry will be hidden or not.
	/// </summary>

	public bool hidden
	{
		get
		{
			return mHidden;
		}
		set
		{
			if (mHidden != value)
			{
				mHidden = value;
				List<UIDrawCall> list = drawCalls;
				HideFlags flags = mHidden ? HideFlags.HideAndDontSave : HideFlags.DontSave | HideFlags.NotEditable;

				foreach (UIDrawCall dc in list)
				{
					dc.gameObject.active = false;
					dc.gameObject.hideFlags = flags;
					dc.gameObject.active = true;
				}
			}
		}
	}

	/// <summary>
	/// Find a UIPanel responsible for handling the specified transform.
	/// </summary>

	static public UIPanel Find (Transform trans)
	{
		UIPanel panel = null;

		while (panel == null && trans != null)
		{
			panel = trans.GetComponent<UIPanel>();
			if (panel != null) break;
			if (trans.parent == null) break;
			trans = trans.parent;
		}

		if (panel == null)
		{
			panel = trans.gameObject.AddComponent<UIPanel>();
		}
		return panel;
	}

	/// <summary>
	/// Get or create a UIScreen responsible for drawing the widgets using the specified material.
	/// </summary>

	UIDrawCall GetDrawCall (Material mat, bool createIfMissing)
	{
		foreach (UIDrawCall dc in drawCalls) if (dc.material == mat) return dc;

		UIDrawCall sc = null;

		if (createIfMissing)
		{
			GameObject go = new GameObject("_UIDrawCall [" + mat.name + "]");
			go.hideFlags = mHidden ? HideFlags.HideAndDontSave : HideFlags.DontSave | HideFlags.NotEditable;
			go.layer = gameObject.layer;

			sc = go.AddComponent<UIDrawCall>();
			sc.material = mat;

			mDrawCalls.Add(sc);
		}
		return sc;
	}

	/*void OnDisable ()
	{
		List<UIDrawCall> dcs = drawCalls;

		for (int i = dcs.Count; i > 0; )
		{
			UIDrawCall dc = dcs[--i];
			if (Application.isPlaying) Destroy(dc.gameObject);
			else DestroyImmediate(dc.gameObject);
		}
		mDrawCalls.Clear();
	}*/

	/// <summary>
	/// Add the specified widget to the managed list.
	/// </summary>

	public void AddWidget (UIWidget widget)
	{
		if (widget != null && widget.material != null)
		{
			UIDrawCall dc = GetDrawCall(widget.material, true);
			dc.AddWidget(widget);
		}
	}

	/// <summary>
	/// Remove the specified widget from the managed list.
	/// </summary>

	public void RemoveWidget (UIWidget widget)
	{
		if (widget != null)
		{
			UIDrawCall dc = GetDrawCall(widget.material, false);
			if (dc != null) dc.RemoveWidget(widget);
		}
	}

	/// <summary>
	/// Refresh a draw call responsible for the specified material.
	/// </summary>

	public void Refresh (Material mat)
	{
		foreach (UIDrawCall dc in drawCalls)
		{
			if (dc.material == mat)
			{
				dc.LateUpdate();
				return;
			}
		}
	}
}