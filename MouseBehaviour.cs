using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Supaplex
{
    public class MouseBehaviour
    {
        public static readonly DependencyProperty MouseCommandProperty =
        DependencyProperty.RegisterAttached("MouseCommand", typeof(ICommand),
        typeof(MouseBehaviour), new FrameworkPropertyMetadata(
        new PropertyChangedCallback(MouseCommandChanged)));

        private static void MouseCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;
            element.MouseUp += element_MouseAction;
            element.MouseWheel += element_MouseAction;
            element.MouseMove += element_MouseAction;
        }

        static void element_MouseAction(object sender, MouseEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            ICommand command = ElementOnMouseEnter(element);
            if (e is MouseButtonEventArgs)
            {
                command = GetMouseUpCommand(element);
            }
            if (e is MouseWheelEventArgs)
            {
                command = GetMouseWheelCommand(element);
            }
            command.Execute(e);
        }

        private static ICommand ElementOnMouseEnter(UIElement element)
        {
            return (ICommand)element.GetValue(MouseCommandProperty);
        }

        public static ICommand GetMouseUpCommand(UIElement element)
        {
            return (ICommand)element.GetValue(MouseCommandProperty);
        }

        public static ICommand GetMouseWheelCommand(UIElement element)
        {
            return (ICommand)element.GetValue(MouseCommandProperty);
        }

        public static void SetMouseCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseCommandProperty, value);
        }
    }
}
