using System.Collections.Generic;
using System;
using UnityEngine;

public class GameplayEventBus : MonoBehaviour
{
    public static GameplayEventBus Instance { get; private set; }

    private readonly Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>();

    private void Awake()
    {
        Instance = this;
    }

    public void Subscribe<T>(Action<T> handler)
    {
        Type eventType = typeof(T);
        if (_events.ContainsKey(eventType))
        {
            _events[eventType] = Delegate.Combine(_events[eventType], handler);
        }
        else
        {
            _events[eventType] = handler;
        }
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        Type eventType = typeof(T);
        if (_events.ContainsKey(eventType))
        {
            _events[eventType] = Delegate.Remove(_events[eventType], handler);
        }
    }

    public void Invoke<T>(T eventArgs)
    {
        Type eventType = typeof(T);
        if (_events.ContainsKey(eventType) && _events[eventType] != null)
        {
            (_events[eventType] as Action<T>)?.Invoke(eventArgs);
        }
    }
}
