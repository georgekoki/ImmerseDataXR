using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MAGES.XRPlotLib
{
    public class DataPointBehaviour : MonoBehaviour
    {
        public DataPoint dataPoint;

        private Renderer rend;
        private KeepScale keepScale;
        private int isAnimatinngStack = 0;

        public bool useTooltip = false;

        public GameObject tooltipPrefab;
        private GameObject tooltipInstance;

        private void Awake()
        {
            rend = GetComponentInChildren<Renderer>();
            keepScale = GetComponent<KeepScale>();

            if (!rend)
            {
                Debug.LogError("[XRPlotLib] Malformed prefab");
                Destroy(this);
            }

            rend.enabled = false;
            if(keepScale != null)
            {
                keepScale.enabled = false;
            }

            if(tooltipPrefab == null)
            {
                Debug.LogWarning("[XRPlotLib] Tooltip reference not found.");
                useTooltip = false;
            }
        }

        private void Start()
        {
            StartCoroutine(SetDataPointVisibility(true));
        }

        public void SetActive(bool active)
        {
            rend.enabled = active;
            if (keepScale != null)
            {
                keepScale.enabled = active;
            }
        }

        public void SetPosition(Vector3 position)
        {
            transform.localPosition = position;
        }

        public void SetColor(Color color)
        {
            rend.materials[0].color = color;
        }

        public void SetOutlineColor(Color color)
        {
            if(rend.materials.Length > 3)
            {
                rend.materials[2].SetColor("_OutlineColor", color);
            }

        }

        public void SetTooltip(bool active)
        {
            if (!useTooltip) return;

            if (rend.enabled == false) return;

            if (active)
            {
                if (tooltipInstance != null) return;

                tooltipInstance = Instantiate(tooltipPrefab, transform);
                tooltipInstance.GetComponent<Tooltip>().InitializeTooltip(dataPoint.ToString());
            }
            else
            {
                if (tooltipInstance) Destroy(tooltipInstance);
            }
        }
        
        [ContextMenu("Destroy")]
        public void DestroyDataPoint()
        {
            StartCoroutine(SetDataPointVisibility(false));
        }

        #region Intro & Outro Animation 
        
        private IEnumerator SetDataPointVisibility(bool enable)
        {
            string colorName;
            Color initialColor;

            Material[] materials = rend.materials;

            yield return new WaitUntil(() => isAnimatinngStack == 0);

            foreach (Material handleMaterial in materials)
            {
                colorName = DetermineMaterialColorProperty(handleMaterial);
                if (colorName == null) continue;

                initialColor = handleMaterial.GetColor(colorName);

                if (enable)
                {
                    handleMaterial.SetColor(colorName, new Color(
                        initialColor.r,
                        initialColor.g,
                        initialColor.b, -0.5f));
                }

                StartCoroutine(AnimateDataPointVisibility(enable, handleMaterial, initialColor, colorName));
            }

            yield break;
        }

        private IEnumerator AnimateDataPointVisibility(bool enable, Material handleMaterial, Color initialColor, string colorName)
        {
            isAnimatinngStack++;
            if (!enable)
            {
                while (handleMaterial.GetColor(colorName).a >= 0)
                {
                    handleMaterial.SetColor(colorName, new Color(
                        initialColor.r,
                        initialColor.g,
                        initialColor.b,
                        handleMaterial.GetColor(colorName).a - 0.01f * Time.deltaTime * 70));
                    yield return null;
                }

                handleMaterial.SetColor(colorName, new Color(
                        initialColor.r,
                        initialColor.g,
                        initialColor.b,
                        0.0f));

                Destroy(this.gameObject);

                yield break;
            }
            else
            {
                while (handleMaterial.GetColor(colorName).a <= initialColor.a)
                {
                    handleMaterial.SetColor(colorName, new Color(
                        initialColor.r,
                        initialColor.g,
                        initialColor.b,
                        handleMaterial.GetColor(colorName).a + 0.01f * Time.deltaTime * 70));
                    yield return null;
                }

                handleMaterial.SetColor(colorName, initialColor);

                isAnimatinngStack--;
                yield break;

            }

        }
        
        private string DetermineMaterialColorProperty(Material handleMaterial)
        {
            if (handleMaterial.HasProperty("_BaseColor"))
            {
                return "_BaseColor";
            }
            else if (handleMaterial.HasProperty("_FaceColor"))
            {
                return "_FaceColor";
            }
            else if (handleMaterial.HasProperty("_OutlineColor"))
            {
                return "_OutlineColor";
            }

            return null;
        }

        #endregion
    }

}

