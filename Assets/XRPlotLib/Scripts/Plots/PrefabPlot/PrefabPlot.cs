using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace MAGES.XRPlotLib
{
    public class PrefabPlot : PlotManager<PrefabPlotDataPoint>
    {
        public GameObject pointPrefab;
        public Vector4 colorDimensions = new Vector4(0, 1, 2, 0);
        public Vector4 colorOutlineDimensions = new Vector4(0, 1, 2, 0);

        public string dataFilePath;

        public IEnumerator Start()
        {
            dataSet = new PrefabPlotDataSet();
            yield return StartCoroutine(((PrefabPlotDataSet)dataSet).ParseDataSetAsync(dataFilePath));

            InitializePlot();

            yield return StartCoroutine(DisplayDataAsync());
        }

        public override void DisplayData() => StartCoroutine(DisplayDataAsync());

        public IEnumerator DisplayDataAsync()
        {
            foreach (DataPoint point in dataSet.data)
            {
                InstantiatePoint(point);
                
                yield return null;
            }

            SetValidityOfData(true);
        }

        public void InstantiatePoint(DataPoint point)
        {
            GameObject instance;
            instance = Instantiate(pointPrefab, plotData.transform);
            instance.transform.localPosition = point.GetCurrentPosition(positionDimensions);

            MeshRenderer rend = instance.GetComponentInChildren<MeshRenderer>();
            rend.material.color = point.GetPointColor();

            instance.transform.GetComponent<DataPointBehaviour>().dataPoint = point;
        }


        [ContextMenu("Data/Destroy")]
        public override void DestroyData()
        {
            StartCoroutine(DestroyDataAsync());
        }

        public IEnumerator DestroyDataAsync()
        {
            SetValidityOfData(false);
            foreach (Transform dataPointTransform in plotData.transform)
            {
                dataPointTransform.gameObject.GetComponent<DataPointBehaviour>().DestroyDataPoint();
            }

            yield break;
        }

        [ContextMenu("Data/Update Data Points")]
        public override void UpdateData()
        {
            foreach(Transform dataPoint in plotData.transform)
            {
                DataPointBehaviour dpb = dataPoint.gameObject.GetComponent<DataPointBehaviour>();
                dpb.SetPosition(((PrefabPlotDataPoint) dpb.dataPoint).GetCurrentPosition(positionDimensions));
                dpb.SetColor(((PrefabPlotDataPoint)dpb.dataPoint).GetCurrentColor(colorDimensions, ((PrefabPlotDataSet) dataSet).minMaxData, false));
                //dpb.SetOutlineColor(((PrefabPlotDataPoint)dpb.dataPoint).GetCurrentColor(colorOutlineDimensions, ((PrefabPlotDataSet)dataSet).minMaxData, true));
            }
        }

        public void ChangePrefab(GameObject newPrefab)
        {
            DestroyData();
            pointPrefab = newPrefab;
            DisplayData();
        }

        #region Editor Debug 

        [ContextMenu("New Element Mode/Enter")]
        public void DebugEnterNewElementMode() => EnterNewElementMode();

        [ContextMenu("New Element Mode/Exit")]
        public void DebugExitNewElementMode() => ExitNewElementMode();

        [ContextMenu("Data/Fit to Viewport")]
        public void DebugFitToViewport() => FitDataToViewport();

        #endregion
    }

    public class PrefabPlotDataPoint : DataPoint
    {
        public PrefabPlotDataPoint(List<float> point, string label = "") : base(point, label) { }
        public PrefabPlotDataPoint(List<float> point, Color color, string label = "") : base(point, color, label) { }
        public PrefabPlotDataPoint(List<float> point, Color color, Color? colorOutline, string label = "") : base(point, color, colorOutline, label) { }

        public Color GetCurrentColor(Vector4 indexes, (float, float)[] minMaxData, bool isOutlineColor)
        {
            Vector4 normalizedIndexes = new Vector4(
                indexes.x == -1 ? 0 : indexes.x,
                indexes.y == -1 ? 0 : indexes.y,
                indexes.z == -1 ? 0 : indexes.z,
                indexes.w == -1 ? 0 : indexes.w
            );

            Vector4 currMin = new Vector4(
                minMaxData[(int)normalizedIndexes.x].Item1,
                minMaxData[(int)normalizedIndexes.y].Item1,
                minMaxData[(int)normalizedIndexes.z].Item1,
                minMaxData[(int)normalizedIndexes.w].Item1
                );

            Vector4 currMax = new Vector4(
                minMaxData[(int)normalizedIndexes.x].Item2,
                minMaxData[(int)normalizedIndexes.y].Item2,
                minMaxData[(int)normalizedIndexes.z].Item2,
                minMaxData[(int)normalizedIndexes.w].Item2
                );

            Vector4 currPoint = new Vector4(
                this.point.ToArray()[(int)normalizedIndexes.x],
                this.point.ToArray()[(int)normalizedIndexes.y],
                this.point.ToArray()[(int)normalizedIndexes.z],
                this.point.ToArray()[(int)normalizedIndexes.w]);

            Color defaultColor = new Color(0, 0, 0, 0);

            if (isOutlineColor && this.colorOutline != null)
                defaultColor = (Color)this.colorOutline;
            else if (!isOutlineColor && this.color != null)
                defaultColor = (Color)this.color;

            return new Color(
                indexes.x == -1 ? defaultColor.r : (currPoint.x - currMin.x) / (currMax.x - currMin.x),
                indexes.y == -1 ? defaultColor.g : (currPoint.y - currMin.y) / (currMax.y - currMin.y),
                indexes.z == -1 ? defaultColor.b : (currPoint.z - currMin.z) / (currMax.z - currMin.z),
                1.0f);
        }
    }

    public class PrefabPlotDataSet : DataSet<PrefabPlotDataPoint>
    {
        public (float, float)[] minMaxData; // Size of array = Number of dimensions

        public override bool IsValidDataSet(string csvDataSet)
        {
            throw new System.NotImplementedException();
        }

        public override void ParseDataSet(string csvDataSet)
        {
            PrefabSpawnManager.Pm_inst.StartCoroutine(ParseDataSetAsync(csvDataSet));
        }

        public IEnumerator ParseDataSetAsync(string csvDataSet)
        {
            TextAsset txt = (TextAsset)Resources.Load(csvDataSet, typeof(TextAsset));
            string jsonString = txt.text;

            // TODO: Add try catch here
            PrefabPlotImportedData importedData = JsonConvert.DeserializeObject<PrefabPlotImportedData>(jsonString);

            // TODO: Check that array sizes match

            int dimensionSize = 0;

            data = new PrefabPlotDataPoint[importedData.points.Count];

            for (int i = 0; i < importedData.points.Count; i++)
            {
                // Point data
                List<float> point = importedData.points.ToArray()[i];

                if(i == 0)
                {
                    dimensionSize = point.Count;
                    minMaxData = new (float, float)[dimensionSize];
                }
                else if(dimensionSize != point.Count)
                {
                    Debug.LogWarning("[XRPlotLib]: Point with ID: " + i + " not imported - invalid dimension size.");
                    continue;
                }

                Color col = Color.gray;
                Color? colOutline = null;
                string label = "";

                // Color data
                if (importedData.colors != null)
                {
                    if(importedData.colors.ToArray()[i].Count == 3)
                    {
                        col = new Color(
                            importedData.colors.ToArray()[i].ToArray()[0],
                            importedData.colors.ToArray()[i].ToArray()[1],
                            importedData.colors.ToArray()[i].ToArray()[2]);
                    } else if (importedData.colors.ToArray()[i].Count == 4)
                    {
                        col = new Color(
                            importedData.colors.ToArray()[i].ToArray()[0],
                            importedData.colors.ToArray()[i].ToArray()[1],
                            importedData.colors.ToArray()[i].ToArray()[2],
                            importedData.colors.ToArray()[i].ToArray()[3]);
                    }
                }

                if (importedData.colorsOutline != null)
                {
                    if (importedData.colorsOutline.ToArray()[i].Count == 3)
                    {
                        colOutline = new Color(
                            importedData.colorsOutline.ToArray()[i].ToArray()[0],
                            importedData.colorsOutline.ToArray()[i].ToArray()[1],
                            importedData.colorsOutline.ToArray()[i].ToArray()[2]);
                    }
                    else if (importedData.colors.ToArray()[i].Count == 4)
                    {
                        colOutline = new Color(
                            importedData.colorsOutline.ToArray()[i].ToArray()[0],
                            importedData.colorsOutline.ToArray()[i].ToArray()[1],
                            importedData.colorsOutline.ToArray()[i].ToArray()[2],
                            importedData.colorsOutline.ToArray()[i].ToArray()[3]);
                    }
                }

                // Label data
                if (importedData.labels != null)
                {
                    label = importedData.labels.ToArray()[i];                    
                }

                data[i] = new PrefabPlotDataPoint(point, col, colOutline, label);

                UpdateMinMax(point);

                yield return null;
            }
        }

        private void UpdateMinMax(List<float> newPoint)
        {
            float[] newPointArray = newPoint.ToArray();

            for(int i = 0; i < minMaxData.Length; i++)
            {
                float currNew = newPointArray[i];
                float currMin = minMaxData[i].Item1;
                float currMax = minMaxData[i].Item2;

                if(currNew < currMin)
                {
                    minMaxData[i].Item1 = currNew;
                }

                if(currNew > currMax)
                {
                    minMaxData[i].Item2 = currNew;
                }
            }
        }
    }

    [Serializable]
    public class PrefabPlotImportedData
    {
        public List<List<float>> points;
        public List<List<float>> colors;
        public List<List<float>> colorsOutline;
        public List<string> labels;

    }

}