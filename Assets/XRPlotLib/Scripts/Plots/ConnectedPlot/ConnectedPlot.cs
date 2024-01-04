using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace MAGES.XRPlotLib
{
    public class ConnectedPlot : PlotManager<ConnectedPlotDataPoint>
    {
        public enum ColoringType
        {
            EdgeBased,
            ThicknessBased
        }

        public GameObject pointPrefab;
        public GameObject edgePrefab;
        public GameObject notePrefab;

        public string dataFilePath;

        public float maxEdgeThickness = 2.0f;
        public float minEdgeThickness = 0.5f;

        public bool showValueOnEdges = false;

        public ColoringType coloringType = ColoringType.EdgeBased;

        public IEnumerator Start()
        {
            dataSet = new ConnectedPlotDataSet();
            yield return StartCoroutine(((ConnectedPlotDataSet)dataSet).ParseDataSetAsync(dataFilePath));

            InitializePlot();

            yield return StartCoroutine(DisplayDataAsync());
        }

        public override void DisplayData() => StartCoroutine(DisplayDataAsync());

        private class InstantiatedNode
        {
            public Transform transform;
            public DataPoint dataPoint;

            public InstantiatedNode(Transform transform, DataPoint dataPoint)
            {
                this.transform = transform;
                this.dataPoint = dataPoint;
            }
        }

        public IEnumerator DisplayDataAsync()
        {
            Dictionary<string, InstantiatedNode> nodes = new();
            foreach (DataPoint point in dataSet.data)
            {
                nodes.Add(point.label, new(InstantiatePoint(point).transform, point));
                
                yield return null;
            }

            foreach ((string, string, float) edge in ((ConnectedPlotDataSet)dataSet).edges.Zip(((ConnectedPlotDataSet)dataSet).edgeThickness, (e, t) => (e.Item1, e.Item2, t)))
            {
                InstantiatedNode nodeA = nodes[edge.Item1];
                InstantiatedNode nodeB = nodes[edge.Item2];
                Transform transformA = nodeA.transform;
                Transform transformB = nodeB.transform;
                float thickness = edge.Item3;

                if(coloringType == ColoringType.EdgeBased)
                    InstantiateLine(transformA, transformB, thickness, nodeA.dataPoint.color, nodeB.dataPoint.color);
                else if(coloringType == ColoringType.ThicknessBased)
                {
                    Color colorA = new(0.93f, 0.12f, 0.25f);
                    Color colorB = new(1.0f, 1.0f, 1.0f);
                    Color colorC = new(0.022f, 0.83f, 0.36f);

                    float mappedLerpValue = (thickness + 1) / 2;

                    Color lerpedColor = Color.Lerp(colorA, colorB, mappedLerpValue);
                    lerpedColor = Color.Lerp(lerpedColor, colorC, mappedLerpValue);
                    InstantiateLine(transformA, transformB, thickness, lerpedColor, lerpedColor);
                }                   
            }

            GameObject instance;
            if(((ConnectedPlotDataSet)dataSet).notes != null)
                foreach (var note in ((ConnectedPlotDataSet)dataSet).notes)
                {
                    instance = Instantiate(notePrefab, plotData.transform);
                    instance.transform.localPosition = new Vector3(note.Value[0], note.Value[1], note.Value[2]);
                    instance.GetComponent<NoteBehaviour>().noteContent= note.Key;
                }

            SetValidityOfData(true);
        }
        public static float MapRange(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            float oldRange = oldMax - oldMin;
            float newRange = newMax - newMin;
            float scaledValue = (value - oldMin) / oldRange;
            float newValue = newMin + (scaledValue * newRange);
            return newValue;
        }
        public void InstantiateLine(Transform nodeA, Transform nodeB, float thickness, Color pointA, Color pointB)
        {
            GameObject instance;
            instance = Instantiate(edgePrefab, plotData.transform);

            instance.GetComponent<LineConnector>().pointA = nodeA;
            instance.GetComponent<LineConnector>().pointB = nodeB;
            instance.GetComponent<LineConnector>().pointAColor = pointA;
            instance.GetComponent<LineConnector>().pointBColor = pointB;
            instance.GetComponent<LineConnector>().showValue = showValueOnEdges;
            instance.GetComponent<LineConnector>().thickness = MapRange(Mathf.Abs(thickness), 0.0f, 1, minEdgeThickness, maxEdgeThickness);
            
        }

        public GameObject InstantiatePoint(DataPoint point)
        {
            GameObject instance;
            instance = Instantiate(pointPrefab, plotData.transform);
            instance.transform.localPosition = point.GetCurrentPosition(positionDimensions);
            instance.name = point.label;

            MeshRenderer rend = instance.GetComponentInChildren<MeshRenderer>();
            rend.material.color = point.GetPointColor();

            instance.transform.GetComponent<DataPointBehaviour>().dataPoint = point;

            return instance;
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
                dpb.SetPosition(((ConnectedPlotDataPoint) dpb.dataPoint).GetCurrentPosition(positionDimensions));
            }
        }

        #region Editor Debug 

        [ContextMenu("New Element Mode/Enter")]
        public void DebugEnterNewElementMode() => EnterNewElementMode();

        [ContextMenu("New Element Mode/Exit")]
        public void DebugExitNewElementMode() => ExitNewElementMode();

        [ContextMenu("Data/Fit to Viewport")]
        public void DebugFitToViewport() => FitDataToViewport();

        [ContextMenu("Data/Parse Data Set")]
        public void DebugLoadDataset()
        {
            dataSet = new ConnectedPlotDataSet();
            dataSet.ParseDataSet("");
        }

        #endregion
    }

    public class ConnectedPlotDataPoint : DataPoint
    {
        public int outgoingEdgeCount = 0;
        public int incomingEdgeCount = 0;
        public int totalEdgeCount = 0;
        public ConnectedPlotDataPoint(List<float> point, string label = "") : base(point, label) { }
        public ConnectedPlotDataPoint(List<float> point, Color color, string label = "") : base(point, color, label) { }
        public ConnectedPlotDataPoint(List<float> point, Color color, Color? colorOutline, string label = "") : base(point, color, colorOutline, label) { }

        public void SetColor(Color newColor)
        {
            color = newColor;
        }
    }

    public class ConnectedPlotEdge
    {
        public string pointA;
        public string pointB;

        public ConnectedPlotEdge(string pointA, string pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }


    public class ConnectedPlotDataSet : DataSet<ConnectedPlotDataPoint>
    {
        public List<(string, string)> edges = new List<(string, string)>();
        public List<float> edgeThickness = new List<float> ();
        public int maxTotalEdgeCount = 0;
        public int maxIncomingEdgeCount = 0;
        public int maxOutgoingEdgeCount = 0;

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
            ConnectedPlotImportedData importedData = JsonConvert.DeserializeObject<ConnectedPlotImportedData>(jsonString);

            // TODO: Make this optional choice
            // Go through all nodes, calculate min max, gather them into a list of
            // ConnectedPlotDataPoint structures so we can count the node connections later
            Dictionary<string, ConnectedPlotDataPoint> tempDict = new Dictionary<string, ConnectedPlotDataPoint>();

            var nodeKeys = importedData.nodes.Keys.ToList();
            var nodeColors = importedData.colors;

            Color nodeColor;
            for (int i = 0; i < nodeKeys.Count; i++)
            {
                string nodeName = nodeKeys[i];
                List<float> nodePosition = importedData.nodes[nodeName];

                nodeColor = new Color(
                    nodeColors[i][0],
                    nodeColors[i][1],
                    nodeColors[i][2],
                    nodeColors[i][3]
                 );

                tempDict.Add(nodeName, new ConnectedPlotDataPoint(nodePosition, nodeColor, nodeName));

                yield return null;
            }

            // Just copy the edges to our local data structure
            foreach (List<string> edge in importedData.edges)
            {
                string[] arr = edge.ToArray();
                edges.Add((arr[0], arr[1]));
            }

            notes = importedData.notes;

            // If edge thickness data exists
            if(importedData.edge_thickness != null)
                foreach(float thickness in importedData.edge_thickness)
                {
                    edgeThickness.Add(thickness);
                }
            else
                foreach((string, string) edge in edges)
                {
                    edgeThickness.Add(1);
                }

            // Asign the final data to the data variable
            data = tempDict.Values.ToArray();

            yield return null;
        }

        private Gradient GenerateGradient()
        {
            Gradient gradient = new Gradient();

            // Define color keys
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0].color = Color.blue;
            colorKeys[0].time = 0f;
            colorKeys[1].color = Color.yellow;
            colorKeys[1].time = 0.5f;
            colorKeys[2].color = Color.red;
            colorKeys[2].time = 1f;

            // Define alpha keys (optional)
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = 1f;
            alphaKeys[0].time = 0f;
            alphaKeys[1].alpha = 1f;
            alphaKeys[1].time = 1f;

            gradient.SetKeys(colorKeys, alphaKeys);

            return gradient;
        }
    }

    [Serializable]
    public class ConnectedPlotImportedData
    {
        public Dictionary<string, List<float>> nodes;
        public List<List<float>> colors;
        public List<List<string>> edges;
        public List<float> edge_thickness;
        public Dictionary<string, List<float>> notes;
    }

}