using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnLocalAxis : MonoBehaviour
{
    public bool freezeX = false, freezeY = false, freezeZ = false;

    private Vector3 initialLocalPosition = Vector3.zero;

    public void Awake()
    {
        initialLocalPosition = transform.localPosition;
    }

    private void FixedUpdate()
    {
        transform.localPosition = new Vector3(
                freezeX ? initialLocalPosition.x : transform.localPosition.x,
                freezeY ? initialLocalPosition.y : transform.localPosition.y,
                freezeZ ? initialLocalPosition.z : transform.localPosition.z
            );
    }
}
