using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Supaplex
{
    public class ConstructModel : Serialisation, INotifyPropertyChanged
    {

        private int _winHeight;
        private int _winWidth;
        private int _cells;
        private int _rows;
        private int _cookiesSet = 0;

        public int CookiesSet
        {
            get { return _cookiesSet; }
            set
            {
                _cookiesSet = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("CookiesSet"));
            }
        }

        public ObservableCollection<ObservableCollection<MyCell>> Data { get; set; }

        public override IEnumerable<IEnumerable<MyCell>> FieldData
        {
            get { return Data; }
            set { Data = new ObservableCollection<ObservableCollection<MyCell>>(value.Select(x => new ObservableCollection<MyCell>(x))); }
        }

        public override int XSize
        {
            get
            {
                if(Data != null)
                return Data[0].Count;
                return 0;
            }
            set
            {
                Width = value;
            }
        }
        public override int YSize
        {
            get
            {
                if(Data != null)
                return Data.Count;
                return 0;
            }
            set
            {
                Height = value;
            }
        }

        //переопределяем размеры окна
        public override int VisibleFieldY
        {
            get
            {
                return _winHeight;
            }
            set
            {
                if (value > 0)
                    _winHeight = value;
                else _winHeight = Data.Count;
            }
        }

        public override int VisibleFieldX
        {
            get
            {
                return _winWidth;
            }
            set
            {
                if (value > 0)
                    _winWidth = value;
                else _winWidth = Data[0].Count;
            }
        }
        public int Width
        {
            get
            {
                return Data[0].Count;

            }
            set 
            {
                _cells = value;
                if (_cells == 0 && Data != null)
                    _cells = Data[0].Count; 
            }
        }

        public int Height
        {
            get
            {
                return Data.Count;
            }
            set 
            { 
                _rows = value;
                if (_rows == 0 && Data != null)
                    _rows = Data.Count;
            }
        }


        public ConstructModel(string level): base(level)
        {
            Width = 3;
            Height = 3;
            Data = new ObservableCollection<ObservableCollection<MyCell>>
            {
                new ObservableCollection<MyCell>
                {
                    new MyCell(CellType.None),
                    new MyCell(CellType.None),
                    new MyCell(CellType.None)

                },
                new ObservableCollection<MyCell>
                {
                    new MyCell(CellType.None),
                    new MyCell(CellType.None),
                    new MyCell(CellType.None)
                },
                new ObservableCollection<MyCell>
                {
                    new MyCell(CellType.None),
                    new MyCell(CellType.None),
                    new MyCell(CellType.None)
                }
            };
            VisibleFieldY = Data.Count;
            VisibleFieldX = Data[0].Count;
        }

        /// <summary>
        /// Этот метод для изменения размера поля
        /// </summary>
        /// <param name="cellTemplate"></param>
        public void Fill(MyCell cellTemplate)
        {
            if (cellTemplate.TypeOfCell == CellType.Murphy)
            {
                Height = _rows;
                Width = _cells;
                return;
            }
            int cellSet = Width;
            int rowsSet = Height;
            if (_cells < cellSet)
            {
                for (int i = 0; i < _rows; i++)
                    for (int j = Data[i].Count - 1; j >= _cells; j--)
                    {
                        Data[i].Remove(Data[i][j]);
                    }
                return;
            }

            if (_rows < rowsSet)
            {
                for (int i = rowsSet - 1; i >= _rows; i--)
                {
                    Data.Remove(Data[i]);
                }
                return;
            }

            if (_rows > rowsSet)
            {

                for (int i = 0; i < _rows - rowsSet; i++)
                {
                    Data.Add(new ObservableCollection<MyCell>());
                    for (int j = 0; j < cellSet; j++)
                        Data[Height - 1].Add(new MyCell(cellTemplate.TypeOfCell));
                }
                return;
            }

            if (_cells > cellSet)
                for (int i = 0; i < rowsSet; i++)
                {
                    for (int j = 0; j < _cells - cellSet; j++)
                        Data[i].Add(new MyCell(cellTemplate.TypeOfCell));
                }
            CookiesCount();
        }

        public void CookiesCount()//подсчет количества установленых печениек
        {
            int cookiesSet = 0;
            foreach (var cellRow in Data)
                {
                    foreach (var cell in cellRow)
                    {
                        if (cell.TypeOfCell == CellType.Cookie)
                            cookiesSet++;
                    }
                }
            CookiesSet = cookiesSet;
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
