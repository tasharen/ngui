using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for all UI components that should be derived from when creating new widget types.
/// </summary>

public abstract class UIWidget : MonoBehaviour
{
	// Atlas used by this widget
	public UIAtlas atlas = null;

	// Sprite within the atlas used to draw this widget
	public string spriteName = "Dark";

	// Color tint applied to this widget
	public Color color = Color.white;

	// Whether the depth is calculated automatically
	public bool autoDepth = true;

	// Depth controls the rendering order -- lowest to highest
	public int depth = 0;

	// Groups can be used to break up the rendering into separate batches
	public int group = 0;

	// Cached for efficiency, used by UIScreen
	[HideInInspector] public Transform mTrans;

	// Cached values, used to check if anything has changed
	Material mMat;
	Texture2D mTex;
	UIAtlas mAtlas;
	UIAtlas.Sprite mSprite;
	Color mColor;
	UIScreen mScreen;
	Vector3 mPos;
	Quaternion mRot;
	Vector3 mScale;
	bool mAutoDepth = true;
	bool mDynamicUpdate = false;
	int mLayer = 0;
	int mGroup = 0;
	int mDepth = 0;
	Rect mInner;
	Rect mOuter;

	// Texture coordinates
	protected Rect mInnerUV;
	protected Rect mOuterUV;

	/// <summary>
	/// Returns the material used by this widget.
	/// </summary>

	public Material material { get { return mAtlas != null ? mAtlas.material : null; } }

	/// <summary>
	/// Returns the texture used to draw this widget.
	/// </summary>

	public Texture2D mainTexture { get { Material mat = material; return mat != null ? mat.mainTexture as Texture2D : null; } }

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
	/// Helper function that automatically calculates depth of this widget based on the hierarchy.
	/// </summary>

