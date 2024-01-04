using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarPoint : MonoBehaviour
{
    Transform parent;
    float resultValue;
    float offset;
    Transform plotArea;
    public float test;

    private void Awake()
    {
        parent = transform.parent;
        plotArea = parent.parent.parent.GetChild(1);
    }

    void Update()
    {
        offset =  (1 / parent.parent.localScale.y) * parent.parent.localPosition.y;

        if (parent.localPosition.y + offset < 0 && parent.localPosition.y > 0)
        {
            resultValue = 0;
        }
        else
        {
          resultValue = parent.localPosition.y + offset;
        }

        if(resultValue < 0)
        {
            resultValue = 0;
        }

        test = parent.parent.localPosition.y + parent.parent.localScale.y * parent.localPosition.y;


        if(test > plotArea.localScale.y)
        {
            resultValue = 0;
        }

        transform.localScale = new Vector3(
            transform.localScale.x, resultValue, transform.localScale.z);

        transform.localPosition = new Vector3(
            transform.localPosition.x, -(resultValue) / 2, transform.localPosition.z);
    }
}
