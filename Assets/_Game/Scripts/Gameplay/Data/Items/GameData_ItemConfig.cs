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

    // Jam
    public bool isTnalak = false;
    public bool isInaul = false;
    public bool isKawayan = false;
    public bool isKulintang = false;
    public bool isAnnatto = false;
}
