using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This script should be attached to each camera that's used to draw the objects with
/// UI components on them. This may mean only one camera (main camera or your UI camera),
/// or multiple cameras if you happen to have multiple viewports. Failing to attach this
/// script simply means that objects drawn by this camera won't receive UI notifications:
/// 
/// - OnHover (isOver) is sent when the mouse hovers over a collider or moves away.
/// - OnPress (isDown) is sent when a mouse button gets pressed on the collider.
/// - OnSelect (selected) is sent when a mouse button is released on the same object as it was pressed on.
/// - OnClick is sent with the same conditions as OnSelect, with the added check to see if the mouse has not moved much.
/// - OnInput (text) is sent when typing after selecting a collider by clicking on it.
/// - OnTooltip (show) is sent when the mouse hovers over a collider for some time without moving.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Camera")]
[RequireComponent(typeof(Camera))]
public class UICamera : MonoBehaviour
{
	public float tooltipDelay = 1f;

	static List<UICamera> mList = new List<UICamera>();

	bool mUseTouchInput = false;
	Camera mCam = null;
	GameObject mMouse = null;
	GameObject mHover = null;
	GameObject mDown = null;
	GameObject mSel = null;
	GameObject mTooltip = null;
	Vector3 mPos = Vector3.zero;
	Vector2 mDelta = Vector2.zero;
	Vector2 mTotalDelta = Vector2.zero;
	LayerMask mLayerMask;
	bool mConsiderForClick = false;
	float mTooltipTime = 0f;

	/// <summary>
	/// Caching is always preferable for performance.
	/// </summary>

	public Camera cachedCamera { get { if (mCam == null) mCam = camera; return mCam; } }

	/// <summary>
	/// Helper function that determines if this script should be handling the events.
	/// </summary>

	bool handlesEvents { get { return eventHandler == this; } }

	/// <summary>
	/// Current mouse or touch position.
	/// </summary>

	static public Vector3 mousePosition
	{
		get
		{
			UICamera mouse = eventHandler;
			return (mouse != null) ? mouse.mPos : Vector3.zero;
		}
	}

	/// <summary>
	/// Convenience function that returns the main HUD camera.
	/// </summary>

	static public Camera mainCamera
	{
		get
		{
			UICamera mouse = eventHandler;
			return (mouse != null) ? mouse.cachedCamera : null;
		}
	}

	/// <summary>
	/// Event handler for all types of events.
	/// </summary>

	static UICamera eventHandler
	{
		get
		{
			foreach (UICamera mouse in mList)
			{
				// Invalid or inactive entry -- keep going
				if (mouse == null || !mouse.enabled || !mouse.gameObject.active) continue;
				return mouse;
			}
			return null;
		}
	}

	/// <summary>
	/// Static comparison function used for sorting.
	/// </summary>

	static int CompareFunc (UICamera a, UICamera b)
	{
		if (a.cachedCamera.depth < b.cachedCamera.depth) return 1;
		if (a.cachedCamera.depth > b.cachedCamera.depth) return -1;
		return 0;
	}

	/// <summary>
	/// Returns the object under the specified position.
	/// </summary>

