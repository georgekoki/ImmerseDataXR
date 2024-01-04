using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAGES.XRPlotLib
{
    public class KeepScale : MonoBehaviour
    {
        public bool freezeXScale = false, freezeYScale = false, freezeZScale = false;

        private Vector3 initialScale;

        private void Start()
        {
            initialScale = transform.localScale;
        }

        public void SetNewScale(Vector3 newScale)
        {
            initialScale = newScale;
        }

        void FixedUpdate()
        {
            // TODO: Optimize a little
            if (transform.hasChanged)
            {
                transform.localScale =
                   new Vector3(
                       (freezeXScale ? initialScale.x * 1 / transform.parent.localScale.x : transform.localScale.x),
                       (freezeYScale ? initialScale.y * 1 / transform.parent.localScale.y : transform.localScale.y),
                       (freezeZScale ? initialScale.z * 1 / transform.parent.localScale.z : transform.localScale.z));
            }
        }
    }
}