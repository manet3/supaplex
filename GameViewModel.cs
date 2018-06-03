using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Supaplex
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private GameModel _model;
        private double _scale;
        private string _level;
        private double _winHeight = 300;
        private double _winWidth = 400;
        public const int Width = 10;
        public const int Heihgt = 10;
        public const int CellSize = 30;
        private double _playGridWidth = CellSize * Width + 2 * SystemParameters.BorderWidth;
        private double _playGridHeight = CellSize * Heihgt + SystemParameters.CaptionHeight + SystemParameters.BorderWidth;
        private string _scores;
        public double WinWidth
        {
            get { return _winWidth; }
            set
            {
                _winWidth = value;
                var scale = Math.Min(_winHeight/_playGridHeight, _winWidth/_playGridWidth);
                //_playGridHeight *= scale;
                //_playGridWidth *= scale;
                Scale = scale;
            }
        }

        public double WinHeight
        {
            get { return _winHeight; }
            set
            {
                _winHeight = value;
                var scale = Math.Min(_winHeight / _playGridHeight, _winWidth / _playGridWidth);
                //_playGridHeight *= scale;
                //_playGridWidth *= scale;
                Scale = scale;

            }
        }
        public string BackgroundPath
        {
            get { return MainMenuParameters.Mode == GameMode.AiMode ? "Images/background3.jpg" : "Images/background2.jpg"; }
        }
        public double Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                NotifyPropertyChanged("Scale");
            }
        }

        public string Scores
        {
            get
            {
                return _scores;
            }
            set
            {
                _scores = value;
                NotifyPropertyChanged("Scores");
            }
        }

        public Visibility AiNotificationVis
        {
            get { return AiMode?Visibility.Visible : Visibility.Collapsed; }
        }

        public ICommand KeyDownCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand KeyUpCommand { get; set; }
        public ICommand NextLevelStartCommand { get; set; }
        public ICommand RestartCommand { get; set; }
        public ICommand ContinueCommand { get; set; }

        public List<string> AvilableLvl { get; set; }

        public GameMenu Menu
        {
            get { return _menu; }
            set
            {
                _menu = value; 
                NotifyPropertyChanged("Menu");
            }
        }

        public string Level
        {
            get { return _level; } //current level
            set 
            {
                if (value != null)
                {
                    _level = value;
                    NewStart(null);
                    NotifyPropertyChanged("Level");
                }
            }
        }

        public GameModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                _model.GameOver += GameFinishing;
                _model.ScoresChanged += ScoresWrite;
                ScoresWrite();
                NotifyPropertyChanged("Model");
            }
        }



        private AiModel _aiModel;
        private int _pathCounter;
        private bool _isFirstStep;
        private bool _isToBeStoped;
        private Vector _nextDirectVector;

        public bool AiMode;

        public DispatcherTimer StepTimer;
        private GameMenu _menu;
        private Visibility _aiNotificationVis;

        public GameViewModel()
        {
            KeyDownCommand = new Command(KeyHendler);
            ContinueCommand = new Command(Continue);
            AvilableLvl = MainMenuParameters.Levels;
            Level = MainMenuParameters.Levels[MainMenuParameters.LevelNumb];
            ExitCommand = new Command(StrWin);
            NextLevelStartCommand = new Command(NextLevelStart);//define level, create a model
            RestartCommand = new Command(NewStart);
            Menu = GameMenu.None;
        }

        private void NewStart(object obj)//every new level start
        {
            if (Menu != null)
            {
                Menu.RestartEnabled = false;//against multiple clicking
                NotifyPropertyChanged("Menu");
            }

            ModelInitilise();

            TimerInitilise();

            Menu = GameMenu.None;

            _nextDirectVector = Vector.Null;
            AiMode = false;
            AiModeSet();
            AiMode = MainMenuParameters.Mode == GameMode.AiMode;

            Menu.RestartEnabled = true;
            NotifyPropertyChanged("Menu");

            if (MainMenuParameters.LevelNumb == AvilableLvl.Count - 1)
                Menu.NextLevelBtVisibility = Visibility.Collapsed;
        }

        private void ModelInitilise()
        {
            var model = new GameModel(Level);
            model.Deserialize();
            model.Init(0);
            Model = model;
        }

        private void TimerInitilise()
        {
            StepTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 0, 0, 200)};
            StepTimer.Tick += Replace;
            _isFirstStep = true;
            _isToBeStoped = false;
            StepTimer.Start();
        }

        private void AiModeSet()
        {
            if (!AiMode)//deleting AI module
            {
                StepTimer.Tick -= PathMoving;
                _pathCounter = 0;
                _aiModel = null;
                return;
            }
            StepTimer.Tick -= Replace;
            StepTimer.Tick += PathMoving;//first - direction event handling, than - replasing
            StepTimer.Tick += Replace;
            _aiModel = new AiModel(Model.DeepCopy());
            _aiModel.ConvertPath();
            MenuShow(GameMenu.None);
        }

        private void StrWin(object eventArgs)
        {
            var current = Application.Current.MainWindow;
            var window = new StartWindow(false);
            window.Show();
            window.Activate();
            current.Close();
            Application.Current.MainWindow = window;
        }

        private void LvlGet()
        {
            var files = Directory.GetFiles("Levels/", "*.lvl");
            foreach (string name in files.Select(Path.GetFileName))
            {
                string[] lvlname = name.Split('.');
                AvilableLvl.Add(lvlname[0]);
            }
            if (AvilableLvl.Count == 0) throw new ApplicationException("No levels found! Try to add some levels and load the game again.");
        }

        private void KeyHendler(object eventArgs)
        {
            if (eventArgs is Vector)
                MovementOperator((Vector)eventArgs);
            else
            {
                switch ((Key) eventArgs)
                {
                    case Key.Escape:
                        MenuShow(Menu.Enabled ? GameMenu.None : GameMenu.PauseMenu);
                        break;
                    case Key.Space:
                        if (_aiModel != null)
                            if (StepTimer.IsEnabled)
                                StepTimer.Stop();
                            else StepTimer.Start();
                        else
                            AiModeSet();
                        break;
                }
            }
        }
        private void PathMoving(object sender, EventArgs elapsedEventArgs)
        {
            if (_aiModel == null)
                return;
            if (_pathCounter <= _aiModel.SelectedPath.Count - 1)
            {
                _nextDirectVector = _aiModel.SelectedPath[_pathCounter];
                _pathCounter++;
            }
        }
        private void Replace(object sender, EventArgs e)//to get rid of timer arguments in a Model
        {
            // fuck the incapsulation
            Model.Replace();
            Model.DirectVector = _nextDirectVector;
            if (_isToBeStoped)
            {
                _nextDirectVector = Vector.Null;
                _isToBeStoped = false;
            } // replace even if the key is released during the first step
            _isFirstStep = false;
            Model.CheckMoving();
        }
        public void MovementOperator(Vector direction)//movimg on key commands
        {
            if (Model.DirectVector.Equals(Vector.Null) && !direction.Equals(Vector.Null))
                _isFirstStep = true; //key is pressed when Murphy is not moving => the first step
            if (_isFirstStep && (direction).Equals(Vector.Null))// first step, key is released before the timer event
                _isToBeStoped = true;
            else
                _nextDirectVector = direction;
        }

        private void GameFinishing(bool IsWon)
        {
            MenuShow(IsWon?GameMenu.WinMenu:GameMenu.LoseMenu);
            if (AiMode)
                Menu.Notification = "He did his best!";
        }

        private void MenuShow(GameMenu menuType)
        {
            if (Menu.Enabled && Model.Result != GameResult.None) return;
            Menu = menuType;
            if (MainMenuParameters.LevelNumb == AvilableLvl.Count - 1)
                Menu.NextLevelBtVisibility = Visibility.Collapsed;
            if (Menu.Enabled)
                StepTimer.Stop();
            else
                StepTimer.Start();
        }

        private void Continue(object eventArgs)
        {
            MenuShow(GameMenu.None);
        }

        private void ScoresWrite()
        {
            Scores = Model.ScoresGained + "/" + Model.ScoresNeed;
        }

        public void NextLevelStart(object obj)
        {
            var lvlNumb = AvilableLvl.IndexOf(Level) + 1;
            Level = AvilableLvl [lvlNumb];
        }

        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
