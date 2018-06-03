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
using System.Windows.Controls;
using System.Windows.Input;

namespace Supaplex
{
    public class EditorModel : Serialisation, INotifyPropertyChanged
    {

        private int _winHeight;
        private int _winWidth;
        private int _cookiesSet;
        public bool AddCellsDir = true;
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

        }
        public override int YSize
        {
            get
            {
                if(Data != null)
                return Data.Count;
                return 0;
            }

        }

        //redefine window size
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


        public EditorModel(string level): base(level)
        {
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

        /// method for mesuring the field's size
        public void Fill(CellType fillCellType, int newXSize, int newYSize, out bool isMurphRemoved)//возвращаем затерт ли Мерфи
        {
            var oldX = XSize;
            isMurphRemoved = false;
            if (oldX > newXSize)
            {
                for (int i = 0; i < YSize; i++)
                {
                    var start = AddCellsDir ? oldX - 1 : oldX - newXSize - 1;
                    var stop = AddCellsDir ? newXSize : 0;
                    for (int j = start; j >= stop; j--)
                    {
                        if (Data[i][j].TypeOfCell.Equals(CellType.Murphy))
                            isMurphRemoved = true;
                        Data[i].Remove(Data[i][j]);
                    }
                }
            }

            if (oldX < newXSize)
                for (int i = 0; i < YSize; i++)
                {
                    for (int j = 0; j < newXSize - oldX; j++)
                    {
                        if (AddCellsDir)
                            Data[i].Add(new MyCell(fillCellType));
                        else
                            Data[i].Insert(0, new MyCell(fillCellType));
                    }
                }

            //добавление/стерание по игрику
            var oldY = YSize;
            if (oldY > newYSize)
            {
                var start = AddCellsDir ? oldY - 1 : oldY - newYSize - 1;
                var stop = AddCellsDir ? newYSize : 0;
                for (int i = start; i >= stop; i--)
                {
                    foreach (var cell in Data[i])
                    {
                        if (cell.TypeOfCell.Equals(CellType.Murphy))
                            isMurphRemoved = true;
                    }
                    Data.Remove(Data[i]);
                }
            }

            if (oldY < newYSize)
            {
                for (int i = 0; i < newYSize - oldY; i++)
                {
                    var newRow = new ObservableCollection<MyCell>();
                    for (int j = 0; j < XSize; j++)
                        newRow.Add(new MyCell(fillCellType));
                    if (AddCellsDir)
                        Data.Add(newRow);
                    else
                    {
                        Data.Insert(0, newRow);
                    }
                }
            }
            CookiesCount();
            AddCellsDir = true;
        }

        public bool CheckMurphySet()
        {
            foreach (var row in Data)
            {
                foreach (var myCell in row)
                {
                    if (myCell.TypeOfCell == CellType.Murphy)
                        return true;
                }
            }
            return false;
        }

        public void ManageBorders(bool isAddBoreders)
        {
            int d = isAddBoreders ? 1 : -1;
            bool isRemoved;
            Fill(CellType.Stone, XSize, YSize + d, out isRemoved);
            Fill(CellType.Stone, XSize + d, YSize, out isRemoved);
            AddCellsDir = false;
            Fill(CellType.Stone, XSize, YSize + d, out isRemoved);
            AddCellsDir = false;
            Fill(CellType.Stone, XSize + d, YSize, out isRemoved);
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

        public EditorModel DeepCopy()
        {
            var dataCopy = new ObservableCollection<ObservableCollection<MyCell>>();
            foreach (var row in Data)
            {
                var newRow = new ObservableCollection<MyCell>();
                foreach (var myCell in row)
                {
                    newRow.Add(new MyCell(myCell.TypeOfCell));
                }
                dataCopy.Add(newRow);
            }
            var modelCopy = new EditorModel(Level)
            {
                Data = dataCopy,
                scoresNeed = scoresNeed,
                VisibleFieldX = VisibleFieldX,
                VisibleFieldY = VisibleFieldY
            };
            return modelCopy;
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
