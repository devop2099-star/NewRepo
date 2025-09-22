using CommunityToolkit.Mvvm.ComponentModel;
using Naviguard.Models;

public class PageListItemViewModel : ObservableObject
{
    private bool _isSelected;
    public Pagina PageData { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string PageName => PageData.page_name;

    public PageListItemViewModel(Pagina pagina)
    {
        PageData = pagina;
    }
}