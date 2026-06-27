using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class TaskItem
{
    [TextArea(1, 3)]
    public string title = "New Task";

    [TextArea(3, 6)]
    public string description = "Task description here...";

    public bool isCompleted = false;
    public string createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
}

public class TaskLister : MonoBehaviour
{
    [Header("Task Board")]
    public List<TaskItem> tasks = new List<TaskItem>();
    public bool autoSave = true;

    void Start()
    {
        if (autoSave) LoadTasks();
    }

    void OnDestroy()
    {
        if (autoSave) SaveTasks();
    }

    public void AddTask(string title, string description)
    {
        TaskItem newTask = new TaskItem();
        newTask.title = title;
        newTask.description = description;
        tasks.Add(newTask);
        if (autoSave) SaveTasks();
    }

    public void RemoveTask(int index)
    {
        tasks.RemoveAt(index);
        if (autoSave) SaveTasks();
    }

    public void ClearCompletedTasks()
    {
        tasks.RemoveAll(t => t.isCompleted);
        if (autoSave) SaveTasks();
    }

    public void SaveTasks()
    {
        string json = JsonUtility.ToJson(new Wrapper { tasks = this.tasks });
        PlayerPrefs.SetString($"TaskLister_{gameObject.name}_{GetInstanceID()}", json);
        PlayerPrefs.Save();
    }

    public void LoadTasks()
    {
        string json = PlayerPrefs.GetString($"TaskLister_{gameObject.name}_{GetInstanceID()}");
        if (!string.IsNullOrEmpty(json))
        {
            Wrapper w = JsonUtility.FromJson<Wrapper>(json);
            if (w != null) tasks = w.tasks;
        }
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<TaskItem> tasks;
    }
}