//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Abstract UI rectangle containing functionality common to both panels and widgets.
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

	protected GameObject mGo;
	protected Transform mTrans;

	int mUpdateFrame = -1;

	/// <summary>
	/// Game object gets cached for speed. Can't simply return 'mGo' set in Awake because this function may be called on a prefab.
	/// </summary>

	public GameObject cachedGameObject { get { if (mGo == null) mGo = gameObject; return mGo; } }

	/// <summary>
	/// Transform gets cached for speed. Can't simply return 'mTrans' set in Awake because this function may be called on a prefab.
	/// </summary>

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	/// <summary>
	/// Get horizontal bounds points relative to the specified transform.
	/// </summary>

	public abstract float GetHorizontal (Transform relativeTo, float relative, int absolute);

	/// <summary>
	/// Get vertical bounds points relative to the specified transform.
	/// </summary>

	public abstract float GetVertical (Transform relativeTo, float relative, int absolute);

	/// <summary>
	/// Set anchor rect references on start.
	/// </summary>

	protected void Start () { UpdateAnchors(); OnStart(); }

	/// <summary>
	/// Rectangles need to update in a specific order -- parents before children.
	/// When deriving from this class, override its OnUpdate() function instead.
	/// </summary>

	public void Update ()
	{
		int frame = Time.frameCount;

		if (mUpdateFrame != frame)
		{
			mUpdateFrame = frame;

			if (leftAnchor.rect		!= null && leftAnchor.rect.mUpdateFrame		!= frame) leftAnchor.rect.Update();
			if (bottomAnchor.rect	!= null && bottomAnchor.rect.mUpdateFrame	!= frame) bottomAnchor.rect.Update();
			if (rightAnchor.rect	!= null && rightAnchor.rect.mUpdateFrame	!= frame) rightAnchor.rect.Update();
			if (topAnchor.rect		!= null && topAnchor.rect.mUpdateFrame		!= frame) topAnchor.rect.Update();

			OnUpdate();
		}
	}

	/// <summary>
	/// Ensure that all rect references are set correctly on the anchors.
	/// </summary>

	protected void UpdateAnchors ()
	{
		leftAnchor.rect		= (leftAnchor.target)	? leftAnchor.target.GetComponent<UIRect>()	 : null;
		bottomAnchor.rect	= (bottomAnchor.target) ? bottomAnchor.target.GetComponent<UIRect>() : null;
		rightAnchor.rect	= (rightAnchor.target)	? rightAnchor.target.GetComponent<UIRect>()	 : null;
		topAnchor.rect		= (topAnchor.target)	? topAnchor.target.GetComponent<UIRect>()	 : null;
	}

	/// <summary>
	/// Abstract start functionality, ensured to happen after the anchor rect references have been set.
	/// </summary>

	protected abstract void OnStart ();

	/// <summary>
	/// Abstract update functionality, ensured to happen after the targeting anchors have been updated.
	/// </summary>

	protected abstract void OnUpdate ();

#if UNITY_EDITOR
	/// <summary>
	/// This callback is sent inside the editor notifying us that some property has changed.
	/// </summary>

	protected virtual void OnValidate() { UpdateAnchors(); }
#endif
}
