//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

#if UNITY_EDITOR || !UNITY_FLASH
#define REFLECTION_SUPPORT
#endif

#if REFLECTION_SUPPORT
using System.Reflection;
using System.Diagnostics;
#endif

using UnityEngine;
using System;

/// <summary>
/// Reference to a specific field or property that can be set via inspector.
/// </summary>

[System.Serializable]
public class PropertyBinding
{
	[SerializeField] Component mTarget;
	[SerializeField] string mName;

	bool mCache = true;
#if REFLECTION_SUPPORT
	FieldInfo mField;
	PropertyInfo mProperty;
#endif

	/// <summary>
	/// Event delegate's target object.
	/// </summary>

	public Component target { get { return mTarget; } set { mTarget = value; mCache = true; } }

	/// <summary>
	/// Event delegate's method name.
	/// </summary>

	public string property { get { return mName; } set { mName = value; mCache = true; } }

	/// <summary>
	/// Whether this delegate's values have been set.
	/// </summary>

	public bool isValid { get { return (mTarget != null && !string.IsNullOrEmpty(mName)); } }

	/// <summary>
	/// Whether the target script is actually enabled.
	/// </summary>

	public bool isEnabled
	{
		get
		{
			if (mTarget == null) return false;
			MonoBehaviour mb = (mTarget as MonoBehaviour);
			return (mb == null || mb.enabled);
		}
	}

	public PropertyBinding () { }
	public PropertyBinding (Component target, string fieldName)
	{
		mTarget = target;
		mName = fieldName;
	}

	/// <summary>
	/// Equality operator.
	/// </summary>

	public override bool Equals (object obj)
	{
		if (obj == null)
		{
			return !isValid;
		}
		
		if (obj is PropertyBinding)
		{
			PropertyBinding pb = obj as PropertyBinding;
			return (mTarget == pb.mTarget && string.Equals(mName, pb.mName));
		}
		return false;
	}

	static int s_Hash = "PropertyBinding".GetHashCode();

	/// <summary>
	/// Used in equality operators.
	/// </summary>

	public override int GetHashCode () { return s_Hash; }

	/// <summary>
	/// Set the delegate callback using the target and method names.
	/// </summary>

	public void Set (Component target, string methodName)
	{
		mTarget = target;
		mName = methodName;
	}

	/// <summary>
	/// Clear the event delegate.
	/// </summary>

	public void Clear ()
	{
		mTarget = null;
		mName = null;
	}

	/// <summary>
	/// Convert the delegate to its string representation.
	/// </summary>

	public override string ToString () { return ToString(mTarget, property); }

	/// <summary>
	/// Convenience function that converts the specified component + property pair into its string representation.
	/// </summary>

	static public string ToString (Component comp, string property)
	{
		if (comp != null)
		{
			string typeName = comp.GetType().ToString();
			int period = typeName.LastIndexOf('.');
			if (period > 0) typeName = typeName.Substring(period + 1);

			if (!string.IsNullOrEmpty(property)) return typeName + "." + property;
			else return typeName + ".[property]";
		}
		return null;
	}

	/// <summary>
	/// Retrieve the property's value.
	/// </summary>

	[DebuggerHidden]
	[DebuggerStepThrough]
	public object Get ()
	{
#if REFLECTION_SUPPORT
		if (mCache) Cache();

		if (mProperty != null)
		{
			if (mProperty.CanRead)
				return mProperty.GetValue(mTarget, null);
		}
		else if (mField != null)
		{
			return mField.GetValue(mTarget);
		}
#endif
		return null;
	}

	/// <summary>
	/// Assign the bound property's value.
	/// </summary>

	[DebuggerHidden]
	[DebuggerStepThrough]
	public bool Set (object value)
	{
#if REFLECTION_SUPPORT
		if (mCache) Cache();

		if (mProperty != null)
		{
			if (mProperty.CanWrite && mProperty.PropertyType.IsAssignableFrom(value.GetType()))
			{
				mProperty.SetValue(mTarget, value, null);
				return true;
			}
		}
		else if (mField != null)
		{
			if (mField.FieldType.IsAssignableFrom(value.GetType()))
			{
				mField.SetValue(mTarget, value);
				return true;
			}
		}
#endif
		return false;
	}

	/// <summary>
	/// Cache the field or property.
	/// </summary>

	[DebuggerHidden]
	[DebuggerStepThrough]
	bool Cache ()
	{
		mCache = false;

#if REFLECTION_SUPPORT
		if (mTarget != null && !string.IsNullOrEmpty(mName))
		{
			Type type = mTarget.GetType();
			mField = type.GetField(mName);
			mProperty = type.GetProperty(mName);
		}
		else
		{
			mField = null;
			mProperty = null;
		}
		return (mField != null || mProperty != null);
#else
		return false;
#endif
	}
}
