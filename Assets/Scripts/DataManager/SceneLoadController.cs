using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadController : MonoBehaviour
{
    [SerializeField] private UserData userData;

    public void LoadGame(int sceneIndex)
    {
        if (userData.IsAuthorized())
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.Log("Пользователь не авторизирован");
        }
    }
}
