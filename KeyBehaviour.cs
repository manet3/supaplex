using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Supaplex
{
    public class KeyBehaviour
    {
        public static readonly DependencyProperty KeyCommandProperty =
        DependencyProperty.RegisterAttached("KeyCommand", typeof(ICommand),
        typeof(KeyBehaviour), new FrameworkPropertyMetadata(KeyCommand));

        private static void KeyCommand(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            element.KeyDown += KeyDownAction;
            element.KeyUp += KeyUpAction;
        }

        static void KeyUpAction(object sender, KeyEventArgs keyEventArgs)
        {
            var element = (FrameworkElement)sender;
            var command = ElementOnKey(element);
            object com;
            switch (keyEventArgs.Key)
            {
                case Key.Down:
                case Key.Up:
                case Key.Left:
                case Key.Right:
                    com = Vector.Null; break;
                default:
                    com = keyEventArgs.Key; break;
            }

            command.Execute(com);
        }

        static void KeyDownAction(object sender, KeyEventArgs keyEventArgs)
        { 
            if(keyEventArgs.IsRepeat) return;
                Vector dir;
                switch (keyEventArgs.Key)
                {
                    case Key.Down:
                        dir = Vector.Down;
                        break;
                    case Key.Up:
                        dir = Vector.Up;
                        break;
                    case Key.Left:
                        dir = Vector.Left;
                        break;
                    case Key.Right:
                        dir = Vector.Right;
                        break;
                    default:
                        return;
                }
                var element = (FrameworkElement) sender;
                var command = ElementOnKey(element);
                command.Execute(dir);
        }

        private static ICommand ElementOnKey(UIElement element)
        {
            return (ICommand)element.GetValue(KeyCommandProperty);
        }

        public static void SetKeyCommand(UIElement element, ICommand value)
        {
            element.SetValue(KeyCommandProperty, value);
        }

    }
}
