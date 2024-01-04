using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawnManager : MonoBehaviour
{
    public static PrefabSpawnManager Pm_inst;

    public void Awake()
    {
        if (Pm_inst == null)
        {
            Pm_inst = this;
        }
        else
        {
            Destroy(this);
        }
    }

}
