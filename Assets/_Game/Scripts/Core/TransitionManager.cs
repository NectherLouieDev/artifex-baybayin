using PixeLadder.EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string _sceneToLoad = "";
    [SerializeField] private int _sceneIndex = 0;
    


    private void OnEnable()
    {
        SceneTransitioner.Instance.OnSceneLoaded.AddListener(SceneTransitioner_OnSceneLoaded);
    }

    private void SceneTransitioner_OnSceneLoaded()
    {
        Debug.Log("SceneTransitioner_OnSceneLoaded()");
    }

    private void OnDisable()
    {
        SceneTransitioner.Instance.OnSceneLoaded.RemoveListener(SceneTransitioner_OnSceneLoaded);
    }

    public void StartTransition()
    {
        //TransitionEffect effectToUse = transitionEffects[_sceneIndex];

        //SceneTransitioner.Instance.LoadScene(_sceneToLoad, effectToUse);
    }
}
