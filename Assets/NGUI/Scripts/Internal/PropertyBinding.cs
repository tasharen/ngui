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

	public enum Direction
	{
		SourceUpdatesTarget,
		TargetUpdatesSource,
		BiDirectional,
	}

	/// <summary>
	/// Direction of updates.
	/// </summary>

	public Direction direction = Direction.SourceUpdatesTarget;

	/// <summary>
	/// When the property update will occur.
	/// </summary>

	public UpdateCondition update = UpdateCondition.OnUpdate;
	public PropertyReference source;
	public PropertyReference target;
	public bool editMode = true;

	object mLastValue = null;

	void Start ()
	{
		UpdateTarget();
		if (update == UpdateCondition.OnStart) enabled = false;
	}

	void Update ()
	{
		if (update == UpdateCondition.OnUpdate) UpdateTarget();
#if UNITY_EDITOR
		else if (editMode && !Application.isPlaying) UpdateTarget();
#endif
	}

	void LateUpdate ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		if (update == UpdateCondition.OnLateUpdate) UpdateTarget();
	}

	void FixedUpdate ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		if (update == UpdateCondition.OnFixedUpdate) UpdateTarget();
	}

	/// <summary>
	/// Immediately update the bound data.
	/// </summary>

	[ContextMenu("Update Now")]
	public void UpdateTarget ()
	{
		if (source != null && target != null && source.isValid && target.isValid)
		{
			if (direction == Direction.SourceUpdatesTarget)
			{
				target.Set(source.Get());
			}
			else if (direction == Direction.TargetUpdatesSource)
			{
				source.Set(target.Get());
			}
			else
			{
				object current = source.Get();

				if (mLastValue == null || !mLastValue.Equals(current))
				{
					mLastValue = current;
					target.Set(current);
				}
				else
				{
					current = target.Get();

					if (!mLastValue.Equals(current))
					{
						mLastValue = current;
						source.Set(current);
					}
				}
			}
		}
	}
}
