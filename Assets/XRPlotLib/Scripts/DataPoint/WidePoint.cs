using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WidePoint : MonoBehaviour
{
    Transform parent;

    private void Awake()
    {
        parent = transform.parent;
    }

    void Update()
    {
        float offset = parent.parent.localPosition.y;

        if (offset > 0)
        {
            offset = 0;
        }

        float resultValue;

        if (parent.localPosition.y + offset < 0 && parent.localPosition.y > 0)
        {
            resultValue = 0;
        }
        else
        {
            resultValue = parent.localPosition.y + offset;
        }

        transform.localScale = new Vector3(
            transform.localScale.x, resultValue, transform.localScale.z);

        transform.localPosition = new Vector3(
            transform.localPosition.x, -(resultValue) / 2, transform.localPosition.z);
    }
}
