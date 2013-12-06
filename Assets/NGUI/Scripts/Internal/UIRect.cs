//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Abstract UI rectangle containing functionality common to both panels and widgets.
/// A UI rectangle contains 4 anchor points (one for each side), and it ensures that they are updated in the proper order.
/// </summary>

public abstract class UIRect : MonoBehaviour
{
	[System.Serializable]
	public class AnchorPoint
	{
		public Transform target;
		public float relative = 0f;
		public int absolute = 0;

		[System.NonSerialized]
		public UIRect rect;

		[System.NonSerialized]
		public Camera cam;

		public AnchorPoint () { }
		public AnchorPoint (float relative) { this.relative = relative; }
	}

	/// <summary>
	/// Left side anchor.
	/// </summary>

	public AnchorPoint leftAnchor = new AnchorPoint();

	/// <summary>
	/// Right side anchor.
	/// </summary>

	public AnchorPoint rightAnchor = new AnchorPoint(1f);

	/// <summary>
	/// Bottom side anchor.
	/// </summary>

	public AnchorPoint bottomAnchor = new AnchorPoint();

	/// <summary>
	/// Top side anchor.
	/// </summary>

	public AnchorPoint topAnchor = new AnchorPoint(1f);

	protected UIRoot mRoot;
	protected Camera mAnchorCam;
	protected GameObject mGo;
	protected Transform mTrans;
	protected UIRect mParent;
	protected BetterList<UIRect> mChildren = new BetterList<UIRect>();
	protected bool mChanged = true;
	protected float mFinalAlpha = 0f;

	int mUpdateFrame = -1;
	bool mAnchorsCached = false;

	/// <summary>
	/// Rectangle's parent, if any.
	/// </summary>

	public UIRect parent { get { return mParent; } }

	/// <summary>
	/// Game object gets cached for speed. Can't simply return 'mGo' set in Awake because this function may be called on a prefab.
	/// </summary>

	public GameObject cachedGameObject { get { if (mGo == null) mGo = gameObject; return mGo; } }

	/// <summary>
	/// Transform gets cached for speed. Can't simply return 'mTrans' set in Awake because this function may be called on a prefab.
	/// </summary>

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	/// <summary>
	/// Returns 'true' if the widget is currently anchored on any side.
	/// </summary>

	public bool isAnchored
	{
		get
		{
			return leftAnchor.target || rightAnchor.target || topAnchor.target || bottomAnchor.target;
		}
	}

	/// <summary>
	/// Alpha property is exposed so that it's possible to make it cumulative.
	/// </summary>

	public abstract float finalAlpha { get; }

	/// <summary>
	/// Local-space corners of the UI rectangle. The order is bottom-left, top-left, top-right, bottom-right.
	/// </summary>

	public abstract Vector3[] localCorners { get; }

	/// <summary>
	/// World-space corners of the UI rectangle. The order is bottom-left, top-left, top-right, bottom-right.
	/// </summary>

	public abstract Vector3[] worldCorners { get; }

	/// <summary>
	/// Sets the local 'changed' flag, indicating that some parent value(s) are now be different, such as alpha for example.
	/// </summary>

	public void Invalidate (bool includeChildren)
	{
		mChanged = true;
		if (includeChildren)
			for (int i = 0; i < mChildren.size; ++i)
				mChildren.buffer[i].Invalidate(true);
	}

	/// <summary>
	/// Get the sides of the rectangle relative to the specified transform.
	/// The order is left, top, right, bottom.
	/// </summary>

	public abstract Vector3[] GetSides (Transform relativeTo);

	/// <summary>
	/// Helper function that gets the specified anchor's position relative to the chosen transform.
	/// </summary>

	protected Vector2 GetLocalPos (AnchorPoint ac, Transform trans)
	{
		Vector3 pos = ac.cam.WorldToScreenPoint(ac.target.position);
		pos = mAnchorCam.ScreenToWorldPoint(pos);
		if (trans != null) pos = trans.InverseTransformPoint(pos);
		pos.x = Mathf.Round(pos.x);
		pos.y = Mathf.Round(pos.y);
		return pos;
	}

	/// <summary>
	/// Automatically find the parent rectangle.
	/// </summary>

	protected virtual void OnEnable ()
	{
		mChanged = true;
		mParent = NGUITools.FindInParents<UIRect>(cachedTransform.parent);
		if (mParent != null && mParent.mRoot) mRoot = mParent.mRoot;
		else mRoot = NGUITools.FindInParents<UIRoot>(cachedTransform);
		if (mParent != null) mParent.mChildren.Add(this);
	}

	/// <summary>
	/// Clear the parent rectangle reference.
	/// </summary>