	static GameObject GetObject (Vector3 inPos)
	{
		foreach (UICamera mouse in mList)
		{
			// Skip inactive scripts
			if (!mouse.enabled || !mouse.gameObject.active) continue;

			// Convert to view space
			Camera cam = mouse.cachedCamera;
			Vector3 pos = cam.ScreenToViewportPoint(inPos);

			// If it's outside the camera's viewport, do nothing
			if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f) continue;

			// Cast a ray into the screen
			Ray ray = cam.ScreenPointToRay(inPos);
			RaycastHit hitInfo;

			// Raycast into the screen
			if (Physics.Raycast(ray, out hitInfo, cam.farClipPlane - cam.nearClipPlane, mouse.cachedCamera.cullingMask))
			{
				return hitInfo.collider.gameObject;
			}
		}
		return null;
	}

	/// <summary>
	/// Add this camera to the list.
	/// </summary>

	void Start ()
	{
		// We should be using touch-based input on Android and iOS-based devices.
		mUseTouchInput = Application.platform == RuntimePlatform.Android ||
						 Application.platform == RuntimePlatform.IPhonePlayer;

		// Add this camera to the list
		mList.Add(this);
		mList.Sort(CompareFunc);
	}

	/// <summary>
	/// Remove this camera from the list.
	/// </summary>

	void OnDestroy ()
	{
		mList.Remove(this);
	}

	/// <summary>
	/// Update the object under the mouse if we're not using touch-based input.
	/// </summary>

	void FixedUpdate ()
	{
		if (Application.isPlaying && !mUseTouchInput && handlesEvents)
		{
			mMouse = GetObject(Input.mousePosition);
		}
	}

	/// <summary>
	/// Check the input and send out appropriate events.
	/// </summary>

	void Update ()
	{
		// Only the first UI layer should be processing events
		if (!Application.isPlaying || !handlesEvents) return;

		bool pressed = false;
		bool unpressed = false;
		
		if (mUseTouchInput)
		{
			if (Input.touchCount > 0)
			{
				// Figure out what we're touching
				Touch touch = Input.touches[0];
				pressed = (touch.phase == TouchPhase.Began);
				unpressed = (touch.phase == TouchPhase.Canceled) || (touch.phase == TouchPhase.Ended);
				mPos = Input.touches[0].position;
				mDelta = Input.touches[0].deltaPosition;
				if (pressed || unpressed) mMouse = GetObject(mPos);
			}
			else
			{
				// Nothing being touched -- no object under the cursor
				mMouse = null;
				mDelta = Vector3.zero;
			}
		}
		else
		{
			pressed = Input.GetMouseButtonDown(0);
			unpressed = Input.GetMouseButtonUp(0);
			Vector3 pos = Input.mousePosition;
			mDelta = pos - mPos;

			if (mPos != pos)
			{
				if (mTooltipTime != 0f) mTooltipTime = Time.time + tooltipDelay;
				else if (mTooltip != null) ShowTooltip(false);
				mPos = pos;
			}
		}

		// If we're using the mouse for input, we should send out a hover(false) message first
		if (!mUseTouchInput && mDown == null && mHover != mMouse && mHover != null)
		{
			if (mTooltip != null) ShowTooltip(false);
			mHover.SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
		}

		// Send the drag notification, intentionally before the pressed object gets changed
		if (mDown != null && mDelta.magnitude != 0f)
		{
			if (mTooltip != null) ShowTooltip(false);
			mTotalDelta += mDelta;
			mDown.SendMessage("OnDrag", mDelta, SendMessageOptions.DontRequireReceiver);

			float threshold = (Application.platform == RuntimePlatform.Android ||
				Application.platform == RuntimePlatform.IPhonePlayer) ? 30f : 5f;
			
			if (mTotalDelta.magnitude > threshold) mConsiderForClick = false;
		}

		// Send out the press message
		if (pressed)
		{
			if (mTooltip != null) ShowTooltip(false);
			mDown = mMouse;
			mConsiderForClick = true;
			mTotalDelta = Vector3.zero;
			if (mDown != null) mDown.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
		}

		// Clear the selection
		if ((mSel != null) && (pressed || Input.GetKeyDown(KeyCode.Escape)))
		{
			if (mTooltip != null) ShowTooltip(false);
			mSel.SendMessage("OnSelect", false, SendMessageOptions.DontRequireReceiver);
			mSel = null;
		}

		// Send out the unpress message
		if (unpressed)
		{
			if (mTooltip != null) ShowTooltip(false);

			if (mDown != null)
			{
				mDown.SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);

				// If the button/touch was released on the same object, consider it a click and select it
				if (mDown == mMouse)
				{
					mSel = mDown;
					mDown.SendMessage("OnSelect", true, SendMessageOptions.DontRequireReceiver);
					if (mConsiderForClick) mDown.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
				}
				// The button/touch was released on a different object, send a hover(false) message
				else if (!mUseTouchInput) mDown.SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
			}
			mDown = null;
			mHover = null;
		}

		// Send out a hover(true) message last
		if (!mUseTouchInput && mDown == null && mHover != mMouse)
		{
			mTooltipTime = Time.time + tooltipDelay;
			mHover = mMouse;
			if (mHover != null) mHover.SendMessage("OnHover", true, SendMessageOptions.DontRequireReceiver);
		}

		// Forward the input to the selected object
		if (mSel != null && Input.inputString.Length > 0)
		{
			if (mTooltip != null) ShowTooltip(false);
			mSel.SendMessage("OnInput", Input.inputString, SendMessageOptions.DontRequireReceiver);
		}

		// If it's time to show a tooltip, inform the object we're hovering over
		if (!mUseTouchInput && mHover != null && mTooltipTime != 0f && mTooltipTime < Time.time)
		{
			mTooltip = mHover;
			ShowTooltip(true);
		}
	}

	/// <summary>
	/// Show or hide the tooltip.
	/// </summary>

	void ShowTooltip (bool val)
	{
		mTooltipTime = 0f;
		mTooltip.SendMessage("OnTooltip", val, SendMessageOptions.DontRequireReceiver);
		if (!val) mTooltip = null;
	}
}