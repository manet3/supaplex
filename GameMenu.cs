using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Supaplex
{
    public class GameMenu:INotifyPropertyChanged
    {
        private Visibility _mVisibility;
        private string _menuNotification;
        private bool _mEnabled;
        private bool _nextLevelBtEnabled;
        private bool _restartBtEnabled;
        private Visibility _nextLevelBtVisibility;
        private Visibility _continueBtVisibility;
        public string Notification
        {
            get { return _menuNotification; }
            set
            {
                _menuNotification = value;
                NotifyPropertyChanged("MenuNotification");
            }
        }
        public Visibility Visibility
        {
            get { return _mVisibility; }
            set
            {
                _mVisibility = value;
                NotifyPropertyChanged("MenuVisibility");
            }
        }
        public bool Enabled
        {
            get { return _mEnabled; }
            set
            {
                Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                _mEnabled = value;
                NotifyPropertyChanged("MenuEnabled");
            }
        }
        public Visibility ContinueBtVisibility
        {
            get { return _continueBtVisibility; }
            set
            {
                _continueBtVisibility = value;
                NotifyPropertyChanged("ContinueBtVisibility");
            }
        }
        public Visibility NextLevelBtVisibility
        {
            get { return _nextLevelBtVisibility; }
            set
            {
                _nextLevelBtVisibility = value;
                NotifyPropertyChanged("NextLevelBtVisibility");
            }
        }
        public bool NextLevelBtEnabled
        {
            get { return _nextLevelBtEnabled; }
            set
            {
                _nextLevelBtEnabled = value;
                NotifyPropertyChanged("NextLevelBtEnabled");
            }
        }

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public bool RestartEnabled 
        {
            get { return _restartBtEnabled; }
            set
            {
                _restartBtEnabled = value;
                NotifyPropertyChanged("RestartEnabled");
            }
        }

        public static GameMenu PauseMenu
        {
            get
            {
                return new GameMenu
                {
                    Notification = "Game paused",
                    ContinueBtVisibility = Visibility.Visible,
                    NextLevelBtEnabled = false,
                    NextLevelBtVisibility = Visibility.Collapsed,
                    Enabled = true,
                };
            }
        }
        public static GameMenu WinMenu
        {
            get
            {
                return new GameMenu
                {
                    Notification = "Congratulations!",
                    ContinueBtVisibility = Visibility.Collapsed,
                    NextLevelBtEnabled = true,
                    NextLevelBtVisibility = Visibility.Visible,
                    Enabled = true,
                };
            }
        }
        public static GameMenu LoseMenu
        {
            get
            {
                return new GameMenu
                {
                    Notification = "Wasted((",
                    ContinueBtVisibility = Visibility.Collapsed,
                    NextLevelBtEnabled = false,
                    NextLevelBtVisibility = Visibility.Collapsed,
                    Enabled = true,
                };
            }
        }


        public static GameMenu None
        {
            get
            {
                return new GameMenu
                {
                    Enabled = false,
                };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