	protected virtual void OnDisable ()
	{
		if (mParent) mParent.mChildren.Remove(this);
		mParent = null;
		mRoot = null;
	}

	/// <summary>
	/// Set anchor rect references on start.
	/// </summary>

	protected void Start () { OnStart(); }

	/// <summary>
	/// Rectangles need to update in a specific order -- parents before children.
	/// When deriving from this class, override its OnUpdate() function instead.
	/// </summary>

	public void Update ()
	{
		if (!mAnchorsCached) CacheAnchors();

		int frame = Time.frameCount;

		if (mUpdateFrame != frame)
		{
			mUpdateFrame = frame;
			bool anchored = false;

			if (leftAnchor.target)
			{
				anchored = true;
				if (leftAnchor.rect != null && leftAnchor.rect.mUpdateFrame != frame)
					leftAnchor.rect.Update();
			}

			if (bottomAnchor.target)
			{
				anchored = true;
				if (bottomAnchor.rect != null && bottomAnchor.rect.mUpdateFrame != frame)
					bottomAnchor.rect.Update();
			}

			if (rightAnchor.target)
			{
				anchored = true;
				if (rightAnchor.rect != null && rightAnchor.rect.mUpdateFrame != frame)
					rightAnchor.rect.Update();
			}

			if (topAnchor.target)
			{
				anchored = true;
				if (topAnchor.rect != null && topAnchor.rect.mUpdateFrame != frame)
					topAnchor.rect.Update();
			}

			// Update the dimensions using anchors
			if (anchored) OnAnchor();

			// Continue with the update
			OnUpdate();
		}
	}

	/// <summary>
	/// Update the dimensions of the rectangle using anchor points.
	/// </summary>

	protected abstract void OnAnchor ();

	/// <summary>
	/// Ensure that all rect references are set correctly on the anchors.
	/// </summary>

	protected void CacheAnchors ()
	{
		mAnchorsCached = true;

		leftAnchor.rect		= (leftAnchor.target)	? leftAnchor.target.GetComponent<UIRect>()	 : null;
		bottomAnchor.rect	= (bottomAnchor.target) ? bottomAnchor.target.GetComponent<UIRect>() : null;
		rightAnchor.rect	= (rightAnchor.target)	? rightAnchor.target.GetComponent<UIRect>()	 : null;
		topAnchor.rect		= (topAnchor.target)	? topAnchor.target.GetComponent<UIRect>()	 : null;

		FindCameraFor(leftAnchor);
		FindCameraFor(bottomAnchor);
		FindCameraFor(rightAnchor);
		FindCameraFor(topAnchor);
	}

	/// <summary>
	/// Helper function -- attempt to find the camera responsible for the specified anchor.
	/// </summary>

	void FindCameraFor (AnchorPoint ap)
	{
		// If we don't have a target or have a rectangle to work with, camera isn't needed
		if (ap.target == null || ap.rect != null)
		{
			ap.cam = null;
			mAnchorCam = null;
		}
		else
		{
			// Find the camera responsible for the target object
			ap.cam = NGUITools.FindCameraForLayer(ap.target.gameObject.layer);

			// No camera found? Clear the references
			if (ap.cam == null)
			{
				ap.target = null;
				mAnchorCam = null;
				return;
			}
			
			// Find the camera responsible for this rectangle
			if (mAnchorCam == null)
			{
				mAnchorCam = NGUITools.FindCameraForLayer(cachedGameObject.layer);

				// No camera found? Clear the references
				if (mAnchorCam == null)
				{
					ap.target = null;
					ap.cam = null;
				}
			}
		}
	}

	/// <summary>
	/// Call this function when the rectangle's parent has changed.
	/// </summary>

	public virtual void ParentHasChanged ()
	{
		UIRect parent = NGUITools.FindInParents<UIRect>(cachedTransform.parent);

		if (mParent != parent)
		{
			if (mParent) mParent.mChildren.Remove(this);
			mParent = parent;
			if (mParent) mParent.mChildren.Add(this);
		}
	}

	/// <summary>
	/// Abstract start functionality, ensured to happen after the anchor rect references have been set.
	/// </summary>

	protected abstract void OnStart ();

	/// <summary>
	/// Abstract update functionality, ensured to happen after the targeting anchors have been updated.
	/// </summary>

	protected virtual void OnUpdate () { }

#if UNITY_EDITOR
	/// <summary>
	/// This callback is sent inside the editor notifying us that some property has changed.
	/// </summary>

	protected virtual void OnValidate()
	{
		CacheAnchors();
		Invalidate(true);
	}
#endif
}
