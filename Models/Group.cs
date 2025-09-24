using Naviguard.Models;
using System.Collections.ObjectModel;

public class Group
{
    public long group_id { get; set; }
    public string group_name { get; set; } = string.Empty;
    public string? description { get; set; }
    public short pin { get; set; }

    public ObservableCollection<Pagina> Paginas { get; set; } = new ObservableCollection<Pagina>();
}