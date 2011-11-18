using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for all UI components that should be derived from when creating new widget types.
/// </summary>

public abstract class UIWidget : MonoBehaviour
{
	// Cached and saved values
	[SerializeField] UIAtlas mAtlas;
	[SerializeField] string mSpriteName;
	[SerializeField] Color mColor;
	[SerializeField] bool mCentered = false;
	[SerializeField] bool mAutoDepth = true;
	[SerializeField] int mDepth = 0;

	bool mPlayMode = true;
	Transform mTrans;
	UIScreen mScreen;
	Material mMat;
	Texture2D mTex;
	UIAtlas.Sprite mSprite;
	bool mIsDirty = false;
	int mLayer = 0;
	Rect mInner;
	Rect mOuter;

	protected Vector3 mPos;
	protected Quaternion mRot;
	protected Vector3 mScale;
	protected Rect mInnerUV;
	protected Rect mOuterUV;

	/// <summary>
	/// Cached for speed.
	/// </summary>

	public Transform cachedTransform { get { return mTrans; } }

	/// <summary>
	/// Returns the material used by this widget.
	/// </summary>

	public Material material { get { return mAtlas != null ? mAtlas.material : null; } }

	/// <summary>
	/// Returns the texture used to draw this widget.
	/// </summary>

	public Texture2D mainTexture { get { Material mat = material; return mat != null ? mat.mainTexture as Texture2D : null; } }

	/// <summary>
	/// Atlas used by this widget.
	/// </summary>
 
	public UIAtlas atlas
	{
		get
		{
			return mAtlas;
		}
		set
		{
			if (mAtlas != value)
			{
				RemoveFromScreen(true);
				mIsDirty = true;
				mAtlas = value;

				// Update the atlas and the texture
				if (mAtlas != null)
				{
					mMat = mAtlas.material;
					mTex = (mMat != null) ? mMat.mainTexture as Texture2D : null;
				}
				else
				{
					mMat = null;
					mTex = null;
				}

				// Re-link the sprite
				if (!string.IsNullOrEmpty(mSpriteName))
				{
					string sprite = mSpriteName;
					mSpriteName = "";
					spriteName = sprite;
				}
			}
		}
	}

	/// <summary>
	/// Sprite within the atlas used to draw this widget.
	/// </summary>
 
