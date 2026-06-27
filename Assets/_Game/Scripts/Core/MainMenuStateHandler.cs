using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuStateHandler : MonoBehaviour
{
    [SerializeField] private SceneLoader _sceneLoader;

    public void OnInteract(InputValue inputValue)
    {
        _sceneLoader.ChangeScene(ESceneID.Chapter_0_Level_1);
    }

    public void OnDash(InputValue inputValue)
    {
        _sceneLoader.ChangeScene(ESceneID.Chapter_0_Level_1);
    }
}
