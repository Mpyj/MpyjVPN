using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MpyjVPN.Application.ViewModels;

public class ScannedIPViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private string _rank = "";
    public string Rank
    {
        get => _rank;
        set { _rank = value; OnPropertyChanged(); }
    }

    private string _address = "";
    public string Address
    {
        get => _address;
        set { _address = value; OnPropertyChanged(); }
    }

    private string _pingText = "";
    public string PingText
    {
        get => _pingText;
        set { _pingText = value; OnPropertyChanged(); }
    }

    private int _pingValue = 0;
    public int PingValue
    {
        get => _pingValue;
        set { _pingValue = value; OnPropertyChanged(); }
    }

    private bool _isWorking = true;
    public bool IsWorking
    {
        get => _isWorking;
        set { _isWorking = value; OnPropertyChanged(); }
    }

    public string StatusIcon => IsWorking ? "âœ…" : "âŒ";
}