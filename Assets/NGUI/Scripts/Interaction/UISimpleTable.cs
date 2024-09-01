//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Much simplified version of the UITable that goes top-to-bottom and organizes the data into 1+ column, adding new independent columns past max height.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Table (Simplified)")]
public class UISimpleTable : UIWidgetContainer
{
	public delegate void OnReposition ();

	[Tooltip("If the number of columns is 1, a maximum height can be used to automatically create a new column after the first one exceeds this value")]
	public int maxHeight = 0;

	[Tooltip("Whether the sort order will be inverted")]
	public bool inverted = false;

	[Tooltip("Final pivot point for the table's content.")]
	public UIWidget.Pivot cellAlignment = UIWidget.Pivot.TopLeft;

	[Tooltip("Final pivot point for the table itself.")]
	public UIWidget.Pivot pivot = UIWidget.Pivot.TopLeft;

	[Tooltip("Whether inactive children will be discarded from the table's calculations.")]
	public bool hideInactive = true;

	[Tooltip("Whether the parent container will be notified of the table's changes.")]
	public bool keepWithinPanel = false;

	[Tooltip("Padding around each entry, in pixels.")]
	public Vector2 padding = Vector2.zero;

	[Tooltip("Extra padding between each entry, in pixels.")]
	public Vector2 spacing = Vector2.zero;

	/// <summary>
	/// Delegate function that will be called when the table repositions its content.
	/// </summary>

	public OnReposition onReposition;

	protected UIPanel mPanel;
	protected bool mInitDone = false;
	protected bool mReposition = false;

	/// <summary>
	/// Reposition the children on the next Update().
	/// </summary>

	public bool repositionNow { set { if (value) { mReposition = true; enabled = true; } } }

	/// <summary>
	/// Get the current list of the grid's children.
	/// </summary>

	public List<Transform> GetChildList ()
	{
		var myTrans = transform;
		var list = new List<Transform>();

		for (int i = 0; i < myTrans.childCount; ++i)
		{
			var t = myTrans.GetChild(i);
			if (!hideInactive || (t && NGUITools.GetActive(t.gameObject)))
				list.Add(t);
		}
		return list;
	}

	protected virtual void OnEnable () { mReposition = true; }

	/// <summary>
	/// Position the grid's contents when the script starts.
	/// </summary>

	protected virtual void Start ()
	{
		Init();
		Reposition();
		enabled = false;
	}

	/// <summary>
	/// Find the necessary components.
	/// </summary>

	public virtual void Init ()
	{
		mInitDone = true;
		mPanel = NGUITools.FindInParents<UIPanel>(gameObject);
	}

	/// <summary>
	/// Is it time to reposition? Do so now.
	/// </summary>

	protected virtual void LateUpdate ()
	{
		if (mReposition) Reposition();
		enabled = false;
	}

	/// <summary>
	/// Reposition the content on inspector validation.
	/// </summary>

	void OnValidate () { if (!Application.isPlaying && NGUITools.GetActive(this)) Reposition(); }

