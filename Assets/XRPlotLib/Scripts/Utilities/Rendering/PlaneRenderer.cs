using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAGES.XRPlotLib
{
    public class PlaneRenderer : MonoBehaviour
    {
        public Transform plotData;
        public Transform plotArea;

        public Axis gridType;

        private Material handleMaterial;

        private float initialAlpha = 0.0f;

        void Start()
        {
            handleMaterial = GetComponentInChildren<Renderer>().material;
            initialAlpha = handleMaterial.color.a;
            handleMaterial.color = new Color(
                handleMaterial.color.r, 
                handleMaterial.color.g, 
                handleMaterial.color.b, 
                0.0f);
        }

        void Update()
        {
            float x_scale = 1;
            float y_scale = 1;
            float x_offset = 1;
            float y_offset = 1;

            switch (gridType)
            {
                case Axis.Root:
                case Axis.X:
                    {
                        x_scale = plotArea.transform.localScale.x * 1 / plotData.transform.localScale.x/10;
                        y_scale = plotArea.transform.localScale.z * 1 / plotData.transform.localScale.y/10;

                        x_offset = -(1 / plotData.transform.localScale.x * plotData.transform.localPosition.x/10);
                        y_offset = -(1 / plotData.transform.localScale.x * plotData.transform.localPosition.z/10);
                        break;
                    }
                case Axis.Y:
                    {
                        x_scale = plotArea.transform.localScale.y * 1 / plotData.transform.localScale.x/10;
                        y_scale = plotArea.transform.localScale.x * 1 / plotData.transform.localScale.y/10;

                        x_offset = -(1 / plotData.transform.localScale.x * plotData.transform.localPosition.y)/10;
                        y_offset = -(1 / plotData.transform.localScale.x * plotData.transform.localPosition.x)/10;
                        break;
                    }
                case Axis.Z:
                    {
                        x_scale = plotArea.transform.localScale.z * 1 / plotData.transform.localScale.x/10;
                        y_scale = plotArea.transform.localScale.y * 1 / plotData.transform.localScale.y/10;

                        x_offset = -(1 / plotData.transform.localScale.x * plotData.transform.localPosition.z)/10;
                        y_offset = -(1 / plotData.transform.localScale.x * plotData.transform.localPosition.y)/10;
                        break;
                    }
            }

            handleMaterial.mainTextureScale =
                new Vector2(x_scale, y_scale);

            handleMaterial.mainTextureOffset =
                new Vector2(x_offset, y_offset);

            transform.localScale = plotArea.localScale;
        }

        public void SetGridVisibility(bool enable)
        {
            StartCoroutine(AnimateGridVisibility(enable));
        }

        public IEnumerator AnimateGridVisibility(bool enable)
        {
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