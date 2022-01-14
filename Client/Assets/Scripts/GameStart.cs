using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    private NetManager mNetManager = NetManager.Instance;
    void Start()
    {
        mNetManager.Connect("172.12.12.30", 8011);
    }

    void Update()
    {
        mNetManager.Update();
    }

    private void OnApplicationQuit()
    {
        mNetManager.Close();
    }
}
