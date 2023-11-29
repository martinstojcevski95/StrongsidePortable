using StrongSideAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{

    public static LoadingManager Instance;

    [SerializeField] Canvas LoadingScreen;
    [SerializeField] TMP_Text loadingText;


    private void Awake()
    {
        Instance = this;
    }


    public void CloseLoadingScreen(string message, float time = 0)
    {
        LoadingScreen.enabled = true;
        loadingText.text = message;
        Invoke("DelayedClearLoading", time);
    }

    public void OpenLoading(string msg)
    {
        loadingText.text = msg;
        LoadingScreen.enabled = true;
    }

    public void DelayedClearLoading()
    {
        loadingText.text = "";
        LoadingScreen.enabled = false;
    }

}
