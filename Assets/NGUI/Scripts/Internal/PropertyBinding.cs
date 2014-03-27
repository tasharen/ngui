//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Property binding lets you bind two fields or properties so that changing one will update the other.
/// </summary>

[ExecuteInEditMode]
public class PropertyBinding : MonoBehaviour
{
	public enum UpdateCondition
	{
		OnStart,
		OnUpdate,
		OnLateUpdate,
		OnFixedUpdate,
	}

	public UpdateCondition update = UpdateCondition.OnUpdate;
	public PropertyReference source;
	public PropertyReference target;

	void Start () { UpdateTarget(); }

	void Update ()
	{
		if (update == UpdateCondition.OnUpdate) UpdateTarget();
#if UNITY_EDITOR
		else if (!Application.isPlaying) UpdateTarget();
#endif
	}

	void LateUpdate () { if (update == UpdateCondition.OnLateUpdate) UpdateTarget(); }
	void FixedUpdate () { if (update == UpdateCondition.OnFixedUpdate) UpdateTarget(); }

	public void UpdateTarget ()
	{
		if (source != null && target != null && source.isValid && target.isValid)
			target.Set(source.Get());
	}
}
