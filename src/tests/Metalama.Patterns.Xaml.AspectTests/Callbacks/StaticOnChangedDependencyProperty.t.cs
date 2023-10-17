using System.Windows;
namespace Metalama.Patterns.Xaml.AspectTests.Callbacks.StaticOnChangedDependencyProperty;
public partial class StaticOnChangedDependencyProperty : DependencyObject
{
  [DependencyProperty]
  public int Foo
  {
    get
    {
      return (int)GetValue(FooProperty);
    }
    set
    {
      this.SetValue(StaticOnChangedDependencyProperty.FooProperty, value);
    }
  }
  private static void OnFooChanged(DependencyProperty d)
  {
  }
  public static readonly DependencyProperty FooProperty;
  static StaticOnChangedDependencyProperty()
  {
    void PropertyChanged(DependencyObject d_1, DependencyPropertyChangedEventArgs e)
    {
      StaticOnChangedDependencyProperty.OnFooChanged(StaticOnChangedDependencyProperty.FooProperty);
    }
    StaticOnChangedDependencyProperty.FooProperty = DependencyProperty.Register("Foo", typeof(int), typeof(StaticOnChangedDependencyProperty), new PropertyMetadata(PropertyChanged));
  }
}