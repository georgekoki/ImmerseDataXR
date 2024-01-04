using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAGES.XRPlotLib
{
    public class PlotHandle : MonoBehaviour
    {
        private Vector3? originalPosition = null;
        public Axis axis;

        private Material handleMaterial;
        private Collider col;
        private float initialAlpha = 0.0f;

        void Start()
        {
            handleMaterial = transform.GetChild(0).GetComponent<MeshRenderer>().material;
            initialAlpha = handleMaterial.color.a;

            col = transform.GetChild(0).GetComponent<Collider>();

            if (!System.Enum.TryParse<Axis>(name, out axis))
            {
                Debug.LogError("[XRPlotLib]: Naming convention broken. Please reload the prefab.");
            }
        }

        public void BeginInteraction()
        {
            originalPosition = transform.localPosition;
        }

        public void EndInteraction()
        {
            originalPosition = null;
        }

        public void SetHandleVisibility(bool enable)
        {
            StartCoroutine(AnimateHandleVisibility(enable));
        }

        public IEnumerator AnimateHandleVisibility(bool enable)
        {
            col.enabled = enable;

            if (!enable)
            {
                yield return new WaitForSeconds(1.0f);
                while (handleMaterial.color.a >= 0)
                {
                    handleMaterial.color = new Color(
                        handleMaterial.color.r,
                        handleMaterial.color.g,
                        handleMaterial.color.b,
                        handleMaterial.color.a - 0.01f * Time.deltaTime * 10);
                    yield return null;
                }

                handleMaterial.color = new Color(
                    handleMaterial.color.r,
                        handleMaterial.color.g,
                        handleMaterial.color.b,
                        0.0f);

                yield break;
            }
            else
            {

                while (handleMaterial.color.a <= initialAlpha)
                {
                    handleMaterial.color = new Color(
                        handleMaterial.color.r,
                        handleMaterial.color.g,
                        handleMaterial.color.b,
                        handleMaterial.color.a + 0.01f * Time.deltaTime * 10);
                    yield return null;
                }

                handleMaterial.color = new Color(
                        handleMaterial.color.r,
                        handleMaterial.color.g,
                        handleMaterial.color.b,
                        initialAlpha);

                yield break;

            }
        }
    }

}
