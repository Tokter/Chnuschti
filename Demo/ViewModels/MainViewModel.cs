using Chnuschti;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.ViewModels;

public class MainViewModel : ViewModelBase
{
    //Title property
    private string _title = "Chnuschti Demo Application";
    private bool _isButtonEnabled = true;
    private bool _isCheckboxEnabled = true;
    private ObservableCollection<Person> _people = new()
    {
        new Person("Alice", 30),
        new Person("Bob", 25),
        new Person("Charlie", 35)
    };

    public ICommand ChangeTitle { get; set; }

    public MainViewModel()
    {
        ChangeTitle = new DelegateCommand((p) =>
        {
            Title = GenerateRandomTitle();
        });  
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

    private double _slider1Value = 25.0;
    public int ScrollBar1Value
    {
        get => (int)_slider1Value;
        set => SetProperty(ref _slider1Value, value);
    }

    private double _slider2Value = 25.0;
    public int ScrollBar2Value
    {
        get => (int)_slider2Value;
        set => SetProperty(ref _slider2Value, value);
    }

    public bool IsButtonEnabled
    {
        get => _isButtonEnabled;
        set => SetProperty(ref _isButtonEnabled, value);
    }

    public bool IsCheckboxEnabled
    {
        get => _isCheckboxEnabled;
        set => SetProperty(ref _isCheckboxEnabled, value);
    }

    public ObservableCollection<Person> People
    {
        get => _people;
        set => SetProperty(ref _people, value);
    }
}


public class Person : ViewModelBase
{
    private string _name;
    private int _age;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }

    public Person(string name, int age)
    {
        _name = name;
        _age = age;
    }
}