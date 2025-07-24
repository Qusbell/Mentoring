using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneUnloadChecker : SingletonT<SceneUnloadChecker>
{
    public bool isUnloadingScene { get; private set; } = false;


    // protected void Awake()
    // { DontDestroyOnLoad(gameObject); }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        isUnloadingScene = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isUnloadingScene = false;
    }

}
