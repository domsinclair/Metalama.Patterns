using System.ComponentModel;
using Metalama.Patterns.NotifyPropertyChanged.AspectTests.Include;
using Metalama.Patterns.NotifyPropertyChanged.Metadata;
namespace Metalama.Patterns.NotifyPropertyChanged.AspectTests.InpcAutoPropertyWithInitializerNoRefs;
[NotifyPropertyChanged]
public class InpcAutoPropertyWithInitializerNoRefs : INotifyPropertyChanged
{
  private SimpleInpcByHand _x = new(42);
  public SimpleInpcByHand X
  {
    get
    {
      return this._x;
    }
    set
    {
      if (!object.ReferenceEquals(value, this._x))
      {
        var oldValue = this._x;
        this._x = value;
        this.OnUnmonitoredObservablePropertyChanged("X", (INotifyPropertyChanged? )oldValue, value);
        this.OnPropertyChanged("X");
      }
    }
  }
  [OnChildPropertyChangedMethod(new string[] { })]
  protected virtual void OnChildPropertyChanged(string parentPropertyPath, string propertyName)
  {
  }
  protected virtual void OnPropertyChanged(string propertyName)
  {
    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
  [OnUnmonitoredObservablePropertyChangedMethod(new string[] { "X" })]
  protected virtual void OnUnmonitoredObservablePropertyChanged(string propertyPath, INotifyPropertyChanged? oldValue, INotifyPropertyChanged? newValue)
  {
  }
  public event PropertyChangedEventHandler? PropertyChanged;
}