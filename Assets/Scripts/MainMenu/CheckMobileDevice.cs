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
        if (Application.isMobilePlatform)
        {
            Debug.Log("This is mobile platform");
            _blockWindow.SetActive(true);
            _isMobile = true;
        }
        else
        {
            Debug.Log("Thisi isn't mobile platform");
            _blockWindow.SetActive(false);
            _isMobile = false;
        }
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
