using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IRecipeInput
{

}

public class RecipeDatabase : MonoBehaviour
{
    [SerializeField] private List<GameData_Recipe> _allRecipes;

    // Dictionary for O(1) lookups is much faster than looping
    private Dictionary<string, GameData_Recipe> _recipeLookup;

    private void Awake()
    {
        BuildRecipeLookup();
    }

    private void BuildRecipeLookup()
    {
        _recipeLookup = new Dictionary<string, GameData_Recipe>();

        foreach (var recipe in _allRecipes)
        {
            // Create a unique key from sorted ingredients
            string key = GetRecipeKey(recipe.inputs);

            if (!_recipeLookup.ContainsKey(key))
            {
                _recipeLookup.Add(key, recipe);
            }
            else
            {
                Debug.LogError($"Duplicate recipe key detected: {key}. Recipes must have unique ingredient combinations.");
            }
        }
    }

    private string GetRecipeKey(List<ERecipeInputType> ingredients)
    {
        // Sort to ensure consistency regardless of order
        var sorted = ingredients.OrderBy(i => i.ToString()).ToList();
        return string.Join("_", sorted);
    }

    private string GetRecipeKey(ERecipeInputType[] ingredients)
    {
        var sorted = ingredients.OrderBy(i => i.ToString()).ToArray();
        return string.Join("_", sorted);
    }

    public GameData_Recipe GetRecipe(ERecipeInputType[] ingredients)
    {
        if (ingredients == null || ingredients.Length == 0)
            return null;

        string key = GetRecipeKey(ingredients);

        if (_recipeLookup.TryGetValue(key, out GameData_Recipe recipe))
        {
            return recipe;
        }

        Debug.LogWarning($"No recipe found for ingredients: {string.Join(", ", ingredients)}");
        return null;
    }

    // Overload for List inputs
    public GameData_Recipe GetRecipe(List<ERecipeInputType> ingredients)
    {
        return GetRecipe(ingredients.ToArray());
    }

    // Helpers
    public ERecipeInputType ConverToRecipeInputType(ICarryable carriedItem)
    {
        ERecipeInputType output = ERecipeInputType.None;

        if (carriedItem is ItemIngredient)
        {
            output = (carriedItem as ItemIngredient).RecipeInputType;
        }

        if (carriedItem is ItemPrepped)
        {
            output = (carriedItem as ItemPrepped).RecipeInputType;
        }

        if (carriedItem is ItemCooked)
        {
            output = (carriedItem as ItemCooked).RecipeInputType;
        }

        return output;
    }
}