using UnityEngine;

/// <summary>
/// Selectable sprite that follows the mouse.
/// </summary>

[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/Examples/UI Cursor")]
public class UICursor : MonoBehaviour
{
	static UICursor mInstance;

	Transform mTrans;
	UISprite mSprite;

	UIAtlas mAtlas;
	string mSpriteName;

	/// <summary>
	/// Keep an instance reference so this class can be easily found.
	/// </summary>

	void Awake () { mInstance = this; }
	void OnDestroy () { mInstance = null; }

	/// <summary>
	/// Cache the expected components and starting values.
	/// </summary>

	void Start ()
	{
		mTrans = transform;
		mSprite = GetComponentInChildren<UISprite>();
		mAtlas = mSprite.atlas;
		mSpriteName = mSprite.spriteName;
		mSprite.depth = 100;
	}

	/// <summary>
	/// Reposition the sprite.
	/// </summary>

	void Update ()
	{
		if (mSprite.atlas != null)
		{
			Vector3 pos = Input.mousePosition;
			pos.x -= Screen.width * 0.5f;
			pos.y -= Screen.height * 0.5f;
			mTrans.localPosition = NGUITools.ApplyHalfPixelOffset(pos, mTrans.localScale);
		}
	}

	/// <summary>
	/// Clear the cursor back to its original value.
	/// </summary>

	static public void Clear ()
	{
		Set(mInstance.mAtlas, mInstance.mSpriteName);
	}

	/// <summary>
	/// Override the cursor with the specified sprite.
	/// </summary>

	static public void Set (UIAtlas atlas, string sprite)
	{
		if (mInstance != null)
		{
			mInstance.mSprite.atlas = atlas;
			mInstance.mSprite.spriteName = sprite;
			mInstance.mSprite.MakePixelPerfect();
			mInstance.Update();
		}
	}
}