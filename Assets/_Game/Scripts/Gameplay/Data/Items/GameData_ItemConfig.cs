using UnityEngine;

[CreateAssetMenu(fileName = "GameData_ItemConfig", menuName = "Scriptable Objects/GameData_ItemConfig")]
public class GameData_ItemConfig : ScriptableObject
{
    public ERecipeInputType recipeInputType;
    public bool isMixable = false;
    public bool isChoppable = false;
    public bool isFryable = false;
    public bool isBakable = false;
    public bool isPlatable = false;
}