	protected void RepositionVariableSize (List<Transform> children)
	{
		var count = children.Count;
		var bounds = new Bounds[count];
		UISimpleTableHint stc;

		// Calculate the bounds of each child
		for (int i = 0, imax = children.Count; i < imax; ++i)
		{
			var t = children[i];
			var b = NGUIMath.CalculateRelativeWidgetBounds(t, !hideInactive);

			var scale = t.localScale;
			b.min = Vector3.Scale(b.min, scale);
			b.max = Vector3.Scale(b.max, scale);
			bounds[i] = b;
		}

		var offset = 0;
		var xOffset = 0f;
		var yOffset = 0f;
		var lowestPoint = 0f;
		var paragraph = 0f;
		var height = 0f;

		for (int i = 0, imax = children.Count; i < imax; ++i)
		{
			var b = bounds[i];
			var nextHeight = height + b.size.y + spacing.y;
			children[i].TryGetComponent(out stc);

			if (stc != null && stc.startNewRow)
			{
				AddColumn(children, bounds, offset, i, ref xOffset, ref yOffset);
				
				lowestPoint = Mathf.Min(lowestPoint, yOffset);
				offset = i;
				paragraph = lowestPoint;
				xOffset = 0f;
				yOffset = paragraph;
			}
			else if (maxHeight > 0 && (nextHeight > maxHeight || (stc != null && stc.startNewColumn)))
			{
				AddColumn(children, bounds, offset, i, ref xOffset, ref yOffset);
				
				lowestPoint = Mathf.Min(lowestPoint, yOffset);
				offset = i;
				yOffset = paragraph;
				height = 0f;
			}
			else height = nextHeight;
		}

		// Add the final column
		if (offset < children.Count) AddColumn(children, bounds, offset, children.Count, ref xOffset, ref yOffset);

		// Apply the origin offset
		if (pivot != UIWidget.Pivot.TopLeft)
		{
			var b = NGUIMath.CalculateRelativeWidgetBounds(transform, !hideInactive);
			var po = NGUIMath.GetPivotOffset(pivot);

			float fx, fy;

			fx = Mathf.Lerp(0f, b.size.x, po.x);
			fy = Mathf.Lerp(-b.size.y, 0f, po.y);

			var myTrans = transform;

			for (int i = 0; i < myTrans.childCount; ++i)
			{
				var t = myTrans.GetChild(i);
				var sp = t.GetComponent<SpringPosition>();

				if (sp != null)
				{
					sp.enabled = false;
					sp.target.x -= fx;
					sp.target.y -= fy;
					sp.enabled = true;
				}
				else
				{
					var pos = t.localPosition;
					pos.x -= fx;
					pos.y -= fy;
					t.localPosition = pos;
				}
			}
		}
	}

	void AddColumn (List<Transform> children, Bounds[] bounds, int offset, int max, ref float xOffset, ref float yOffset)
	{
		// Determine the maximum width and height of the content's cells
		var maxBounds = bounds[offset];
		UISimpleTableHint stc;

		for (int i = offset + 1; i < max; ++i)
		{
			var b = bounds[i];
			
			if (children[i].TryGetComponent(out stc) && stc.ignoreWidth)
			{
				var c = b.center;
				var s = b.size;
				c.x -= b.extents.x;
				s.x = 0f;
				b = new Bounds(c, s);
			}

			maxBounds.Encapsulate(b);
		}

		var po = NGUIMath.GetPivotOffset(cellAlignment);

		for (int i = offset; i < max; ++i)
		{
			var b = bounds[i];
			var t = children[i];

			var pos = t.localPosition;

			pos.x = xOffset + b.extents.x - b.center.x;
			pos.x -= Mathf.Lerp(0f, b.max.x - b.min.x - maxBounds.max.x + maxBounds.min.x, po.x) - padding.x;
			pos.y = yOffset - b.extents.y - b.center.y - padding.y;
			if (i > offset) pos.y -= spacing.y;

			t.localPosition = pos;

			yOffset -= b.size.y + padding.y * 2f;
		}
		
		xOffset += padding.x + maxBounds.size.x + spacing.x;
	}

	/// <summary>
	/// Recalculate the position of all elements within the table, sorting them alphabetically if necessary.
	/// </summary>

	[ContextMenu("Execute")]
	public void Reposition () { Reposition(false); }

	/// <summary>
	/// Recalculate the position of all elements within the table, sorting them alphabetically if necessary.
	/// </summary>

	public virtual void Reposition (bool forceUpdateWidgetSizes)
	{
		if (Application.isPlaying && !mInitDone && NGUITools.GetActive(this)) Init();

		// If you have added widgets underneath the table that haven't had a chance to run their Update() yet, then their dimensions may not be correct yet,
		// as anchoring and labels have not yet had a chance to run. In this case you should pass 'true' to force-update them before proceeding.
		if (forceUpdateWidgetSizes)
		{
			NGUITools.ExecuteAll<UIWidget>(gameObject, "Start");
			NGUITools.ExecuteAll<UIWidget>(gameObject, "Update");
			NGUITools.ExecuteAll<UIWidget>(gameObject, "UpdateAnchors");
		}

		mReposition = false;
		var myTrans = transform;
		var ch = GetChildList();
		if (ch.Count > 0) RepositionVariableSize(ch);

		if (keepWithinPanel && mPanel != null)
		{
			mPanel.ConstrainTargetToBounds(myTrans, true);
			var sv = mPanel.GetComponent<UIScrollView>();
			if (sv != null) sv.UpdateScrollbars(true);
		}

		if (onReposition != null) onReposition();
	}
}
