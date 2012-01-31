using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI Panel is responsible for collecting, sorting and updating widgets in addition to generating widgets' geometry.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Panel")]
public class UIPanel : MonoBehaviour
{
	public enum DebugInfo
	{
		None,
		Gizmos,
		Geometry,
	}

	// Whether normals and tangents will be generated for all meshes
	public bool generateNormals = false;

	// Whether generated geometry is shown or hidden
	[SerializeField] DebugInfo mDebugInfo = DebugInfo.Gizmos;

#if UNITY_FLASH // Unity 3.5b6 is bugged when SerializeField is mixed with prefabs (after LoadLevel)
	// Clipping rectangle
	public UIDrawCall.Clipping mClipping = UIDrawCall.Clipping.None;
	public Vector4 mClipRange = Vector4.zero;
	public Vector2 mClipSoftness = new Vector2(40f, 40f);
#else
	// Clipping rectangle
	[SerializeField] UIDrawCall.Clipping mClipping = UIDrawCall.Clipping.None;
	[SerializeField] Vector4 mClipRange = Vector4.zero;
	[SerializeField] Vector2 mClipSoftness = new Vector2(40f, 40f);
#endif

	// List of managed transforms
	Dictionary<Transform, UINode> mChildren = new Dictionary<Transform, UINode>();

	// List of all widgets managed by this panel
	List<UIWidget> mWidgets = new List<UIWidget>();

	// Widgets using these materials will be rebuilt next frame
	List<Material> mChanged = new List<Material>();

	// List of UI Screens created on hidden and invisible game objects
	List<UIDrawCall> mDrawCalls = new List<UIDrawCall>();

	// Cached in order to reduce memory allocations
	List<Vector3> mVerts = new List<Vector3>();
	List<Vector3> mNorms = new List<Vector3>();
	List<Vector4> mTans = new List<Vector4>();
	List<Vector2> mUvs = new List<Vector2>();
	List<Color> mCols = new List<Color>();

	Transform mTrans;
	Camera mCam;
	int mLayer = -1;
	bool mDepthChanged = false;
	bool mRebuildAll = false;

	float mMatrixTime = 0f;
	Matrix4x4 mWorldToLocal = Matrix4x4.identity;

	// Values used for visibility checks
	static float[] mTemp = new float[4];
	Vector2 mMin = Vector2.zero;
	Vector2 mMax = Vector2.zero;

	// When traversing through the child dictionary, deleted values are stored here
	List<Transform> mRemoved = new List<Transform>();

#if UNITY_EDITOR
	// Screen size, saved for gizmos, since Screen.width and Screen.height returns the Scene view's dimensions in OnDrawGizmos.
	Vector2 mScreenSize = Vector2.one;
#endif

	/// <summary>
	/// Cached for speed.
	/// </summary>

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	/// <summary>
	/// Whether the panel's generated geometry will be hidden or not.
	/// </summary>

	public DebugInfo debugInfo
	{
		get
		{
			return mDebugInfo;
		}
		set
		{
			if (mDebugInfo != value)
			{
				mDebugInfo = value;
				List<UIDrawCall> list = drawCalls;
				HideFlags flags = (mDebugInfo == DebugInfo.Geometry) ? HideFlags.DontSave | HideFlags.NotEditable : HideFlags.HideAndDontSave;

				foreach (UIDrawCall dc in list)
				{
					GameObject go = dc.gameObject;
					go.active = false;
					go.hideFlags = flags;
					go.active = true;
				}
			}
		}
	}

	/// <summary>
	/// Clipping method used by all draw calls.
	/// </summary>

	public UIDrawCall.Clipping clipping
	{
		get
		{
			return mClipping;
		}
		set
		{
			if (mClipping != value)
			{
				mClipping = value;
				UpdateDrawcalls();
			}
		}
	}

	/// <summary>
	/// Rectangle used for clipping (used with a valid shader)
	/// </summary>

