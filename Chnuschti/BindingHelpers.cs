using System.ComponentModel;
using System.Linq.Expressions;

namespace Chnuschti;

public static class BindingHelpers
{
    public static IReadOnlyBinding OneWayToDC<TS, TP>(
        this DataElement fe,
        Expression<Func<TS, TP>> selector)
        where TS : class, INotifyPropertyChanged
        => new DataContextBinding<TS, TP>(fe, selector, BindingMode.OneWay);

    public static ITwoWayBinding TwoWayToDC<TS, TP>(
        this DataElement fe,
        Expression<Func<TS, TP>> selector)
        where TS : class, INotifyPropertyChanged
        => (ITwoWayBinding)new DataContextBinding<TS, TP>(fe, selector,  BindingMode.TwoWay);

    public static IReadOnlyBinding OneTimeToDC<TS, TP>(
        this DataElement fe,
        Expression<Func<TS, TP>> selector)
        where TS : class, INotifyPropertyChanged
        => new DataContextBinding<TS, TP>(fe, selector, BindingMode.OneTime);
}
