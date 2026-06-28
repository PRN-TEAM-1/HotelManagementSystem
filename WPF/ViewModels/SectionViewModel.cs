using System.Collections.ObjectModel;

namespace WPF.ViewModels;

public sealed class SectionViewModel : BaseViewModel
{
    public SectionViewModel(
        string title,
        string description,
        string accentLabel,
        IEnumerable<string> highlights,
        IEnumerable<string> handoffNotes,
        IEnumerable<string>? tags = null)
    {
        Title = title;
        Description = description;
        AccentLabel = accentLabel;
        Highlights = new ObservableCollection<string>(highlights ?? Array.Empty<string>());
        HandoffNotes = new ObservableCollection<string>(handoffNotes ?? Array.Empty<string>());
        Tags = new ObservableCollection<string>(tags ?? Array.Empty<string>());
    }

    public override string Title { get; }

    public override string Description { get; }

    public string AccentLabel { get; }

    public ObservableCollection<string> Highlights { get; }

    public ObservableCollection<string> HandoffNotes { get; }

    public ObservableCollection<string> Tags { get; }
}
