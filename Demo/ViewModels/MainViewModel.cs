using Chnuschti;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.ViewModels;

public class MainViewModel : ViewModelBase
{
    //Title property
    private string _title = "Chnuschti Demo Application";

    public ICommand ChangeTitle { get; set; }

    public MainViewModel()
    {
        ChangeTitle = new DelegateCommand((p) => Title = GenerateRandomTitle());
    }

    private string GenerateRandomTitle()
    {
        var random = new Random();

        string[] titles = { "Demo Title 1", "Demo Title 2", "Demo Title 3" };
        return titles[random.Next(titles.Length)];
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}
