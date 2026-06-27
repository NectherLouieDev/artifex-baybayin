using PixeLadder.EasyTransition;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ESceneID
{
    MainMenu,
    MapSelection,
    SettingsMenu,
    QuitMenu,
    Chapter_0_Level_1,
    Chapter_0_Level_2,
    Chapter_0_Level_3,
    Chapter_0_Level_4,
    Chapter_0_Level_5,
    Chapter_0_Level_6,
    Chapter_0_Level_7,
    Chapter_0_Level_8,
    Chapter_0_Level_9,
    Chapter_0_Level_Boss,
    DEMO_LEVEL
}

[Serializable]
public class SceneData
{
    public int index;
    public string sceneName;
}

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Mappings")]
    [SerializeField] private ESceneID[] sceneIDs;
    [SerializeField] private SceneData[] sceneData;

    [Header("Transition Config")]
    [SerializeField] private int _transitionIndex = 0;
    [SerializeField] private TransitionEffect[] transitionEffects;

    private Dictionary<ESceneID, SceneData> sceneMap = new Dictionary<ESceneID, SceneData>();

    private void Start()
    {
        for (int i = 0; i < sceneIDs.Length; i++)
        {
            sceneMap.Add(sceneIDs[i], sceneData[i]);
        }
    }

    public void ChangeScene(ESceneID sceneID)
    {
        SceneData _sceneData = sceneMap.GetValueOrDefault(sceneID);

        int sceneIndex = _sceneData.index;

        if (sceneIndex != -1)
        {
            //SceneManager.LoadScene(sceneIndex);
            TransitionEffect effectToUse = transitionEffects[_transitionIndex];
            SceneTransitioner.Instance.LoadScene(_sceneData.sceneName, effectToUse);
        }
    }
}
