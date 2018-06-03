using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Supaplex
{
    public enum CellType
    {
        None = 0,
        Grass = 1,
        Ball = 2,
        Stone = 3,
        Cookie = 4,
        Murphy = 5,
        Exit,
        Scissors,
        Explosing
    }

    public enum DirectionType//related to PathBranch.CommonDirectons(scissors)
    {
        None,
        Up,
        Down,
        Left,
        Right,
    }



    public enum ExitType
    {
        Closed,
        Open
    }
    public class AnimationHelper : DependencyObject
    {
        public static Duration GetAnimationDuration(DependencyObject obj)
        {
            return (Duration)obj.GetValue(AnimationDurationProperty);
        }

        public static void SetAnimationDuration(DependencyObject obj, Duration value)
        {
            obj.SetValue(AnimationDurationProperty, value);
        }

        // Using a DependencyProperty as the backing store for AnimationDuration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.RegisterAttached("AnimationDuration", typeof(Duration), typeof(AnimationHelper), new PropertyMetadata(Duration.Automatic, OnAnimationDurationChanged));

        private static void OnAnimationDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (d.GetType() == typeof (DoubleAnimation))
            //    d = new DoubleAnimation(0, 30, (Duration) e.NewValue);
            FrameworkElement element = d as FrameworkElement;
            element.Loaded += (s, arg) => element.RenderTransform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, 30, (Duration)e.NewValue) { RepeatBehavior = RepeatBehavior.Forever });
        }
    }

    public class MyCell: INotifyPropertyChanged
    {
        private CellType _typeOfCell;
        private string _movingAnimationDir;
        private DirectionType _typeOfMurphy;
        private ExitType _typeOfExit = ExitType.Closed;
        private int _zIndex = 1;
        private Duration _animSpeed = new Duration(TimeSpan.FromMilliseconds(20));
        private SolidColorBrush _debugBackground;
        private DirectionType _typeOfScissors;

        public CellType TypeOfCell 
        {
            get
            {
                return _typeOfCell;   
            }
            set
            {
                _typeOfCell = value;
                NotifyPropertyChanged("Img");
            }
        }

        public SolidColorBrush DebugBackground
        {
            get { return _debugBackground; }
            set
            {
                _debugBackground = value; 
                NotifyPropertyChanged("DebugBackground");
            }
        }

        public Duration AnimSpeed
        {
            get { return new Duration(TimeSpan.Parse("0:0:5")); }
            set
            {
                _animSpeed = value;
                NotifyPropertyChanged("AnimSpeed");
            }
        }

        public DirectionType TypeOfMurphy
        {
            get { return _typeOfMurphy; }
            set
            {
                _typeOfMurphy = value;
                NotifyPropertyChanged("Img");
            }
        }

        public DirectionType TypeOfScissors
        {
            get { return _typeOfScissors; }
            set
            {
                _typeOfScissors = value;
                NotifyPropertyChanged("Img");
            }
        }

        public ExitType TypeOfExit
        {
            get { return _typeOfExit; }
            set
            {
                _typeOfExit = value;
                NotifyPropertyChanged("Img");
            }
        }

        public MyCell(CellType typeOfCell)
        {
            TypeOfCell = typeOfCell;
        }

        public string MovingAnimationDir
        {
            get { return _movingAnimationDir; }
            set
            {
                _movingAnimationDir = value;
                if (value == "")
                    ZIndex = DefaultZIndex;
                else ZIndex = 2;
                NotifyPropertyChanged("MovingAnimationDir");
            }
        }


        protected const int DefaultZIndex = 1;

        public int ZIndex
        {
            get { return _zIndex; }
            set
            {
                _zIndex = value;
                NotifyPropertyChanged("ZIndex");
            }
        }

        public string Img
        {
            get
            {
                switch (TypeOfCell)
                {
                    case CellType.None:
                        return "Images/none.png";
                    case CellType.Grass:
                        return "Images/grass.bmp";
                    case CellType.Ball:
                        return "Images/ball.png";
                    case CellType.Stone:
                        return "Images/stone.bmp";
                    case CellType.Cookie:
                        return "Images/cookie.png";
                    case CellType.Murphy:
                        switch (TypeOfMurphy)
                        {
                            case DirectionType.Up:
                                return "Images/MurphyUp.png";
                            case DirectionType.Down:
                                return "Images/MurphyD.png";
                            case DirectionType.Left:
                                return "Images/MurphyL.png";
                            case DirectionType.Right:
                                return "Images/MurphyR.png";
                        }
                        return "Images/Murphy.png";
                    case CellType.Scissors:
                        switch (TypeOfScissors)
                        {
                            case DirectionType.Up:
                                return "Images/ScissorsU.png";
                            case DirectionType.Down:
                                return "Images/ScissorsD.png";
                            case DirectionType.Left:
                                return "Images/ScissorsL.png";
                        }
                        return "Images/ScissorsR.png";
                    case CellType.Explosing:
                        return "Images/explosing.png";
                    case CellType.Exit:
                        if(TypeOfExit == ExitType.Open)
                            return "Images/exit.gif";
                        return "Images/exit_closed.png";
                }
                return null;
            }
        }

        //if the cell is filled after the animation
        public bool ToBeFilled { get; set; }

        public void NotifyPropertyChanged(string property)
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler PropertyChanged;

                    
    }
}
