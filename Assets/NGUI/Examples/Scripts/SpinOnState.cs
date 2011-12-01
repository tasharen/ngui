using UnityEngine;

[RequireComponent(typeof(Spin))]
[AddComponentMenu("Examples/Spin On State")]
public class SpinOnState : MonoBehaviour
{
	void OnState (int state)
	{
		Spin spin = GetComponent<Spin>();
		if (spin != null) spin.enabled = (state == 1);
	}
}