	public int CalculateDepth ()
	{
		Transform t = mTrans;
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
	/// Register this widget with the manager.
	/// </summary>

	void Start ()
	{
		if (mScreen == null && atlas != null && atlas.material != null && !string.IsNullOrEmpty(spriteName))
		{
			mColor	= color;
			mTrans	= transform;
			mPos	= mTrans.position;
			mRot	= mTrans.rotation;
			mScale	= mTrans.lossyScale;
			mAtlas	= atlas;
			mTex	= mainTexture;
			mSprite = mAtlas.GetSprite(spriteName);

			if (mSprite != null)
			{
				mInner = mSprite.inner;
				mOuter = mSprite.outer;

				mInnerUV = mInner;
				mOuterUV = mOuter;

				if (mTex != null && mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
				{
					Vector2 size = new Vector2(mTex.width, mTex.height);
					mInnerUV = UIAtlas.ConvertToTexCoords(mInnerUV, size);
					mOuterUV = UIAtlas.ConvertToTexCoords(mOuterUV, size);
				}
			}

			if (autoDepth) depth = CalculateDepth();
			mAutoDepth	= autoDepth;
			mDepth		= depth;
			mScreen		= UIScreen.GetScreen(mMat = atlas.material, mLayer = gameObject.layer, mGroup = group, true);

			// We want to update the widgets right away in the editor mode
			mDynamicUpdate = !Application.isPlaying;

			OnStart();
			mScreen.AddWidget(this);
		}
	}

	/// <summary>
	/// Unregister this widget.
	/// </summary>

	void OnDestroy ()
	{
		if (mMat != null && mScreen != null)
		{
			mScreen.RemoveWidget(this);
		}
		mScreen = null;
	}

	/// <summary>
	/// Work-around for Unity not calling OnEnable in some cases.
	/// </summary>

	void Update ()
	{
		if (mScreen == null) Start();
	}

	/// <summary>
	/// Enable and Disable functionality should mirror Start and Destroy.
	/// </summary>

	void OnEnable () { Start(); }
	void OnDisable () { OnDestroy(); }

	/// <summary>
	/// Check to see if anything has changed, and if it has, mark the screen as having changed.
	/// </summary>

	public bool ScreenUpdate ()
	{
		// Call the virtual function
		bool retVal = OnUpdate();

		// If something has changed, act accordingly
		if (HasSpriteChanged() || mMat != material || mTex != mainTexture || mGroup != group || mLayer != gameObject.layer || mColor != color)
		{
			if (mMat != null) mScreen.RemoveWidget(this);

			mMat	= material;
			mTex	= mainTexture;
			mColor	= color;
			mPos	= mTrans.position;
			mRot	= mTrans.rotation;
			mScale	= mTrans.lossyScale;
			mLayer	= gameObject.layer;
			mDepth	= depth;
			mGroup	= group;
			retVal	= true;

			if (mMat != null)
			{
				mScreen = UIScreen.GetScreen(mMat, mLayer, mGroup, true);
				mScreen.AddWidget(this);
			}
		}
		// Check to see if the position, rotation or scale has changed
		else if (mMat != null)
		{
			Vector3 pos = mTrans.position;
			Quaternion rot = mTrans.rotation;
			Vector3 scale = mTrans.lossyScale;

			if (mAutoDepth != autoDepth)
			{
				mAutoDepth = autoDepth;
				if (mAutoDepth) depth = CalculateDepth();
			}

			if (mDepth != depth || mPos != pos || mRot != rot || mScale != scale)
			{
				mPos		= pos;
				mRot		= rot;
				mScale		= scale;
				mAutoDepth	= autoDepth;
				mDepth		= depth;
				retVal		= true;
			}
		}
		return retVal;
	}

	/// <summary>
	/// Checks to see if the sprite has changed. Used in the Update function above.
	/// </summary>

	bool HasSpriteChanged ()
	{
		bool spriteChanged = false;

		// If the atlas changes, the sprite should also change
		if (mAtlas != atlas)
		{
			mAtlas = atlas;

			if (mAtlas != null)
			{
				mMat = mAtlas.material;
				mTex = mainTexture;
				mSprite = mAtlas.GetSprite(spriteName);
			}
			else
			{
				mMat = null;
				mTex = null;
				mSprite = null;
			}
			spriteChanged = true;
		}
		else if (mSprite != null && !string.Equals(mSprite.name, spriteName))
		{
			// If the sprite name changes, update the local reference
			mSprite = mAtlas.GetSprite(spriteName);
			spriteChanged = true;
		}

		// If the sprite reference changed we need to update the inner and outer rectangles
		if (mSprite != null)
		{
			if (spriteChanged)
			{
				mInner = mSprite.inner;
				mOuter = mSprite.outer;
				mInnerUV = mInner;
				mOuterUV = mOuter;
			}
			else if (mDynamicUpdate && (mInner != mSprite.inner || mOuter != mSprite.outer))
			{
				// If the rectangles change we also need to update the widget
				mInner = mSprite.inner;
				mOuter = mSprite.outer;
				mInnerUV = mInner;
				mOuterUV = mOuter;
				spriteChanged = true;
			}

			// Widgets always work with texture-space coordinates rather than pixels
			if (spriteChanged && mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
			{
				Texture2D tex = mainTexture;

				if (tex != null)
				{
					Vector2 size = new Vector2(tex.width, tex.height);
					mInnerUV = UIAtlas.ConvertToTexCoords(mInnerUV, size);
					mOuterUV = UIAtlas.ConvertToTexCoords(mOuterUV, size);
				}
			}
		}
		return spriteChanged;
	}

	/// <summary>
	/// Virtual version of the Start function.
	/// </summary>

	virtual public void OnStart () { }

	/// <summary>
	/// Virtual version of the Update function. Should return 'true' if the widget has changed visually.
	/// </summary>

	virtual public bool OnUpdate () { return false; }

	/// <summary>
	/// Virtual function called by the UIScreen that fills the buffers.
	/// </summary>

	virtual public void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols) { }
}