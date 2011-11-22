using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI Panel acts as a separator in the UI drawing process. Every widget that's
/// a child of the game object that the UI Panel resides on will be drawn together.
/// </summary>

[AddComponentMenu("NGUI/UI Panel")]
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
				if (dc == null || dc.widgets == 0) mDrawCalls.RemoveAt(i);
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
	/// Get or create a UIScreen responsible for drawing the widgets using the specified material.
	/// </summary>

	static public UIDrawCall GetScreen (Transform trans, Material mat)
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
		else
		{
			List<UIDrawCall> list = panel.drawCalls;
			foreach (UIDrawCall dc in list) if (dc.material == mat) return dc;
		}

		GameObject go = new GameObject("_UIScreen [" + mat.name + "]");
		go.hideFlags = panel.mHidden ? HideFlags.HideAndDontSave : HideFlags.DontSave | HideFlags.NotEditable;
		go.layer = trans.gameObject.layer;

		UIDrawCall sc = go.AddComponent<UIDrawCall>();
		sc.material = mat;

		panel.mDrawCalls.Add(sc);
		return sc;
	}
}