	public string spriteName
	{
		get
		{
			return mSpriteName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				// If the sprite name hasn't been set yet, no need to do anything
				if (string.IsNullOrEmpty(mSpriteName)) return;

				// Clear the sprite name and the sprite reference
				mIsDirty = true;
				mSpriteName = "";
				mSprite = null;
			}
			else if (string.IsNullOrEmpty(mSpriteName) || !string.Equals(mSpriteName, value,
				System.StringComparison.OrdinalIgnoreCase))
			{
				// If the sprite name changes, the sprite reference should also be updated
				mIsDirty = true;
				mSpriteName = value;
				mSprite = (mAtlas != null) ? mAtlas.GetSprite(mSpriteName) : null;
				UpdateUVs();
			}
		}
	}

	/// <summary>
	/// Color used by the widget.
	/// </summary>

	public Color color
	{
		get
		{
			return mColor;
		}
		set
		{
			if (mColor != value)
			{
				mIsDirty = true;
				mColor = value;
			}
		}
	}

	/// <summary>
	/// Whether the widget is centered or top-left aligned.
	/// </summary>

	public bool centered
	{
		get
		{
			return mCentered;
		}
		set
		{
			if (mCentered != value)
			{
				mIsDirty = true;
				mCentered = value;
			}
		}
	}

	/// <summary>
	/// Whether the depth is calculated automatically.
	/// </summary>

	public bool autoDepth
	{
		get
		{
			return mAutoDepth;
		}
		set
		{
			if (mAutoDepth != value)
			{
				mIsDirty = true;
				mAutoDepth = value;
				if (mAutoDepth) mDepth = CalculateDepth();
			}
		}
	}

	
	/// <summary>
	/// Depth controls the rendering order -- lowest to highest.
	/// </summary>

	public int depth
	{
		get
		{
			return mDepth;
		}
		set
		{
			if (mDepth != value)
			{
				mIsDirty = true;
				mDepth = value;
			}
		}
	}

	/// <summary>
	/// Static widget comparison function used for Z-sorting.
	/// </summary>

	static public int CompareFunc (UIWidget left, UIWidget right)
	{
		if (left.mDepth > right.mDepth) return 1;
		if (left.mDepth < right.mDepth) return -1;
		return 0;
	}

	/// <summary>
	/// Update the texture UVs used by the widget.
	/// </summary>

	void UpdateUVs ()
	{
		if (mSprite != null && mTex != null)
		{
			if (mInner != mSprite.inner || mOuter != mSprite.outer)
			{
				mIsDirty = true;
				mInner = mSprite.inner;
				mOuter = mSprite.outer;

				mInnerUV = mInner;
				mOuterUV = mOuter;

				if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
				{
					Vector2 size = new Vector2(mTex.width, mTex.height);
					mInnerUV = UIAtlas.ConvertToTexCoords(mInnerUV, size);
					mOuterUV = UIAtlas.ConvertToTexCoords(mOuterUV, size);
				}
			}
		}
	}

	/// <summary>
	/// Helper function, removes the widget from the rendering screen.
	/// </summary>

	void RemoveFromScreen (bool update)
	{
		if (mMat != null && mScreen != null)
		{
			mScreen.RemoveWidget(this);
			if (update && !mPlayMode) mScreen.LateUpdate();
			mScreen = null;
		}
	}

	/// <summary>
	/// If we haven't created a UI screen yet, do so now.
	/// </summary>

	void AddToScreen ()
	{
		// Material and texture values are not saved, so they need to be reset
		if (mAtlas != null)
		{
			if (mMat == null && mAtlas.material != null)
			{
				UIAtlas atl = mAtlas;
				mAtlas = null;
				atlas = atl;
			}
		}

		if (mMat != null && mScreen == null)
		{
			if (mTrans == null) mTrans = transform;
			mScreen = UIScreen.GetScreen(mTrans, mMat);
			mScreen.AddWidget(this);
			mIsDirty = true;
			//if (!mPlayMode) mScreen.LateUpdate();
		}
	}

	/// <summary>
	/// Helper function that automatically calculates depth of this widget based on the hierarchy.
	/// </summary>

	public int CalculateDepth ()
	{
		Transform t = transform;
		int val = 0;

		for (;;)
		{
			t = t.parent;
			if (t == null) break;
			++val;
		}
		return val;
	}

	/// <summary>
	/// Cache the transform.
	/// </summary>

	void Awake () { mTrans = transform; mPlayMode = Application.isPlaying; }

	/// <summary>
	/// Unregister this widget.
	/// </summary>

	void OnDestroy () { RemoveFromScreen(false); }

	/// <summary>
	/// Unregister this widget.
	/// </summary>

	void OnDisable () { RemoveFromScreen(false); }

	/// <summary>
	/// If the layer changes, we need to place the widget on a different screen.
	/// </summary>

	void Update ()
	{
		if (mTrans == null) mTrans = transform;

		if (mLayer != gameObject.layer)
		{
			RemoveFromScreen(false);
			mLayer = gameObject.layer;
		}
		if (mScreen == null) AddToScreen();
	}

	/// <summary>
	/// Force-refresh the widget. Only meant to be executed from the edit mode.
	/// </summary>

	public void Refresh ()
	{
		if (!mPlayMode)
		{
			Update();
			if (mScreen != null) mScreen.LateUpdate();
		}
	}

	/// <summary>
	/// Check to see if anything has changed, and if it has, mark the screen as having changed.
	/// </summary>

	public bool ScreenUpdate ()
	{
		if (mMat == null) return false;

		// Call the virtual function
		bool retVal = OnUpdate();

		// Update the UV coordinates
		if (!mPlayMode) UpdateUVs();

		// If there is no material to work with, there is no point in updating the pos/rot/scale
		if (mMat != null)
		{
			if (mIsDirty)
			{
				// If something has changed, simply update the cached values
				mPos = mTrans.position;
				mRot = mTrans.rotation;
				mScale = mTrans.lossyScale;
				retVal = true;
			}
			else
			{
				Vector3 pos = mTrans.position;
				Quaternion rot = mTrans.rotation;
				Vector3 scale = mTrans.lossyScale;

				// Check to see if the position, rotation or scale has changed
				if (mPos != pos || mRot != rot || mScale != scale)
				{
					mPos = pos;
					mRot = rot;
					mScale = scale;
					retVal = true;
				}
			}
		}
		mIsDirty = false;
		return retVal;
	}

	/// <summary>
	/// Virtual version of the Update function. Should return 'true' if the widget has changed visually.
	/// </summary>

	virtual public bool OnUpdate () { return false; }

	/// <summary>
	/// Virtual function called by the UIScreen that fills the buffers.
	/// </summary>

	virtual public void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols) { }
}