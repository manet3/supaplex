using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Supaplex
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        public ExceptionWindow(Exception Params)
        {
            InitializeComponent();
            MainWindow.Title = Params.Title;
            Message.Text = Params.Text;
            Image.Source = new BitmapImage(new Uri("pack://application:,,,/Images/exception_icon.png"));
            if (Params.Type == ExceptionType.ForgotMurphy)
                Image.Source = new BitmapImage(new Uri("pack://application:,,,/Images/exception_head.png"));
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public enum ExceptionType
    {
        None,
        IncorrectInput,
        ForgotMurphy,
        LevelNotFound,
        ForgotToSave
    }
    public class Exception
    {
        public string Text;
        public string Title;
        public ExceptionType Type;

        public Exception(): this(ExceptionType.None)
        {
        }

        public Exception(ExceptionType type)
        {
            Type = type;
            switch (type)
            {
                case ExceptionType.None: Text = " Something went wrong...";
                    Title = "Ups)";break;
                case ExceptionType.IncorrectInput: Text = " Is it really hard to understand what and were to input? Let me know and help to improve the product!";
                    Title = "Incorrect input"; break;
                case ExceptionType.ForgotMurphy: Text = " You have forgotten to place Murphy! Please, do it if you respect me.";
                    Title = "Player is required"; break;
                case ExceptionType.LevelNotFound: Text = "Level with such name does not exist!";
                    Title = "Level not found";
                    break;
            }
        }

        public void ShowException()
        {
            var window = new ExceptionWindow(this);
            window.ShowDialog();
        }
    }
}
