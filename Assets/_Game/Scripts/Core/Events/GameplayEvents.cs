
using System.Collections.Generic;
using UnityEngine;

public static class GameplayEvents
{
    public struct InteractableChanged
    {
        public IInteractable SelectedObject;
    }

    public struct BaseHealthChanged
    {
        public BaseStateHandler Base;
        public int CurrentHealth;
        public int MaxHealth;
    }

    public struct BaseDeath
    {
        public BaseStateHandler Base;
    }

    public struct PreWaveTimerUpdated
    {
        public float CurrentTime;
        public float TargetTime;
    }

    public struct WaveStatePreparing
    {
        public int CurrentWaveNumber;
        public int MaxWaveNumber;
    }

    public struct CreepDeath
    {
        public CreepStateHandler StateHandler;
        public int CreepsLeft;
    }

    public struct CoinCollected
    {
        public Coin CoinObject;
        public PlayerIdentity Identity;
    }

    public struct CoinCountUpdated
    {
        public int CoinCount;
    }

    public struct AllWavesCompleted
    {
        public int WaveCount;
    }

    public struct LevelCleared
    {
        public int WaveCount;
    }

    public struct PlayerPickedUpItem
    {
        public PlayerIdentity Identity;
        public ICarryable ItemCarried;
    }

    public struct PlayerDroppedItem
    {
        public PlayerIdentity Identity;
        public ICarryable ItemCarried;
    }

    public struct MixerSpawnedItem
    {
        public ICarryable ItemPrepped;
    }

    public struct ChopperSpawnedItem
    {
        public ICarryable ItemChopped;
    }

    public struct FryerSpawnedItem
    {
        public ICarryable ItemCooked;
    }

    public struct OvenSpawnedItem
    {
        public ICarryable ItemBaked;
    }

    public struct AssemblySpawnedItem
    {
        public ICarryable ItemAssembled;
        public int Amount;
    }

    public struct Tutorial00_GreenFlagTriggered
    {
        public PlayerIdentity Identity;
    }

    public struct Tutorial_RedFlagTriggered
    {
        public PlayerIdentity Identity;
    }

    public struct QuestDataUpdated
    {
        public QuestData Data;
    }

    public struct CountdownTimerUpdated
    {
        public float CurrentTime;
    }

    public struct CountdownTimerCompleted
    {
        public float CurrentTime;
    }

    public struct BlockDestroyed<T> 
    { 
        public T Sender; 
    }

    public struct BlockRepaired<T>
    {
        public T Sender;
    }
}
