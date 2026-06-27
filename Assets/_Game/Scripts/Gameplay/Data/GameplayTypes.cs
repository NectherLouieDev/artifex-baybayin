public enum EWaveState
{
    Idle,
    Preparing,
    Spawning,
    Active,
    Completed,
    Failed
}

public enum ERecipeInputType
{
    None,

    // Raw
    Flour,
    Chemical,
    Spice,
    Metal,
    Crystal,

    // Prepped
    BombBatter,
    MetalShavings,

    // Cooked
    FriedBomb,
    RepairPaste,
    BakedBomb,

    // Assembled
    BlastBomb,
    RepairBomb
}

public enum ERecipeOutputType
{
    None,

    // Prepped
    BombBatter,
    MetalShavings,
    RepairPaste,
    
    // Cooked
    FriedBomb,

    // Assembled
    BlastBomb
}

public enum EAssembledType
{
    BlastBomb
}

public enum EBombType
{
    /// <summary>
    /// Cooked Bombs
    /// </summary>
    FriedBomb,
    BakedBomb,

    /// <summary>
    /// Assembled Bombs
    /// </summary>
    BlastBomb,
    RepairBomb,
    StunBomb,
    IceBomb,
    FlameBomb,
    MagnetBomb
}

public enum ECreepType
{
    Basic,
    Elite,
    Boss
}

public enum ELootType
{
    Coin,
    Gem
}

public enum ELevelIDs
{
    None,
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
}
