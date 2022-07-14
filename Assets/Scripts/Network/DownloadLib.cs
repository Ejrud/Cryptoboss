#define LOAD_DLL_MANUALLY
 
using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
 
public class DownloadLib : MonoBehaviour {
 
    private IntPtr handleB;
 
#if LOAD_DLL_MANUALLY
    void Awake() {
        //Load
        handleB = LoadLib("/var/game/Server_Data/Mono/Kernel32.dll");
    }
 
    private IntPtr LoadLib( string path ) {
        IntPtr ptr = LoadLibrary( path );
        if (ptr == IntPtr.Zero)
        {
            int errorCode = Marshal.GetLastWin32Error();
            Debug.LogError(string.Format("Failed to load library {1} (ErrorCode: {0})",errorCode, path));
        }else {
            Debug.Log ("loaded lib "+path);
        }
        return ptr;
    }
 
    void OnDestroy() {
        //Free
        if(handleB != IntPtr.Zero)
            FreeLibrary(handleB);
    }
#endif
 
    // Use this for initialization
    void Start () {
        Debug.Log( myExternDllFunction() );
    }
 
#if LOAD_DLL_MANUALLY
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string libname);
   
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern bool FreeLibrary(IntPtr hModule);
#endif
 
   
    [DllImport("A")]
    private static extern float myExternDllFunction();
}
