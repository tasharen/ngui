using UnityEngine;

/// <summary>
/// Unity's LWRP could only support a single camera. Latest version of Unity no longer has this limitation,
/// but if you are stuck with an older version, you can simply use this script to position your UI underneath
/// your game's perspective-based camera. Check out Example 16 for more usage.
/// </summary>

public class PerspectivePixelPerfect : MonoBehaviour
{
    [Tooltip("Bias is a value above 0 that determines how far offset the object will be from the near clip, in percent (near to far clip)")]
    public float bias = 0.001f;

    [ContextMenu("Execute")]
	void Start ()
	{
        var t = transform;
        var c = Camera.main;
        var near = c.nearClipPlane;
        var far = c.farClipPlane;
        var distance = Mathf.Lerp(near, far, bias);
        var fov = c.fieldOfView;
        var s = Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f) * distance;
        t.localPosition = new Vector3(0f, 0f, distance);
        t.localScale = new Vector3(s, s, 1f);
    }
}