	public Vector4 clipRange
	{
		get
		{
			return mClipRange;
		}
		set
		{
			if (mClipRange != value)
			{
				mClipRange = value;
				UpdateDrawcalls();
			}
		}
	}

	/// <summary>
	/// Clipping softness is used if the clipped style is set to "Soft".
	/// </summary>

	public Vector2 clipSoftness { get { return mClipSoftness; } set { if (mClipSoftness != value) { mClipSoftness = value; UpdateDrawcalls(); } } }

	/// <summary>
	/// Widgets managed by this panel.
	/// </summary>

	public List<UIWidget> widgets { get { return mWidgets; } }

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
				if (dc == null) mDrawCalls.RemoveAt(i);
			}
			return mDrawCalls;
		}
	}

	/// <summary>
	/// Helper function to retrieve the node of the specified transform.
	/// </summary>

	UINode GetNode (Transform t)
	{
		UINode node = null;
		if (t != null) mChildren.TryGetValue(t, out node);
		return node;
	}

	/// <summary>
	/// Returns whether the specified rectangle is visible by the panel. The coordinates must be in world space.
	/// </summary>

	bool IsVisible (Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		UpdateTransformMatrix();

		// Transform the specified points from world space to local space
		a = mWorldToLocal.MultiplyPoint3x4(a);
		b = mWorldToLocal.MultiplyPoint3x4(b);
		c = mWorldToLocal.MultiplyPoint3x4(c);
		d = mWorldToLocal.MultiplyPoint3x4(d);

		mTemp[0] = a.x;
		mTemp[1] = b.x;
		mTemp[2] = c.x;
		mTemp[3] = d.x;

		float minX = Mathf.Min(mTemp);
		float maxX = Mathf.Max(mTemp);

		mTemp[0] = a.y;
		mTemp[1] = b.y;
		mTemp[2] = c.y;
		mTemp[3] = d.y;

		float minY = Mathf.Min(mTemp);
		float maxY = Mathf.Max(mTemp);

		if (maxX < mMin.x) return false;
		if (maxY < mMin.y) return false;
		if (minX > mMax.x) return false;
		if (minY > mMax.y) return false;
		return true;
	}

	/// <summary>
	/// Returns whether the specified widget is visible by the panel.
	/// </summary>

	public bool IsVisible (UIWidget w)
	{
		if (!w.enabled || !w.gameObject.active || w.mainTexture == null || w.color.a < 0.001f) return false;

		// No clipping? No point in checking.
		if (mClipping == UIDrawCall.Clipping.None) return true;

		Vector2 size = w.relativeSize;
		Vector2 a = Vector2.Scale(w.pivotOffset, size);
		Vector2 b = a;

		a.x += size.x;
		a.y -= size.y;

		// Transform coordinates into world space
		Transform wt = w.cachedTransform;
		Vector3 v0 = wt.TransformPoint(a);
		Vector3 v1 = wt.TransformPoint(new Vector2(a.x, b.y));
		Vector3 v2 = wt.TransformPoint(new Vector2(b.x, a.y));
		Vector3 v3 = wt.TransformPoint(b);
		return IsVisible(v0, v1, v2, v3);
	}

	/// <summary>
	/// Helper function that marks the specified material as having changed so its mesh is rebuilt next frame.
	/// </summary>

	public void MarkMaterialAsChanged (Material mat, bool sort)
	{
		if (mat != null)
		{
			if (sort) mDepthChanged = true;
			if (!mChanged.Contains(mat)) mChanged.Add(mat);
		}
	}

	/// <summary>
	/// Add the specified transform to the managed list.
	/// </summary>

	UINode AddTransform (Transform t)
	{
		UINode node = null;
		UINode retVal = null;

		// Add transforms all the way up to the panel
		while (t != null && t != cachedTransform)
		{
			// If the node is already managed, we're done
			if (mChildren.TryGetValue(t, out node))
			{
				if (retVal == null) retVal = node;
				break;
			}
			else
			{
				// The node is not yet managed -- add it to the list
				node = new UINode(t);
				if (retVal == null) retVal = node;
				node.parent = t.parent;
				mChildren.Add(t, node);
				t = t.parent;
			}
		}
		return retVal;
	}

	/// <summary>
	/// Remove the specified transform from the managed list.
	/// </summary>

	void RemoveTransform (Transform t)
	{
		if (t != null)
		{
			Debug.Log("Removing " + NGUITools.GetHierarchy(t.gameObject), this);

			while (mChildren.Remove(t))
			{
				t = t.parent;
				if (t == null || t == cachedTransform || t.GetComponentInChildren<UIWidget>() != null) break;
			}
		}
	}

	/// <summary>
	/// Add the specified widget to the managed list.
	/// </summary>

	public void AddWidget (UIWidget w)
	{
		if (w != null)
		{
			UINode node = AddTransform(w.cachedTransform);

			if (node != null)
			{
				node.widget = w;

				if (!mWidgets.Contains(w))
				{
					mWidgets.Add(w);
					if (!mChanged.Contains(w.material)) mChanged.Add(w.material);
					mDepthChanged = true;
				}
			}
			else
			{
				Debug.LogError("Unable to find an appropriate root to add the widget.\n" +
					"Please make sure that there is at least one game object above this widget!", this);
			}
		}
	}

	/// <summary>
	/// Remove the specified widget from the managed list.
	/// </summary>

	public void RemoveWidget (UIWidget w)
	{
		if (w != null)
		{
			// Do we have this node? Mark the widget's material as having been changed
			UINode pc = GetNode(w.cachedTransform);

			if (pc != null)
			{
				// Mark the material as having been changed
				if (pc.visibleFlag == 1 && !mChanged.Contains(w.material)) mChanged.Add(w.material);

				// Remove this transform
				RemoveTransform(w.cachedTransform);
			}
			mWidgets.Remove(w);
		}
	}

	/// <summary>
	/// Get or create a UIScreen responsible for drawing the widgets using the specified material.
	/// </summary>

	UIDrawCall GetDrawCall (Material mat, bool createIfMissing)
	{
		foreach (UIDrawCall dc in drawCalls) if (dc.material == mat) return dc;

		UIDrawCall sc = null;

		if (createIfMissing)
		{
#if UNITY_EDITOR
			// If we're in the editor, create the game object with hide flags set right away
			GameObject go = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags("_UIDrawCall [" + mat.name + "]",
				(mDebugInfo == DebugInfo.Geometry) ? HideFlags.DontSave | HideFlags.NotEditable : HideFlags.HideAndDontSave);
#else
			GameObject go = new GameObject("_UIDrawCall [" + mat.name + "]");
			go.hideFlags = HideFlags.HideAndDontSave;
#endif
			go.layer = gameObject.layer;
			sc = go.AddComponent<UIDrawCall>();
			sc.material = mat;
			mDrawCalls.Add(sc);
		}
		return sc;
	}

	/// <summary>
	/// Layer is used to ensure that if it changes, widgets get moved as well.
	/// </summary>

	void Start ()
	{
		mLayer = gameObject.layer;
		mCam = NGUITools.FindCameraForLayer(mLayer);
	}

	/// <summary>
	/// Mark all widgets as having been changed so the draw calls get re-created.
	/// </summary>

	void OnEnable ()
	{
		foreach (UIWidget w in mWidgets) AddWidget(w);
		mRebuildAll = true;
	}

	/// <summary>
	/// Destroy all draw calls we've created when this script gets disabled.
	/// </summary>

	void OnDisable ()
	{
		for (int i = mDrawCalls.Count; i > 0; )
		{
			UIDrawCall dc = mDrawCalls[--i];
			if (dc != null) DestroyImmediate(dc.gameObject);
		}
		mDrawCalls.Clear();
		mChanged.Clear();
		mChildren.Clear();
	}

	// Temporary list used in GetChangeFlag()
	static List<UINode> mHierarchy = new List<UINode>();

	/// <summary>
	/// Convenience function that figures out the panel's correct change flag by searching the parents.
	/// </summary>

	int GetChangeFlag (UINode start)
	{
		int flag = start.changeFlag;

		if (flag == -1)
		{
			Transform trans = start.trans.parent;
			UINode sub;

			// Keep going until we find a set flag
			for (;;)
			{
				// Check the parent's flag
				if (mChildren.TryGetValue(trans, out sub))
				{
					flag = sub.changeFlag;
					trans = trans.parent;

					// If the flag hasn't been set either, add this child to the hierarchy
					if (flag == -1) mHierarchy.Add(sub);
					else break;
				}
				else
				{
					flag = 0;
					break;
				}
			}

			// Update the parent flags
			foreach (UINode pc in mHierarchy) pc.changeFlag = flag;
			mHierarchy.Clear();
		}
		return flag;
	}

	/// <summary>
	/// Update the world-to-local transform matrix as well as clipping bounds.
	/// </summary>

	void UpdateTransformMatrix ()
	{
		float time = Time.time;

		if (time == 0f || mMatrixTime != time)
		{
			mMatrixTime = time;
			mWorldToLocal = cachedTransform.worldToLocalMatrix;

			if (mClipping != UIDrawCall.Clipping.None)
			{
				Vector2 size = new Vector2(mClipRange.z, mClipRange.w);

				if (size.x == 0f) size.x = (mCam == null) ? Screen.width  : mCam.pixelWidth;
				if (size.y == 0f) size.y = (mCam == null) ? Screen.height : mCam.pixelHeight;

				size *= 0.5f;

				mMin.x = mClipRange.x - size.x;
				mMin.y = mClipRange.y - size.y;
				mMax.x = mClipRange.x + size.x;
				mMax.y = mClipRange.y + size.y;
			}
		}
	}

	/// <summary>
	/// Run through all managed transforms and see if they've changed.
	/// </summary>

	void UpdateTransforms ()
	{
		bool transformsChanged = false;

		// Check to see if something has changed
		foreach (KeyValuePair<Transform, UINode> child in mChildren)
		{
			UINode pc = child.Value;

			if (pc.trans == null)
			{
				mRemoved.Add(pc.trans);
				continue;
			}
			
			// If the node's parent has changed...
			if (pc.trans.parent != pc.parent)
			{
				Transform parent = pc.trans.parent;

				if (parent != null)
				{
					// Find the panel responsible for this new parent
					UIPanel panel = NGUITools.FindInChildren<UIPanel>(parent.gameObject);

					// If the panel has changed...
					if (panel != this)
					{
						// Remove this transform from the managed list
						mRemoved.Add(pc.trans);

						// If there is a widget present
						if (pc.widget != null)
						{
							// Mark the material as having changed and remove the widget from the managed list
							MarkMaterialAsChanged(pc.widget.material, false);
							mWidgets.Remove(pc.widget);

							// Add this widget to the other panel
							if (panel != null)
							{
								pc.widget.panel = null;
								pc.widget.MarkAsChanged();
							}
						}
						continue;
					}
				}
			}
			
			if (pc.HasChanged())
			{
				pc.changeFlag = 1;
				transformsChanged = true;
			}
			else pc.changeFlag = -1;
		}

		// Clean up deleted transforms
		foreach (Transform rem in mRemoved) mChildren.Remove(rem);
		mRemoved.Clear();

		// If something has changed, propagate the changes *down* the tree hierarchy (to children).
		// An alternative (but slower) approach would be to do a pc.trans.GetComponentsInChildren<UIWidget>()
		// in the loop above, and mark each one as dirty.

		if (transformsChanged || mRebuildAll)
		{
			foreach (KeyValuePair<Transform, UINode> child in mChildren)
			{
				UINode pc = child.Value;

				if (pc.widget != null)
				{
					// If the change flag has not yet been determined...
					if (pc.changeFlag == -1) pc.changeFlag = GetChangeFlag(pc);

					if (pc.changeFlag == 1)
					{
						// Is the widget visible?
						int visibleFlag = IsVisible(pc.widget) ? 1 : 0;

						// If the widget is visible (or the flag hasn't been set yet)
						if (visibleFlag == 1 || pc.visibleFlag != 0)
						{
							// Update the visibility flag
							pc.visibleFlag = visibleFlag;
							Material mat = pc.widget.material;

							// Add this material to the list of changed materials
							if (!mChanged.Contains(mat)) mChanged.Add(mat);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Update all widgets and rebuild their geometry if necessary.
	/// </summary>

	void UpdateWidgets ()
	{
		foreach (KeyValuePair<Transform, UINode> c in mChildren)
		{
			UINode pc = c.Value;

			// If the widget is visible, update it
			if (pc.visibleFlag == 1 && pc.widget != null)
			{
				if (pc.widget.PanelUpdate() || pc.verts == null || pc.verts.Count == 0)
				{
					// Rebuild the widget's geometry
					Vector3 offset = pc.widget.pivotOffset;
					Vector2 scale = pc.widget.relativeSize;
					offset.x *= scale.x;
					offset.y *= scale.y;
					pc.Rebuild(offset);

					// We will need to refill this buffer
					if (!mChanged.Contains(pc.widget.material)) mChanged.Add(pc.widget.material);
				}

				// If we have vertices to work with, transform them
				if ((pc.verts != null && pc.verts.Count > 0) &&
					(pc.changeFlag == 1 || pc.rtpVerts == null || pc.rtpVerts.Count != pc.verts.Count))
				{
					pc.TransformVerts(mWorldToLocal * pc.trans.localToWorldMatrix, generateNormals);
				}
			}
		}
	}

	/// <summary>
	/// Update the clipping rect in the shaders and draw calls' positions.
	/// </summary>

	void UpdateDrawcalls ()
	{
		Vector4 range = Vector4.zero;

		if (mClipping != UIDrawCall.Clipping.None)
		{
			range = new Vector4(mClipRange.x, mClipRange.y, mClipRange.z * 0.5f, mClipRange.w * 0.5f);
		}

		if (range.z == 0f) range.z = Screen.width * 0.5f;
		if (range.w == 0f) range.w = Screen.height * 0.5f;

		RuntimePlatform platform = Application.platform;

		if (platform == RuntimePlatform.WindowsPlayer ||
			platform == RuntimePlatform.WindowsWebPlayer ||
			platform == RuntimePlatform.WindowsEditor)
		{
			range.x -= 0.5f;
			range.y += 0.5f;
		}

		Transform t = cachedTransform;

		foreach (UIDrawCall dc in mDrawCalls)
		{
			dc.clipping = mClipping;
			dc.clipRange = range;
			dc.clipSoftness = mClipSoftness;

			// Set the draw call's transform to match the panel's.
			// Note that parenting directly to the panel causes unity to crash as soon as you hit Play.
			Transform dt = dc.transform;
			dt.position = t.position;
			dt.rotation = t.rotation;
			dt.localScale = t.lossyScale;
		}
	}

	/// <summary>
	/// Set the draw call's geometry responsible for the specified material.
	/// </summary>

	void Fill (Material mat)
	{
		// Cleanup deleted widgets
		for (int i = mWidgets.Count; i > 0; ) if (mWidgets[--i] == null) mWidgets.RemoveAt(i);

		// Fill the buffers for the specified material
		foreach (UIWidget w in mWidgets)
		{
			if (w.visibleFlag == 1 && w.material == mat)
			{
				UINode node = GetNode(w.cachedTransform);

				if (node != null)
				{
					if (generateNormals) node.Fill(mVerts, mUvs, mCols, mNorms, mTans);
					else node.Fill(mVerts, mUvs, mCols, null, null);
				}
				else
				{
					Debug.LogError("No transform found for " + NGUITools.GetHierarchy(w.gameObject), this);
				}
			}
		}

		if (mVerts.Count > 0)
		{
			// Rebuild the draw call's mesh
			UIDrawCall dc = GetDrawCall(mat, true);
			dc.Set(mVerts, generateNormals ? mNorms : null, generateNormals ? mTans : null, mUvs, mCols);
		}
		else
		{
			// There is nothing to draw for this material -- eliminate the draw call
			UIDrawCall dc = GetDrawCall(mat, false);

			if (dc != null)
			{
				mDrawCalls.Remove(dc);
				DestroyImmediate(dc.gameObject);
			}
		}

		// Cleanup
		mVerts.Clear();
		mNorms.Clear();
		mTans.Clear();
		mUvs.Clear();
		mCols.Clear();
	}

	/// <summary>
	/// Update all widgets and rebuild the draw calls if necessary.
	/// </summary>

	public void LateUpdate ()
	{
		UpdateTransformMatrix();
		UpdateTransforms();

		// Always move widgets to the panel's layer
		if (mLayer != gameObject.layer)
		{
			mLayer = gameObject.layer;
			mCam = NGUITools.FindCameraForLayer(mLayer);
			SetChildLayer(cachedTransform, mLayer);
			foreach (UIDrawCall dc in drawCalls) dc.gameObject.layer = mLayer;
		}

		UpdateWidgets();

		// If the depth has changed, we need to re-sort the widgets
		if (mDepthChanged)
		{
			mDepthChanged = false;
			mWidgets.Sort(UIWidget.CompareFunc);
		}

		// Fill the draw calls for all of the changed materials
		foreach (Material mat in mChanged) Fill(mat);

		// Update the clipping rects
		UpdateDrawcalls();
		mChanged.Clear();
		mRebuildAll = false;
#if UNITY_EDITOR
		mScreenSize = new Vector2(Screen.width, Screen.height);
	}

	/// <summary>
	/// Draw a visible pink outline for the clipped area.
	/// </summary>

	void OnDrawGizmos ()
	{
		if (mDebugInfo == DebugInfo.Gizmos && mClipping != UIDrawCall.Clipping.None)
		{
			Vector2 size = new Vector2(mClipRange.z, mClipRange.w);

			if (size.x == 0f) size.x = mScreenSize.x;
			if (size.y == 0f) size.y = mScreenSize.y;

			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(new Vector2(mClipRange.x, mClipRange.y), size);
		}
	}
#else
	}
#endif

	/// <summary>
	/// Helper function that recursively sets all childrens' game objects layers to the specified value, stopping when it hits another UIPanel.
	/// </summary>

	static void SetChildLayer (Transform t, int layer)
	{
		for (int i = 0; i < t.childCount; ++i)
		{
			Transform child = t.GetChild(i);

			if (child.GetComponent<UIPanel>() == null)
			{
				child.gameObject.layer = layer;
				SetChildLayer(child, layer);
			}
		}
	}

	/// <summary>
	/// Find the UIPanel responsible for handling the specified transform.
	/// </summary>

	static public UIPanel Find (Transform trans, bool createIfMissing)
	{
		UIPanel panel = null;

		while (panel == null && trans != null)
		{
			panel = trans.GetComponent<UIPanel>();
			if (panel != null) break;
			if (trans.parent == null) break;
			trans = trans.parent;
		}

		if (createIfMissing && panel == null)
		{
			panel = trans.gameObject.AddComponent<UIPanel>();
			SetChildLayer(panel.cachedTransform, panel.gameObject.layer);
		}
		return panel;
	}

	/// <summary>
	/// Find the UIPanel responsible for handling the specified transform, creating a new one if necessary.
	/// </summary>

	static public UIPanel Find (Transform trans) { return Find(trans, true); }
}