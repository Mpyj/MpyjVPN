using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MpyjVPN.Application.ViewModels;

public class ConnectionStackViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Protocol Ø§Ù†ØªØ®Ø§Ø¨ Ø´Ø¯Ù‡
    private ProtocolViewModel? _selectedProtocol;
    public ProtocolViewModel? SelectedProtocol
    {
        get => _selectedProtocol;
        set
        {
            _selectedProtocol = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PreviewText));
        }
    }

    // Config Ø§Ù†ØªØ®Ø§Ø¨ Ø´Ø¯Ù‡
    private ProtocolViewModel? _selectedConfig;
    public ProtocolViewModel? SelectedConfig
    {
        get => _selectedConfig;
        set
        {
            _selectedConfig = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PreviewText));
        }
    }

    // Preview
    public string PreviewText
    {
        get
        {
            var parts = new System.Collections.Generic.List<string>();

            if (SelectedConfig != null)
                parts.Add($"{SelectedConfig.Icon} {SelectedConfig.Name}");

            if (SelectedProtocol != null)
                parts.Add($"{SelectedProtocol.Icon} {SelectedProtocol.Name}");

            if (parts.Count == 0)
                return "â€”";

            return string.Join("  â†’  ", parts);
        }
    }

    public bool HasSelection => SelectedProtocol != null || SelectedConfig != null;
}