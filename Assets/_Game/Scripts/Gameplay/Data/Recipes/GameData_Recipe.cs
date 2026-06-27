using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData_Recipe", menuName = "Scriptable Objects/GameData_Recipe")]
public class GameData_Recipe : ScriptableObject
{
    [Header("Recipe Identification")]
    [SerializeField] private string _recipeID;
    public string RecipeID => _recipeID;

    [Header("Input Requirements")]
    public List<ERecipeInputType> inputs;
    public bool allowDifferentOrder = true;

    [Header("Output")]
    public GameObject outputPrefab;

    [Header("Crafting Properties")]
    public float craftingTime = 2f;
    public Sprite recipeIcon;

    [Header("Feedback")]
    public Color craftingColor = Color.white;
    public AudioClip craftingCompleteSound;

    private void OnValidate()
    {
        // Auto-generate ID if empty
        if (string.IsNullOrEmpty(_recipeID) && inputs != null)
        {
            GenerateRecipeID();
        }
    }

    [ContextMenu("Generate Recipe ID")]
    private void GenerateRecipeID()
    {
        var sorted = new List<ERecipeInputType>(inputs);
        sorted.Sort((a, b) => a.ToString().CompareTo(b.ToString()));
        _recipeID = string.Join("_", sorted);
    }

    // Check if this recipe matches given ingredients
    public bool MatchesIngredients(ERecipeInputType[] ingredientsToCheck)
    {
        if (inputs.Count != ingredientsToCheck.Length)
            return false;

        if (allowDifferentOrder)
        {
            var sortedInputs = inputs.OrderBy(i => i).ToList();
            var sortedCheck = ingredientsToCheck.OrderBy(i => i).ToList();

            for (int i = 0; i < sortedInputs.Count; i++)
            {
                if (sortedInputs[i] != sortedCheck[i])
                    return false;
            }
        }
        else
        {
            // Check in exact order
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i] != ingredientsToCheck[i])
                    return false;
            }
        }

        return true;
    }
}
