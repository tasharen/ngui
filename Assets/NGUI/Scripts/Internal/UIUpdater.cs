using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central point of update calls for panels and widgets that ensures they are called in a proper order.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Internal/Updater")]
public class UIUpdater : MonoBehaviour
{
	static UIUpdater mInst;
	List<UIWidget> mWidgets = new List<UIWidget>();
	List<UIPanel> mPanels = new List<UIPanel>();

	/// <summary>
	/// Call all update functions.
	/// </summary>

	/*void Update ()
	{
		for (int i = mPanels.Count; i > 0; )
		{
			UIPanel panel = mPanels[--i];

			if (panel != null)
			{
				if (panel.enabled && panel.gameObject.active) panel.CustomUpdate();
			}
			else mPanels.RemoveAt(i);
		}
	}*/

	/// <summary>
	/// Call all late update functions and destroy this class if no callbacks have been registered.
	/// </summary>

	void LateUpdate ()
	{
		if (mInst != this)
		{
			if (Application.isPlaying) Destroy(gameObject);
			else DestroyImmediate(gameObject);
		}
		else
		{
			/*for (int i = mWidgets.Count; i > 0; )
			{
				UIWidget widget = mWidgets[--i];

				if (widget != null)
				{
					if (widget.enabled && widget.gameObject.active) widget.UIUpdate();
				}
				else mWidgets.RemoveAt(i);
			}*/

			for (int i = mPanels.Count; i > 0; )
			{
				UIPanel panel = mPanels[--i];

				if (panel != null)
				{
					if (panel.enabled && panel.gameObject.active) panel.CustomLateUpdate();
				}
				else mPanels.RemoveAt(i);
			}

			if (mWidgets.Count == 0 && mPanels.Count == 0)
			{
				if (Application.isPlaying) Destroy(gameObject);
				else DestroyImmediate(gameObject);
			}
		}
	}

	/// <summary>
	/// Ensure that there is an instance of this class present.
	/// </summary>

	static void CreateInstance ()
	{
		if (mInst == null)
		{
			mInst = GameObject.FindObjectOfType(typeof(UIUpdater)) as UIUpdater;

			if (mInst == null)
			{
				GameObject go = new GameObject("_UIUpdater");
				//go.hideFlags = HideFlags.HideAndDontSave;
				go.hideFlags = HideFlags.DontSave;
				mInst = go.AddComponent<UIUpdater>();
			}
		}
	}

	/// <summary>
	/// Add a new panel to the updater.
	/// </summary>

	static public void Add (UIPanel panel)
	{
		CreateInstance();
		if (!mInst.mPanels.Contains(panel)) mInst.mPanels.Add(panel);
		else Debug.Log("Already contains " + panel.name);
	}

	/// <summary>
	/// Add a new widget to the updater.
	/// </summary>

	static public void Add (UIWidget widget)
	{
		CreateInstance();
		if (!mInst.mWidgets.Contains(widget)) mInst.mWidgets.Add(widget);
	}

	/// <summary>
	/// Remove the specified panel from the updater.
	/// </summary>

	static public void Remove (UIPanel panel) { if (mInst != null) mInst.mPanels.Remove(panel); }

	/// <summary>
	/// Remove the specified widget from the updater.
	/// </summary>

	static public void Remove (UIWidget widget) { if (mInst != null) mInst.mWidgets.Remove(widget); }
}