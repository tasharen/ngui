using UnityEngine;

namespace AnimationOrTween
{
	public enum Trigger
	{
		OnClick,
		OnHover,
		OnPress,
	}

	public enum Direction
	{
		Reverse = -1,
		Toggle = 0,
		Forward = 1,
	}

	public enum EnableCondition
	{
		DoNothing = 0,
		EnableThenPlay,
	}

	public enum DisableCondition
	{
		DisableAfterReverse = -1,
		DoNotDisable = 0,
		DisableAfterForward = 1,
	}
}