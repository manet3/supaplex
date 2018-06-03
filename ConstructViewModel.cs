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
using System.Windows.Input;
using System.Windows;

namespace Supaplex
{
    //public class MyDouble : INotifyPropertyChanged// для биндинга к масиву с прозрачностями кнопок
    //{
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    private double _Value;

    //    public MyDouble(double value)
    //    {
    //        Value = value;
    //    }
    //    public double Value
    //    {
    //        get { return _Value; }
    //        set { _Value = value; OnPropertyChanged("Value"); }
    //    }

    //    void OnPropertyChanged(string propertyName)
    //    {
    //        if (PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //        }
    //    }
    //}

    public class ConstructViewModel: INotifyPropertyChanged
    {
        private ConstructModel _model;
        private bool _isMurphySet;
        private GridLength _fieldWidth;
        private GridLength _fieldHeight;
        private string _level;
        private Visibility _murfButtonVisibility;
        private MyCell _cellTemplate;
        //private double[] _buttonsOpacity;
        private string _scoresRequired;
        private int _buttonSelectedNumb;
        public int WinH { get; set; }

        public int WinW { get; set; }

        public int WinMaxH { get; set; }

        public int WinMaxW { get; set; }

        public string Level
        {
            get { return _level; }
            set
            {
                if (value != null)
                {
                    if (value != "")
                    {
                        _level = value;
                        AvilableLvl.Add(_level);
                    }
                    else
                    {
                        _level = "Level" + " " + LvlNumb();
                    }
                    Model.Level = _level;
                    PropertyChanged(this, new PropertyChangedEventArgs("AvilableLvl"));
                }
            }
        }

        public List<string> AvilableLvl { get; set; }
        public string NewCells
        {
            get { return Model.Width.ToString(); }
            set 
            {
                Model.Width = int.Parse(value); 
                _fieldWidth = new GridLength(Model.Width*30);
                Model.Fill(CellTemplate);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("NewCells"));
                    PropertyChanged(this, new PropertyChangedEventArgs("FieldWidth"));
                }
            }
        }

        public string NewRows
        {
            get { return Model.Height.ToString(); }
            set
            {
                Model.Height = int.Parse(value);
                _fieldHeight = new GridLength(Model.Height * 30);
                Model.Fill(CellTemplate);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("NewRows"));
                    PropertyChanged(this, new PropertyChangedEventArgs("FieldHeight"));
                }
            }
        }

        public GridLength FieldWidth
        {
            get
            {
                return _fieldWidth = new GridLength();
            }
            set
            {
                _fieldWidth = value;
                NewCells = (value.Value / 30).ToString();
            }
        }

        public GridLength FieldHeight
        {
            get
            {
                return _fieldHeight = new GridLength(); 
            }
            set
            {
                _fieldHeight = value;
                NewRows = (value.Value / 30).ToString();
            }
        }

        public ConstructModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Model")); 

            }
        }

        public MyCell CellTemplate
        {
            get { return _cellTemplate; }
            set
            {
                _cellTemplate = value;
                //foreach (MyDouble opacity in ButtonsOpacity)
                //    opacity.Value = 0.6;
                //ButtonsOpacity[(int)_cellTemplate.TypeOfCell].Value = 9.5;
            }
        }

        //public ObservableCollection<MyDouble>  ButtonsOpacity { get; set; }

        public int ButtonSelectedNumb
        {
            get { return _buttonSelectedNumb; }
            set
            {
                _buttonSelectedNumb = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ButtonSelectedNumb")); 
            }
        }


        public ICommand WinSizedCommand{ get; set; }
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
                _scoresRequired = value;
                Model._scoresNeed = int.Parse(value);
            }
        }

        public Visibility MurfButtonVisibility
        {
            get { return _murfButtonVisibility; }
            set
            {
                _murfButtonVisibility = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("MurfButtonVisibility")); 
            }
        }

        public ConstructViewModel()
        {
            //ButtonsOpacity = new ObservableCollection<MyDouble>();
            //for (int i = 0; i < 6; i++)
            //    ButtonsOpacity.Add(new MyDouble(0.6));//начальные значения прозрачности кнопок
            //ButtonsOpacity[0].Value = 9.5;//выбрана по умочанию
            //WinSizedCommand = new Command(WinSized);
            //ResizeWinChgCommand = new Command(WinResMode);
            ChangeTypeCommand = new Command(ChangeTemplate);
            MouseCommand = new Command(OnMouseEvent);
            SerializeCommand = new Command(Serialize);
            DeserializeCommand = new Command(Deserialize);
            PlayCommand = new Command(StrWin);
            CellTemplate = new MyCell(CellType.None);
            AvilableLvl = new List<string>();
            LvlGet();
            Model = new ConstructModel(Level);
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
            if (CellTemplate.TypeOfCell == CellType.Murphy)//Мерфи поставили
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
            if (_isMurphySet && CellTemplate.TypeOfCell == CellType.Murphy)//когда поставили Мерфи
                newTemplate = (int) CellType.None;
            CellTemplate.TypeOfCell = (CellType)newTemplate;
            if (_isMurphySet && newTemplate > (int) CellType.Murphy)//если есть кнопки, стоящие после кнопки Мерфи
                newTemplate--;
            ButtonSelectedNumb = newTemplate;
            //if (CellTemplate.TypeOfCell >= CellType.Exit)
            //    CellTemplate.TypeOfCell = CellType.None;
            //else
            //    CellTemplate.TypeOfCell++;
        }

        public void Serialize(object eventArgs)
        {
            Model.Serialize();
            AvilableLvl.Add(Level);
        }

        public void Deserialize(object eventArgs)
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
            if(!isExist)
                throw new ApplicationException("No level name assigned. Please, input the name of level, you want to load");
            Model.Deserialize();
            Model.CookiesCount();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Model"));
                PropertyChanged(this, new PropertyChangedEventArgs("NewCells"));
                PropertyChanged(this, new PropertyChangedEventArgs("NewRows"));
            }
        }

        private void LvlGet()
        {
            AvilableLvl.Add("");
            foreach (string name in Directory.GetFiles("Levels/", "*.lvl").Select(Path.GetFileName))
            {
                string[] lvlname = name.Split('.');
                AvilableLvl.Add(lvlname[0]);
            }
        }

        private int LvlNumb()
        {
            List<int> lvl_numbs = new List<int>();
            foreach (var lvl in AvilableLvl)
            {
                int numb;
                var nm_prts = lvl.Split(' ');
                if (nm_prts.Length > 1 && int.TryParse(nm_prts[1], out numb))
                    lvl_numbs.Add(numb);
            }
            var res = 1;
            if(lvl_numbs.Count > 0)
                res = lvl_numbs.Max() + 1;
            return res;
        }
        //public void WinResMode(object eventArgs)
        //{            
        //    WinMaxW = Model.Data[0].Count * 30 + 16;
        //    WinMaxH = Model.Data.Count * 30 + 38;
        //    WinH = Model.WinHeight;
        //    WinW = Model.WinWidth;
        //    var window = Application.Current.MainWindow;
        //    window = new SizeWin();
        //    window.Show();
        //    window.Activate();
        //}

        public void WinSized(object eventArgs)
        {
            Model.VisibleFieldY = WinH;
            Model.VisibleFieldX = WinW;//сериализуемые значения Window.Height и Width
        }
        public void StrWin(object eventArgs)
        {
            var current = Application.Current.MainWindow;
            var window = Application.Current.MainWindow;
            window = new StartWindow();
            window.Show();
            window.Activate();            
            current.Close();
            Application.Current.MainWindow = window;
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
