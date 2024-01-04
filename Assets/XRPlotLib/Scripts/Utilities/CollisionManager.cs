using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MAGES.XRPlotLib
{
    public enum StackState
    {
        True, False
    }

    public enum RunState
    {
        Init, Calc
    }

    [Serializable]
    public class ColliderStack
    {
        public string name = "";
        public int layer;
        public UnityEvent onEnter;
        public UnityEvent onExit;

        private RunState runState;
        public StackState stackState = StackState.False;
        public StackState oldStackState = StackState.False;

        public ColliderStack(int layer = 0)
        {
            this.layer = layer;
        }

        public void FixedUpdate()
        {
            if (runState == RunState.Init)
            {
                oldStackState = stackState;
                stackState = StackState.False;

                runState = RunState.Calc;
                return;
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (stackState == StackState.False)
            {
                if (other.gameObject.layer == layer)
                {
                    stackState = StackState.True;
                }
            }
        }

        public void Update()
        {
            if (oldStackState == StackState.True && stackState == StackState.False)
            {
                try
                {
                    onExit.Invoke();
                    oldStackState = StackState.False;
                    stackState = StackState.False;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            if (oldStackState == StackState.False && stackState == StackState.True)
            {
                try
                {
                    onEnter.Invoke();
                    oldStackState = StackState.True;
                    stackState = StackState.True;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            runState = RunState.Init;
        }
    }

    public class CollisionManager : MonoBehaviour
    {
        public List<ColliderStack> colliderLayers;

        public void FixedUpdate()
        {
            foreach (ColliderStack layer in colliderLayers)
            {
                layer.FixedUpdate();
            }
        }

        public void OnTriggerStay(Collider other)
        {
            foreach (ColliderStack layer in colliderLayers)
            {
                layer.OnTriggerStay(other);
            }
        }

        public void Update()
        {
            foreach (ColliderStack layer in colliderLayers)
            {
                layer.Update();
            }
        }
    }
}