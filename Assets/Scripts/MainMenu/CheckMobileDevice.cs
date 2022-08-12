using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckMobileDevice : MonoBehaviour
{
    [SerializeField] private GameObject _blockWindow;
    [SerializeField] private GameObject _defaultWindow;

    private bool _isMobile;

    private void Start()
    {
        // #if UNITY_WEBGL
        //     if (Application.isMobilePlatform)
        //     {
        //         Debug.Log("This is mobile platform");
        //         _blockWindow.SetActive(true);
        //         _isMobile = true;
        //     }
        //     else
        //     {
        //         _blockWindow.SetActive(false);
        //         _isMobile = false;
        //     }
        // #endif

        // #if !UNITY_WEBGL
        //     _blockWindow.SetActive(false);
        //     _isMobile = false;
        // #endif
    }

    public void OpenAppStore()
    {
        if (_isMobile)
            Application.OpenURL("https://www.apple.com/app-store/");
    }

    public void OpenPlayMarket()
    {
        if (_isMobile)
            Application.OpenURL("https://play.google.com/apps/testing/com.DefaultCompany.Cryptoboss");
    }
}
