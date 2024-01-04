using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MAGES.XRPlotLib
{
    public class Tooltip : MonoBehaviour
    {
        public void InitializeTooltip(string text)
        {
            transform.GetChild(0).GetComponent<TextMesh>().text = text;
        }
    }
}