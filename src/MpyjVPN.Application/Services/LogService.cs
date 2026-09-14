using System;
using System.Collections.ObjectModel;

namespace MpyjVPN.Application.Services;

public static class LogService
{
    private static readonly ObservableCollection<string> _logs = new();
    
    public static ObservableCollection<string> Logs => _logs;
    
    public static event Action? LogAdded;
    
    public static void Add(string message)
    {
        var time = DateTime.Now.ToString("HH:mm:ss");
        _logs.Insert(0, $"[{time}] {message}");
        
        while (_logs.Count > 200)
            _logs.RemoveAt(_logs.Count - 1);
        
        LogAdded?.Invoke();
    }
    
    public static void Clear()
    {
        _logs.Clear();
        Add("🗑️ Log cleared");
    }
}
