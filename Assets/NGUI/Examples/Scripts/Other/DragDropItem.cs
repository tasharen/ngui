//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

[AddComponentMenu("NGUI/Examples/Drag and Drop Item")]
public class DragDropItem : MonoBehaviour
{
	/// <summary>
	/// Prefab object that will be instantiated on the DragDropSurface if it receives the OnDrop event.
	/// </summary>

	public GameObject prefab;

	Transform mTrans;
	Transform mParent;
	Collider mCollider;
	UIRoot mRoot;
	int mTouchID = int.MinValue;

	/// <summary>
	/// Cache the transform.
	/// </summary>

	void Awake ()
	{
		mTrans = transform;
		mCollider = collider;
	}

	/// <summary>
	/// Start the dragging operation.
	/// </summary>

	void OnDragStart ()
	{
		if (!enabled || mTouchID != int.MinValue) return;

		// Disable the collider so that it doesn't intercept events
		if (mCollider != null) mCollider.enabled = false;

		mTouchID = UICamera.currentTouchID;
		mParent = mTrans.parent;
		mRoot = NGUITools.FindInParents<UIRoot>(mTrans.gameObject);

		if (DragDropRoot.root != null)
			mTrans.parent = DragDropRoot.root;

		Vector3 pos = mTrans.localPosition;
		pos.z = 0f;
		mTrans.localPosition = pos;

		// Notify the widgets that the parent has changed
		NGUITools.MarkParentAsChanged(gameObject);
	}

	/// <summary>
	/// Perform the dragging.
	/// </summary>

	void OnDrag (Vector2 delta)
	{
		if (!enabled || mTouchID != UICamera.currentTouchID) return;
		mTrans.localPosition += (Vector3)delta * mRoot.pixelSizeAdjustment;
	}

	/// <summary>
	/// Notification sent when the drag event has ended.
	/// </summary>

	void OnDragEnd ()
	{
		if (!enabled || mTouchID != UICamera.currentTouchID) return;
		mTouchID = int.MinValue;

		// Enable the collider again
		if (mCollider != null) mCollider.enabled = true;

		// Is there a droppable container?
		DragDropContainer container = (UICamera.hoveredObject != null) ? UICamera.hoveredObject.GetComponent<DragDropContainer>() : null;

		if (container != null)
		{
			// Container found -- parent this object to the container
			mTrans.parent = (container.reparentTarget != null) ? container.reparentTarget : container.transform;

			Vector3 pos = mTrans.localPosition;
			pos.z = 0f;
			mTrans.localPosition = pos;
		}
		else
		{
			// No valid container under the mouse -- revert the item's parent
			mTrans.parent = mParent;
		}

		// Notify the table of this change
		UITable table = NGUITools.FindInParents<UITable>(gameObject);
		if (table != null) table.repositionNow = true;

		// Notify the grid as well, if we have one
		UIGrid grid = NGUITools.FindInParents<UIGrid>(gameObject);
		if (grid != null) grid.repositionNow = true;

		// Notify the widgets that the parent has changed
		NGUITools.MarkParentAsChanged(gameObject);
	}
}
