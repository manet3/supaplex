using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Shapes;

namespace Supaplex
{

    public class ConstructViewModel: INotifyPropertyChanged
    {
        private EditorModel _model;
        private bool _isMurphySet;
        private GridLength _fieldWidth;
        private GridLength _fieldHeight;
        private string _level;
        private Visibility _murfButtonVisibility;
        private string _scoresRequired;
        private int _buttonSelectedNumb;
        private GridLength _upField;
        private GridLength _leftField;
        private string _levelIndex;
        public int WinH { get; set; }

        public int WinW { get; set; }

        public int WinMaxH { get; set; }

        public int WinMaxW { get; set; }

        public string LevelNameInput
        {
            set
            {
                if (!AvilableLvl.Contains(value) && value != "")
                    Level = value;
            }
        }

        public string LevelIndex
        {
            get { return _levelIndex; }
            set
            {
                if(!CheckCphInput(value))return;
                _levelIndex = value;
                NotifyPropertyChanged("LevelIndex");
            }
        }


        public string Level
        {
            get
            {
                return _level;
            }
            set
            {
                if (value != null)
                {
                    if (value != "")
                    {
                        _level = value;
                    }
                    else
                    {
                        _level = "Level" + " " + LvlNumb();
                    }
                    LevelIndex = (AvilableLvl.IndexOf(_level) + 1).ToString();
                    Model.Level = _level;
                    NotifyPropertyChanged("AvilableLvl");
                }
                NotifyPropertyChanged("Level");
            }
        }

        public List<string> AvilableLvl { get; set; }
        public string NewCells
        {
            get { return (_fieldWidth.Value/30).ToString(); }
            set 
            {
                if (!CheckCphInput(value)) return;
                if (int.Parse(value) == 0)
                {
                    (new Exception(ExceptionType.IncorrectInput)).ShowException();
                    return;
                }
                _fieldWidth = new GridLength(int.Parse(value) * 30);
                ResizeField( int.Parse(value), Model.YSize);
                NotifyPropertyChanged("FieldWidth");
                NotifyPropertyChanged("NewCells");
            }
        }

        public string NewRows
        {
            get { return (_fieldHeight.Value/30).ToString(); }
            set
            {
                if (!CheckCphInput(value)) return;
                if (int.Parse(value) == 0)
                {
                    (new Exception(ExceptionType.IncorrectInput)).ShowException();
                    return;
                }
                _fieldHeight = new GridLength(int.Parse(value) * 30);
                ResizeField(Model.XSize, int.Parse(value));
                NotifyPropertyChanged("FieldHeight");
                NotifyPropertyChanged("NewRows");
            }
        }

        private void ResizeField(int offsetX, int offsetY)
        {
            var fillCellType = (CellTemplate.TypeOfCell == CellType.Murphy) ? CellType.None : CellTemplate.TypeOfCell;
            bool isRemoved;
            Model.Fill(fillCellType, offsetX, offsetY,out isRemoved);
            if (isRemoved)
            {
                _isMurphySet = false;
                ChangeTemplate(null); //adding Set Murphy button if he was wiped
            }
        }

        public GridLength FieldWidth
        {
            get
            {
                return _fieldWidth;
            }
            set
            {
                NewCells = (value.Value / 30).ToString();
            }
        }

        public GridLength FieldHeight
        {
            get
            {
                return _fieldHeight; 
            }
            set
            {
                NewRows = (value.Value / 30).ToString();
            }
        }

        public GridLength UpField
        {
            get { return _upField; }
            set
            {
                var offset = value.Value - _upField.Value;
                if (Math.Abs(offset).Equals(30))
                {
                    Model.AddCellsDir = false;
                    FieldHeight = new GridLength(FieldHeight.Value - offset);
                }
                _upField = value;
                NotifyPropertyChanged("UpField");
            }
        }

        public GridLength LeftField
        {
            get { return _leftField; }
            set
            {
                var offset = value.Value - _leftField.Value;
                if (Math.Abs(offset).Equals(30))
                {
                    Model.AddCellsDir = false;
                    FieldWidth = new GridLength( FieldWidth.Value - offset);
                }
                _leftField = value;
                NotifyPropertyChanged("LeftField");
            }
        }

