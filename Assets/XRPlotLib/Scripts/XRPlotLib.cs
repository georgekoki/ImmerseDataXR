using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MAGES.XRPlotLib
{
    public enum Axis { X, Y, Z, Root }

    [Serializable]
    public class ColumnInfo
    {
        public int index;
        public string title;
        public string description;

        public ColumnInfo(int index, string name, string description)
        {
            this.index = index;
            this.title = name;
            this.description = description;
        }
    }

    public class Utilities
    {
        public static float GetAxisFromVector(Axis ax, Vector3 vec) {

            float ret = 0.0f;

            switch (ax)
            {
                default:
                case Axis.Root:
                case Axis.X:
                    {
                        ret = vec.x;
                        break;
                    }
                case Axis.Y:
                    {
                        ret = vec.y;
                        break;
                    }
                case Axis.Z:
                    {
                        ret = vec.z;
                        break;
                    }
            }

            return ret;
        }

        public static IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds, bool isLocalPosition)
        {
            float elapsedTime = -0.5f;
            Vector3 startingPos = isLocalPosition ? objectToMove.transform.localPosition : objectToMove.transform.position;
            while (elapsedTime < seconds)
            {
                if (isLocalPosition)
                {
                    objectToMove.transform.localPosition = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
                }
                else
                {
                    objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
                }

                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            if (isLocalPosition)
            {
                objectToMove.transform.localPosition = end;
            }
            else
            {
                objectToMove.transform.position = end;
            }

        }
    }

    public abstract class PlotManager<T> : MonoBehaviour where T : DataPoint
    {

        public GameObject axisValuePrefab;

        public Vector3 positionDimensions = new Vector3(0, 1, 2);

        [HideInInspector]
        public DataSet<T> dataSet { get; set; }

        [HideInInspector]
        public bool isDataValid = false;

        [HideInInspector]
        public bool isAnimating = false;

        protected GameObject plotData;
        protected GameObject handles;
        protected GameObject plotArea;
        protected GameObject plotRoot;
        protected GameObject axisNumbers;
        protected GameObject newElementLimits;
        protected GameObject pleaseWait;

        private Dictionary<Axis, (GameObject, PlotHandle)> handleArray;

        private Dictionary<Axis, GameObject> newElementLimitsArray;

        private List<float> valuesSmall = new List<float>(),
                valuesMedium = new List<float>(),
                valuesLarge = new List<float>();

        private Dictionary<Axis, List<GameObject>> axisValues = new Dictionary<Axis, List<GameObject>>();

        private bool isNewElementMode = false;

        private Vector3[] positionsBeforeNewElement = new Vector3[6];

        public void FixedUpdate()
        {
            if (isDataValid)
            {
                TransformPlotArea();
            }
        }

        public void InitializePlot()
        {
            try
            {
                PlaneRenderer[] planeRenderers = GetComponentsInChildren<PlaneRenderer>();
                foreach (PlaneRenderer planeRenderer in planeRenderers)
                {
                    planeRenderer.SetGridVisibility(true);
                }

                plotRoot = transform.GetChild(0).gameObject;
                plotData = plotRoot.transform.GetChild(0).gameObject;
                plotArea = plotRoot.transform.GetChild(1).gameObject;
                handles = plotRoot.transform.GetChild(2).gameObject;
                newElementLimits = transform.GetChild(2).gameObject;
                axisNumbers = plotRoot.transform.GetChild(3).gameObject;
                pleaseWait = transform.GetChild(3).gameObject;

                handleArray = new Dictionary<Axis, (GameObject, PlotHandle)> {
                    {Axis.X, (handles.transform.GetChild(0).gameObject,
                    handles.transform.GetChild(0).gameObject.GetComponent<PlotHandle>()) },
                    {Axis.Y, (handles.transform.GetChild(1).gameObject,
                    handles.transform.GetChild(1).gameObject.GetComponent<PlotHandle>()) },
                    {Axis.Z, (handles.transform.GetChild(2).gameObject,
                    handles.transform.GetChild(2).gameObject.GetComponent<PlotHandle>()) },
                    {Axis.Root, (transform.GetChild(1).gameObject,
                    transform.GetChild(1).gameObject.GetComponent<PlotHandle>()) }
                };

                newElementLimitsArray = new Dictionary<Axis, GameObject> {
                    { Axis.X, newElementLimits.transform.GetChild(0).gameObject },
                    { Axis.Y, newElementLimits.transform.GetChild(1).gameObject },
                    { Axis.Z, newElementLimits.transform.GetChild(2).gameObject },
                    { Axis.Root, newElementLimits.transform.GetChild(3).gameObject }
                };

                FitDataToViewport();

            }
            catch (NullReferenceException e)
            {
                Debug.LogError("[XRPlotLib]: Prefab malformed. Please reload the prefab\nStack Trace:\n"
                    + e.Message + ": \n" + e.StackTrace);
            }
        }

        public abstract void DisplayData();

        public abstract void DestroyData();

        public void CreateAxisMarks(Axis ax)
        {
            // Destroy old axis marks

            if (axisValues.ContainsKey(ax))
            {
                foreach (GameObject parent in axisValues[ax])
                {
                    foreach (Transform mark in parent.transform)
                    {
                        Destroy(mark.gameObject);
                    }
                }
                axisValues[ax] = null;
            }

            float minValue = dataSet.GetMinValue(ax, positionDimensions);
            float maxValue = dataSet.GetMaxValue(ax, positionDimensions);

            valuesSmall = AxisValuesCreate(minValue, maxValue, 0.1f);
            valuesMedium = AxisValuesCreate(minValue, maxValue, 1.0f);
            valuesLarge = AxisValuesCreate(minValue, maxValue, 10.0f);

            InstantiateAxisValues(valuesSmall, axisNumbers.transform.GetChild((int)ax).GetChild(0), ax);
            InstantiateAxisValues(valuesMedium, axisNumbers.transform.GetChild((int)ax).GetChild(1), ax);
            InstantiateAxisValues(valuesLarge, axisNumbers.transform.GetChild((int)ax).GetChild(2), ax);

            axisValues[ax] = new List<GameObject>();
            axisValues[ax].Add(axisNumbers.transform.GetChild((int)ax).GetChild(0).gameObject);
            axisValues[ax].Add(axisNumbers.transform.GetChild((int)ax).GetChild(1).gameObject);
            axisValues[ax].Add(axisNumbers.transform.GetChild((int)ax).GetChild(2).gameObject);

        }

        public abstract void UpdateData();

        public void FitDataToViewport()
        {
            float xMax, xMin, yMax, yMin, zMax, zMin;
            xMax = dataSet.GetMaxValue(Axis.X, positionDimensions);
            yMax = dataSet.GetMaxValue(Axis.Y, positionDimensions);
            zMax = dataSet.GetMaxValue(Axis.Z, positionDimensions);

            xMin = dataSet.GetMinValue(Axis.X, positionDimensions);
            yMin = dataSet.GetMinValue(Axis.Y, positionDimensions);
            zMin = dataSet.GetMinValue(Axis.Z, positionDimensions);

            float xLen, yLen, zLen;

            xLen = Math.Abs(xMax) + Math.Abs(xMin);
            yLen = Math.Abs(yMax) + Math.Abs(yMin);
            zLen = Math.Abs(zMax) + Math.Abs(zMin);

            float maxLen = Mathf.Max(xLen, yLen, zLen);

            float currShortestAxis = Mathf.Min(
                plotArea.transform.localScale.x,
                plotArea.transform.localScale.y,
                plotArea.transform.localScale.z);

            float scale = (1 / maxLen) * 0.9f * currShortestAxis;

            plotData.transform.localScale = new Vector3(scale, scale, scale);

            plotData.transform.localPosition = new Vector3(
                GetRatio(xMin, xMax, currShortestAxis),
                GetRatio(yMin, yMax, currShortestAxis),
                GetRatio(zMin, zMax, currShortestAxis));
        }

        private float GetRatio(float a, float b, float currShortestAxis)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);
            if (a + b == 0)
                return 0;

            return (a / (a + b)) * currShortestAxis;
        }

        public void EnterNewElementMode()
        {
            isNewElementMode = true;
            isAnimating = true;
            StartCoroutine(AnimateEnterNewElementMode());
        }

        public IEnumerator AnimateEnterNewElementMode()
        {
            // Save transforms for exiting
            positionsBeforeNewElement[0] = handleArray[Axis.Root].Item1.transform.position;
            positionsBeforeNewElement[1] = handleArray[Axis.X].Item1.transform.position;
            positionsBeforeNewElement[2] = handleArray[Axis.Y].Item1.transform.position;
            positionsBeforeNewElement[3] = handleArray[Axis.Z].Item1.transform.position;
            positionsBeforeNewElement[4] = plotData.transform.localPosition;
            positionsBeforeNewElement[5] = plotData.transform.localScale;

            StartCoroutine(Utilities.MoveOverSeconds(handleArray[Axis.Root].Item1, newElementLimitsArray[Axis.Root].transform.position, 5, false));
            StartCoroutine(Utilities.MoveOverSeconds(handleArray[Axis.X].Item1, newElementLimitsArray[Axis.X].transform.position, 5, false));
            StartCoroutine(Utilities.MoveOverSeconds(handleArray[Axis.Y].Item1, newElementLimitsArray[Axis.Y].transform.position, 5, false));
            Coroutine lastCoroutine = StartCoroutine(Utilities.MoveOverSeconds(handleArray[Axis.Z].Item1, newElementLimitsArray[Axis.Z].transform.position, 5, false));

            handleArray[Axis.Root].Item2.SetHandleVisibility(false);
            handleArray[Axis.X].Item2.SetHandleVisibility(false);
            handleArray[Axis.Y].Item2.SetHandleVisibility(false);
            handleArray[Axis.Z].Item2.SetHandleVisibility(false);

            PlaneRenderer[] planeRenderers = GetComponentsInChildren<PlaneRenderer>();

            foreach (PlaneRenderer planeRenderer in planeRenderers)
            {
                planeRenderer.SetGridVisibility(false);
            }

            yield return lastCoroutine;

            FitDataToViewport();
            isAnimating = false;
        }

        public void ExitNewElementMode()
        {
            isNewElementMode = false;
            isAnimating = true;
            StartCoroutine(AnimateExitNewElementMode());
        }

        public IEnumerator AnimateExitNewElementMode()
        {
            StartCoroutine(Utilities.MoveOverSeconds(handleArray[Axis.Root].Item1, positionsBeforeNewElement[0], 2, false));
            StartCoroutine(Utilities.MoveOverSeconds(handleArray[Axis.X].Item1, positionsBeforeNewElement[1], 2, false));
            StartCoroutine(Utilities.MoveOverSeconds(handleArray[Axis.Y].Item1, positionsBeforeNewElement[2], 2, false));
            Coroutine lastcoroutine = StartCoroutine(Utilities.MoveOverSeconds(handleArray[Axis.Z].Item1, positionsBeforeNewElement[3], 2, false));

            handleArray[Axis.Root].Item2.SetHandleVisibility(true);
            handleArray[Axis.X].Item2.SetHandleVisibility(true);
            handleArray[Axis.Y].Item2.SetHandleVisibility(true);
            handleArray[Axis.Z].Item2.SetHandleVisibility(true);

            plotData.transform.localPosition = positionsBeforeNewElement[4];
            plotData.transform.localScale = positionsBeforeNewElement[5];

            yield return lastcoroutine;

            PlaneRenderer[] planeRenderers = GetComponentsInChildren<PlaneRenderer>();

            foreach (PlaneRenderer planeRenderer in planeRenderers)
            {
                planeRenderer.SetGridVisibility(true);
            }

            isAnimating = false;
        }

        protected void SetValidityOfData(bool valid)
        {
            isDataValid = valid;
            pleaseWait.SetActive(!valid);

            CreateAxisMarks(Axis.X);
            CreateAxisMarks(Axis.Z);
            CreateAxisMarks(Axis.Y);
        }

        private List<float> AxisValuesCreate(float min, float max, float step)
        {
            List<float> list = new List<float>();

            for (float i = min; i <= max; i = i + step)
            {
                i = (float)Math.Round((double)i, 1);
                list.Add(i);
            }


            if (list.Count < 2)
            {
                list.Clear();
                list.Add(min);
                list.Add(max);
            }

            return list;
        }

        private void InstantiateAxisValues(List<float> values, Transform parent, Axis ax)
        {
            GameObject instance;
            foreach (float value in values)
            {
                instance = Instantiate(axisValuePrefab, parent);

                switch (ax)
                {
                    case Axis.X:
                        {
                            instance.transform.localPosition = new Vector3(value, 0, 0);
                            instance.transform.GetChild(0).localRotation = Quaternion.Euler(90, 180, 90);
                            break;
                        }
                    case Axis.Y:
                        {
                            instance.transform.localPosition = new Vector3(0, value, 0);
                            instance.transform.GetChild(0).localRotation = Quaternion.Euler(0, 180, 0);
                            break;
                        }
                    case Axis.Z:
                        {
                            instance.transform.localPosition = new Vector3(0, 0, value);
                            instance.transform.GetChild(0).localRotation = Quaternion.Euler(90, 180, 0);
                            break;
                        }
                }

                instance.GetComponentInChildren<TextMeshPro>().text = " " + value.ToString();
            }
        }

        private void TransformPlotArea()
        {
            // TODO: Optimize to only call when the user moves the handle

            if (plotArea == null) return;

            // Plot Area Transforms
            plotArea.transform.localScale = new Vector3(
                handleArray[Axis.X].Item1.transform.localPosition.x,
                handleArray[Axis.Y].Item1.transform.localPosition.y,
                handleArray[Axis.Z].Item1.transform.localPosition.z);

            // Plot Data Transforms
            Vector3 plotDataScale = Vector3.zero;

            plotData.transform.localScale += plotDataScale;

            // TODO: Not ideal
            UpdateAxisScale(Axis.X);
            UpdateAxisScale(Axis.Y);
            UpdateAxisScale(Axis.Z);
        }

        private void UpdateAxisScale(Axis ax)
        {
            float minScale = Utilities.GetAxisFromVector(ax, plotData.transform.localScale);

            if (minScale < 0.04f)
            {
                axisValues[ax].ToArray()[0].SetActive(false);
                axisValues[ax].ToArray()[1].SetActive(false);
                axisValues[ax].ToArray()[2].SetActive(true);
            }
            else if (minScale < 0.4f)
            {
                axisValues[ax].ToArray()[0].SetActive(false);
                axisValues[ax].ToArray()[1].SetActive(true);
                axisValues[ax].ToArray()[2].SetActive(false);
            }
            else
            {
                axisValues[ax].ToArray()[0].SetActive(true);
                axisValues[ax].ToArray()[1].SetActive(false);
                axisValues[ax].ToArray()[2].SetActive(false);
            }
        }
    }

    public abstract class DataSet<T> where T : DataPoint
    {
        public T[] data { get; protected set; }
        public Dictionary<string, List<float>> notes;

        protected DataSet() { }

        public abstract void ParseDataSet(string csvDataSet);

        public abstract bool IsValidDataSet(string csvDataSet);

        public DataPoint GetMax(Axis ax, Vector3 currentDimensions) => GetMinMax(ax, true, currentDimensions);

        public DataPoint GetMin(Axis ax, Vector3 currentDimensions) => GetMinMax(ax, false, currentDimensions);

        public float GetMaxValue(Axis ax, Vector3 positionDimensions) => GetMinMaxValue(ax, true, positionDimensions);

        public float GetMinValue(Axis ax, Vector3 positionDimensions) => GetMinMaxValue(ax, false, positionDimensions);

        public float GetMinMaxValue(Axis ax, bool isMax, Vector3 positionDimensions)
        {
            DataPoint point = GetMinMax(ax, isMax, positionDimensions);

            return Utilities.GetAxisFromVector(ax, point.GetCurrentPosition(positionDimensions));
        }

        private DataPoint GetMinMax(Axis ax, bool isMax, Vector3 positionDimensions)
        {
            DataPoint ret = data[0];

            float tempRet, tempPoint;
            foreach (DataPoint point in data)
            {
                tempRet = Utilities.GetAxisFromVector(ax, ret.GetCurrentPosition(positionDimensions));
                tempPoint = Utilities.GetAxisFromVector(ax, point.GetCurrentPosition(positionDimensions));

                if (isMax)
                {
                    if (tempRet < tempPoint)
                    {
                        ret = point;
                    }
                }
                else if(!isMax)
                {
                    if (tempRet > tempPoint)
                    {
                        ret = point;
                    }
                }
            }
            return ret;
        }

        public override string ToString()
        {
            string str = "";
            int i = 0;

            foreach(T dataPoint in data)
            {
                str += "[" + i++ + "]\n" + dataPoint;
            }

            return str;
        }
    }

    public abstract class DataPoint
    {
        public List<float> point { get; protected set; }
        public string label { get; protected set; }
        public Color color { get; protected set; }
        public Color? colorOutline { get; protected set; }

        protected DataPoint()
        {
            point = new List<float>() { 0, 0, 0};
            label = "";
            color = Color.blue;
        }

        public Vector3 GetCurrentPosition(Vector3 indexes)
        {
            return new Vector3(
                (int)indexes.x == -1? 0 : this.point.ToArray()[(int)indexes.x],
                (int)indexes.y == -1? 0 : this.point.ToArray()[(int)indexes.y],
                (int)indexes.z == -1? 0 : this.point.ToArray()[(int)indexes.z]);
        }

        public Color GetPointColor()
        {
            return this.color;
        }

        public bool HasColorOutline()
        {
            if (this.colorOutline == null ||
                this.colorOutline.Equals(new Color(0, 0, 0, 0)))
                return false;

            return true;
        }

        public Color GetPointColorOutline()
        {
            if (this.HasColorOutline())
            {
                return (Color)this.colorOutline;
            }
            else
            {
                return new Color(0, 0, 0, 0);
            }
        }

        protected DataPoint(List<float> point, string label = "")
        {
            this.point = point;
            this.label = label;
            this.color = Color.blue;
            this.colorOutline = null;
        }

        protected DataPoint(List<float> point, Color color, string label = "")
        {
            this.point = point;
            this.label = label;
            this.color = color;
            this.colorOutline = null;
        }

        protected DataPoint(List<float> point, Color color, Color? colorOutline, string label = "")
        {
            this.point = point;
            this.label = label;
            this.color = color;
            this.colorOutline = colorOutline;
        }

        public override string ToString()
        {
            return label /*+ "\n" + string.Join("\n", point.ToArray());*/;
        }
    }

}
