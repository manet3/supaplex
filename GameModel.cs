using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Supaplex
{
    public enum GameResult
    {
        None,
        Failed,
        Won
    }
    public class GameModel : Serialisation, INotifyPropertyChanged
    {
        private int _xSize;
        private int _ySize;
        private int _scoresGained;
        private double _gridScrollingVertical;
        private double _gridScrollingHorisontal;

 

        public double GridScrollingVertical// grid and game field animation direction
        {
            get { return _gridScrollingVertical; }
            set
            {
                _gridScrollingVertical += value * 30;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("GridScrollingVertical"));
            }
        }

        public double GridScrollingHorisontal
        {
            get { return _gridScrollingHorisontal; }
            set
            {
                _gridScrollingHorisontal += value * 30;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("GridScrollingHorisontal"));
            }
        }

        public int ScoresNeed
        {
            get
            {
                return scoresNeed;                 
            }
            set { scoresNeed = value; }
        }

        public event Action ScoresChanged;
        public event Action<bool> GameOver;
        public event Action<Vector, Vector> EntityMoved;
        public int ScoresGained
        {
            get { return _scoresGained; }
            set
            {
                _scoresGained = value;
                AllScoresCollected = _scoresGained >= ScoresNeed;
                CheckExit();
                if (ScoresChanged != null)
                    ScoresChanged();
            }
        }

        public bool AllScoresCollected;
        public override int VisibleFieldY { get; set; }//size of the seen part
        public override int VisibleFieldX { get; set; }

        public override int XSize
        {
            get { return Data[0].Length; }
        }

        public override int YSize
        {
            get { return Data.Length; }
        }

        public MyCell[][] Data { get; set; }

        public override IEnumerable<IEnumerable<MyCell>> FieldData
        {
            get { return Data; }
            set { Data = value.Select(x => x.ToArray()).ToArray(); }
           
        }

        public GameResult Result { get; set; }

        public Vector DirectVector;
        public Vector NextLoc;
        public MovingEntity Murphy;
        public Dictionary<Vector, MovingEntity> MovingEntities = new Dictionary<Vector, MovingEntity>();
        public Ball PushedBall;
        private string _backGround;

        public GameModel(string level) : base(level)
        {
            DirectVector = Vector.Null;
        }

        public void Init(int scores)//defining game field and all the things related
        {
            ScoresGained = scores;
            Murphy = MovingEntity.CreateEntity(this, MurfLoc());
            for(int i = 0; i < Data.Length; i ++)
                for (int j = 0; j < Data[0].Length; j++)
                {
                    var loc = new Vector(j, i);
                    if (loc.Get(Data).TypeOfCell == CellType.Murphy) continue;
                    var newEnt = MovingEntity.CreateEntity(this, loc);
                    if (!MovingEntities.ContainsKey(newEnt.Loc))
                        MovingEntities.Add(newEnt.Loc, newEnt);
                }
            CheckMoving();
        }

        public void CheckExit()
        {
            if (!AllScoresCollected) return;
            foreach (var cellAr in Data)
                foreach (var cell in cellAr)
                    if (cell.TypeOfCell.Equals(CellType.Exit))
                        cell.TypeOfExit = ExitType.Open;
        }

        public void CheckMoving()//check and run the animation
        {
            MyCell target;
            //delete cells after explosion
            var que = MovingEntities.Values.ToList();
            foreach (var entity in que)
                //mooving check and running the animation
            {
                var newCell = entity.Loc.Get(Data);
                newCell.MovingAnimationDir = entity.Moving(Vector.Down, out target, MoveType.Falling);
            }
            foreach (var entity in que)
            {
                if(entity.GetType() == typeof(Scissors))continue;
                var newCell = entity.Loc.Get(Data);
                if (newCell.TypeOfCell != CellType.Ball && newCell.TypeOfCell != CellType.Cookie)//СПАСИТЕЛЬНЫЙ КОСТЫЛЬ
                    continue;
                newCell.MovingAnimationDir = entity.Moving(Vector.Down, out target, MoveType.Rolling);
            }
            foreach (var entity in que)
            {
                if (entity.Loc.Get(Data).TypeOfCell == CellType.Scissors)
                {
                    entity.Loc.Get(Data).MovingAnimationDir = ((Scissors)entity).Moving();
                }
            }
            var cell = Murphy.Loc.Get(Data);
            if (cell == null)return;//just in case
            cell.MovingAnimationDir = Murphy.Moving(DirectVector, out target, MoveType.Common);//проверка на движение и запуск анимации Murphy
            if (target != null && target.TypeOfCell != CellType.None && target.TypeOfCell != CellType.Ball)//анимация сжатия сьедаемой ячейки    
                target.MovingAnimationDir = "S" + cell.MovingAnimationDir;
        }

        public void Replace()//changing Data after animation
        {
            var queue = MovingEntities.Values.ToArray();//MovingEntities can not be traversed while beeing changed
            foreach (var movingEntity in queue)
            {
                movingEntity.Loc.Get(Data).DebugBackground = new SolidColorBrush(Colors.Transparent);
            }
            MovingEntities.Clear();//delete replaced elements

            if (PushedBall != null)
            {
                PushedBall.Loc.Get(Data).MovingAnimationDir = "";
                PushedBall.Moved();
            }
            foreach (var entity in queue)// replacing all the entities if needed
            {
                entity.Loc.Get(Data).MovingAnimationDir = "";
                if (entity.Loc != entity.NextLoc)
                    if(EntityMoved != null)
                    EntityMoved(entity.Loc, entity.NextLoc);
                entity.Moved();
            }
            Murphy.Loc.Get(Data).MovingAnimationDir = "";
            Murphy.Moved();//replace the player
        }

        public GameModel DeepCopy()
        {
            //model.Init(Model.ScoresGained);
            var dataCopy = new MyCell[YSize][];//copy of game field for AI mod
            for (int i = 0; i < YSize; i++)
            {
                dataCopy[i] = new MyCell[XSize];
                for (int j = 0; j < XSize; j++)
                {
                    dataCopy[i][j] = new MyCell(Data[i][j].TypeOfCell);
                }
            }
            var entitiesCopy = new Dictionary<Vector, MovingEntity>();
            foreach (var entity in MovingEntities)
            {
                entitiesCopy.Add(entity.Key, entity.Value.ShallowCopy());
            }
            var modelCopy = new GameModel(Level)
            {
                Data = dataCopy, ScoresNeed = ScoresNeed, MovingEntities = entitiesCopy
            };
            modelCopy.Init(ScoresGained);
            return modelCopy;
        }

        public event PropertyChangedEventHandler PropertyChanged;

 

        public void GameFinishing(bool IsWon)
        {
            if (IsWon)
                Result = GameResult.Won;
            else
            {
                Result = GameResult.Failed;
                Murphy.Explode(true);
            }
            if (GameOver != null)
                GameOver(IsWon);
        }
    }
}