        public EditorModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                NotifyPropertyChanged("Model");

            }
        }

        public MyCell CellTemplate { get; set; }


        public int ButtonSelectedNumb
        {
            get { return _buttonSelectedNumb; }
            set
            {
                _buttonSelectedNumb = value;
                NotifyPropertyChanged("ButtonSelectedNumb");
            }
        }

        public ICommand WinSizedCommand{ get; set; }
        public ICommand DragCommand { get; set; }
        public ICommand ResizeWinChgCommand { get; set; }
        public ICommand ChangeTypeCommand { get; set;}
        public ICommand MouseCommand { get; set; }
        public ICommand SerializeCommand { get; set; }
        public ICommand DeserializeCommand { get; set; }
        public ICommand PlayCommand { get; set; }
        public string ScoresRequired
        {
            get { return _scoresRequired; }
            set
            {
                if (!CheckCphInput(value)) return;
                if (Model == null) return;
                Model.scoresNeed = Math.Min(int.Parse(value), Model.CookiesSet);
                _scoresRequired = Model.scoresNeed.ToString();
                NotifyPropertyChanged("ScoresRequired");

            }
        }

        public Visibility MurfButtonVisibility
        {
            get { return _murfButtonVisibility; }
            set
            {
                _murfButtonVisibility = value;
                NotifyPropertyChanged("MurfButtonVisibility");
            }
        }

        public ConstructViewModel()
        {
            _upField = new GridLength(30);
            _leftField = new GridLength(30);
            DragCommand = new Command(DragField);
            ChangeTypeCommand = new Command(ChangeTemplate);
            MouseCommand = new Command(OnMouseEvent);
            SerializeCommand = new Command(Serialize);
            DeserializeCommand = new Command(Deserialize);
            PlayCommand = new Command(StrWin);
            CellTemplate = new MyCell(CellType.None);
            AvilableLvl = MainMenuParameters.Levels;
            ScoresRequired = "0";
            Model = new EditorModel(Level);
            Level = MainMenuParameters.DefaultLevel?? AvilableLvl[MainMenuParameters.LevelNumb];
            Deserialize(null);
            _fieldHeight = new GridLength(Model.YSize * 30);
            _fieldWidth = new GridLength(Model.XSize * 30);
        }

        private bool CheckCphInput(string input)
        {
            int outNumb;
            if (int.TryParse(input, out outNumb) && outNumb >= 0) return true;
            (new Exception(ExceptionType.IncorrectInput)).ShowException();
            return false;
        }

        private bool _isMovable = false;
        private void DragField(object eventArgs)
        {
            var mouseArgs = eventArgs as MouseEventArgs;
            if (mouseArgs == null || mouseArgs.LeftButton == MouseButtonState.Released)
            {
                _isMovable = false;
                return;
            }
            if (mouseArgs.OriginalSource as Ellipse != null)
            {
                _isMovable = true;
                return;
            }
            if (_isMovable)
            {
                var position = mouseArgs.GetPosition(((FrameworkElement) mouseArgs.OriginalSource).Parent as UIElement);
                UpField = new GridLength(position.Y + 10);
                LeftField = new GridLength(position.X + 10);
            }

        }

        public void OnMouseEvent(object eventArgs)
        {
            if (eventArgs is MouseButtonEventArgs) Change(eventArgs);
            else if (eventArgs is MouseWheelEventArgs) ChangeTemplate(null);
            else if (eventArgs is MouseEventArgs) Change(eventArgs);
        }

        public void Change(object eventArgs)
        {
            MouseEventArgs mea;
            FrameworkElement element;
            MyCell cell;
            if (eventArgs == null
                || (mea = eventArgs as MouseEventArgs) == null
                || (element = mea.OriginalSource as FrameworkElement) == null
                || (cell = element.DataContext as MyCell) == null
                ) return;

            if (!(eventArgs is MouseButtonEventArgs) && mea.LeftButton != MouseButtonState.Pressed) return;
            if(cell.TypeOfCell == CellType.Murphy && CellTemplate.TypeOfCell != CellType.Murphy)
            {
                _isMurphySet = false;
                ChangeTemplate(null);
            }
            if (cell.TypeOfCell != CellType.Cookie && CellTemplate.TypeOfCell == CellType.Cookie)
                Model.CookiesSet++;
            if (cell.TypeOfCell == CellType.Cookie && CellTemplate.TypeOfCell != CellType.Cookie)
                Model.CookiesSet--;
            cell.TypeOfCell = CellTemplate.TypeOfCell;
            var ran = new Random();
            cell.TypeOfScissors = (DirectionType)ran.Next(0, 4);
            if (CellTemplate.TypeOfCell == CellType.Murphy)//reseting templete after setting Murphy
            {
                _isMurphySet = true;
                CellTemplate.TypeOfCell = CellType.None;
                ChangeTemplate(((int)CellType.None).ToString());
            }
        }

        public void ChangeTemplate(object eventArgs)
        {
            MurfButtonVisibility = (_isMurphySet) ? Visibility.Collapsed : Visibility.Visible;
            if (eventArgs == null)return;
            var newTemplate = int.Parse((string) eventArgs);
            if (_isMurphySet && CellTemplate.TypeOfCell == CellType.Murphy)//templates scrolling when Murphy is already selected
                newTemplate = (int) CellType.None;
            CellTemplate.TypeOfCell = (CellType)newTemplate;
            if (_isMurphySet && newTemplate > (int) CellType.Murphy)//if there are buttons after Murphy button
                newTemplate--;
            ButtonSelectedNumb = newTemplate;
        }

        public void Serialize(object eventArgs)
        {
            if (!Model.CheckMurphySet())
            {
                (new Exception(ExceptionType.ForgotMurphy)).ShowException();
                return;
            }
            if (!AvilableLvl.Contains(_level))
                AvilableLvl.Add(_level);
            var savedModel = Model.DeepCopy();
            savedModel.ManageBorders(true);
            savedModel.Serialize();
            SaveConsequence();
        }

        private void SaveConsequence()
        {
            var index = int.Parse(LevelIndex);
            if (index > AvilableLvl.Count)
                index = AvilableLvl.Count;
            if (index < 0)
                index = 0;
            if (AvilableLvl.Contains(Level))
                AvilableLvl.Remove(Level);
            AvilableLvl.Insert(index == 0? 0: index - 1, Level);
            File.WriteAllLines(MainMenuParameters.LvlOrderFilePath, AvilableLvl, Encoding.UTF8);
        }

        public void Deserialize(object eventArgs)
        {
            if (Level != MainMenuParameters.DefaultLevel)
            {
                bool isExist = false;
                foreach (var lvl in AvilableLvl)
                {
                    if (Level == lvl)
                    {
                        isExist = true;
                        break;
                    }
                }
                if (!isExist)
                {
                    (new Exception(ExceptionType.LevelNotFound)).ShowException();
                    return;
                }
                var thisWindow = eventArgs as Window;
                if (thisWindow != null)
                {
                    MainMenuParameters.LevelNumb = AvilableLvl.IndexOf(Level);
                    var window = new ConstructWindow();
                    window.Show();
                    window.Activate();
                    thisWindow.Close();
                    Application.Current.MainWindow = window;
                }
            }
            Model.Deserialize();
            Model.CookiesCount();
            ScoresRequired = Model.scoresNeed.ToString();
            Model.ManageBorders(false);
            NotifyPropertyChanged("Model");
            NotifyPropertyChanged("NewCells");
            NotifyPropertyChanged("NewRows");
            _isMurphySet = Model.CheckMurphySet();
            ChangeTemplate(null);
        }

        private int LvlNumb()
        {
            List<int> lvl_numbs = new List<int>();
            foreach (var lvl in AvilableLvl)
            {
                int numb;
                var nmPrts = lvl.Split(' ');
                if (nmPrts.Length > 1 && int.TryParse(nmPrts[1], out numb))
                    lvl_numbs.Add(numb);
            }
            var res = 1;
            if(lvl_numbs.Count > 0)
                res = lvl_numbs.Max() + 1;
            return res;
        }

        public void WinSized(object eventArgs)
        {
            Model.VisibleFieldY = WinH;
            Model.VisibleFieldX = WinW;//saved Window.Height and Width
        }
        public void StrWin(object eventArgs)
        {
            var current = Application.Current.MainWindow;
            var window = new StartWindow(false);
            window.Show();
            window.Activate();            
            current.Close();
            Application.Current.MainWindow = window;
        }

        public void NotifyPropertyChanged(string property)
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
