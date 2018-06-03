using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace Supaplex
{
    public enum MoveType
    {
        Common,
        Falling,
        Rolling
    }

    public abstract class MovingEntity
    {
        protected GameModel Model;
        private bool _isExploded;
        public bool IsFalling;

        public Vector Loc;

        public Vector NextLoc;

        public abstract CellType OperatedCell { get; }
        protected MovingEntity(GameModel model, Vector location)
        {
            Model = model;
            Loc = NextLoc = location;
        }

        public static MovingEntity CreateEntity(GameModel model, Vector location)
        {
            var cell = location.Get(model.Data);
            if (cell == null) 
                return null;
            switch (cell.TypeOfCell)
            {
                case CellType.Murphy:
                    return new Murphy(model, location);
                case CellType.Cookie:
                    return new Cookie(model, location);
                case CellType.Ball:
                    return new Ball(model, location);
                case CellType.Scissors:
                    return new Scissors(model, location);
                default:
                    return new OtherEntity(model, location);
            }
        }

        public MovingEntity ShallowCopy()
        {
            return (MovingEntity) MemberwiseClone();
        }

        protected bool MovedCommon()
        {
            if (_isExploded)
            {
                Explode(false);
                return false;
            }
            var cell = Loc.Get(Model.Data);
            cell.ToBeFilled = false;
            NextLoc.Get(Model.Data).ToBeFilled = false;
            if (NextLoc.Equals(Loc))
            {
                IsFalling = false;
                return false;
            }
            cell.TypeOfCell = CellType.None;
            return true;
        }
        public abstract void Moved();
        public abstract string Moving(Vector direct, out MyCell cell, MoveType typeOfMoving);
        public void Explode(bool explodeType)
        {
            _isExploded = explodeType;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var cellLoc = new Vector(Loc.X + x, Loc.Y + y);
                    MovingEntity expEntity;
                    Model.MovingEntities.TryGetValue(cellLoc, out expEntity);
                    if (cellLoc.Get(Model.Data) != null)
                    {
                        var expCell = Model.Data[cellLoc.Y][cellLoc.X];
                        var lastType = expCell.TypeOfCell;
                        expCell.TypeOfCell = explodeType? CellType.Explosing: CellType.None;
                        expCell.MovingAnimationDir = "";
                        expCell.ToBeFilled = false;
                        if (!explodeType)//adding neighbours after the call
                            CreateEntity(Model, cellLoc).AddUpperEntities();
                        if (lastType == CellType.Murphy)
                            Model.GameFinishing(false);
                        if (expEntity == null) continue;
                        expEntity.NextLoc = expEntity.Loc;
                        if(!explodeType)
                            Model.MovingEntities.Remove(Loc);
                    }
                }
            }
        }
        protected void AddUpperEntities()// traversing entities to check
        {
            if (GetType() != typeof (Murphy))
            {
                IsFalling = true;
                if (!Model.MovingEntities.ContainsKey(NextLoc))
                    Model.MovingEntities.Add(NextLoc, this);
                else
                    Model.MovingEntities[NextLoc] = this;
            }
            for (int x = Loc.X - 1; x <= Loc.X + 1; x++) //adding 3 upper cells
            {
                AddMovingEntity(x, Loc.Y - 1);
            }
        }

        protected void AddMovingEntity(int x, int y)
        {
            var loc = new Vector(x, y);
            var addCell = loc.Get(Model.Data);
            if (addCell == null) return;

            //adding falling objects only
            if (addCell.TypeOfCell != CellType.Ball && addCell.TypeOfCell != CellType.Cookie) return;

            if (Model.MovingEntities.ContainsKey(loc)) return; // if the element is added

            var entity = CreateEntity(Model, loc);

            Model.MovingEntities.Add(loc, entity);
        }
    }

    public class OtherEntity : MovingEntity//create for every entity ideally
    {
        public OtherEntity(GameModel model, Vector location) : base(model, location)
        {
        }

        public override CellType OperatedCell
        {
            get { return CellType.None; }//random value for all instanses. Not used yet
        }

        public override string Moving(Vector direct, out MyCell cell, MoveType typeOfMoving)
        {
            cell = null;
            return "";
        }

        public override void Moved()
        {
        }
    }

    public class Murphy: MovingEntity
    {
        public override CellType OperatedCell
        {
            get { return CellType.Murphy;}
        }

        public Murphy(GameModel model, Vector location) : base(model, location)
        {
        }

        public override void Moved()
        {
            if (!MovedCommon()) return;
            AddUpperEntities();
            AddMovingEntity(Loc.X + 1, Loc.Y);
            AddMovingEntity(Loc.X - 1, Loc.Y);
            var newCell = NextLoc.Get(Model.Data);
            if (Model.MovingEntities.ContainsKey(NextLoc) && newCell.TypeOfCell != CellType.Ball)
                Model.MovingEntities.Remove(NextLoc);
            newCell.TypeOfCell = CellType.Murphy;
            Loc = NextLoc;
        }

        public override string Moving(Vector direct, out MyCell cell, MoveType typeOfMoving)
        {
            Vector newLoc = Loc + direct;
            cell = newLoc.Get(Model.Data);
            MovingEntity targetEnt;
            var isTargetFalling = Model.MovingEntities.TryGetValue(newLoc, out targetEnt) && targetEnt.IsFalling;
            if (cell != null && !cell.ToBeFilled && !isTargetFalling)
            {
                if (cell.TypeOfCell == CellType.Cookie) //пcounting cookies
                    Model.ScoresGained ++;
                switch (cell.TypeOfCell)
                {
                    case CellType.Exit:
                        if (Model.AllScoresCollected)
                            Model.GameFinishing(true);
                        break;
                    case CellType.Ball:
                    case CellType.Cookie:
                    case CellType.None:
                    case CellType.Grass:
                        NextLoc = newLoc;
                        var isBall = cell.TypeOfCell == CellType.Ball;
                        if (direct.Equals(Vector.Left))
                        {
                            if (isBall)
                            {
                                //if not than create such a ball
                                if (Model.PushedBall == null || !Model.PushedBall.Loc.Equals(newLoc))
                                    Model.PushedBall = new Ball(Model, newLoc);
                                //if can not be mooved yet
                                if (!Model.PushedBall.CheckPushing(Vector.Left)) break;
                            }
                            Loc.Get(Model.Data).TypeOfMurphy = DirectionType.Left;
                            Model.GridScrollingHorisontal = -1; //field rolling direction set
                            return "L";
                        }
                        if (direct.Equals(Vector.Right))
                        {
                            if (isBall)
                            {
                                //if no pushed ball than create
                                if (Model.PushedBall == null || !Model.PushedBall.Loc.Equals(newLoc))
                                    Model.PushedBall = new Ball(Model, newLoc);
                                //if can not moove the ball
                                if (!Model.PushedBall.CheckPushing(Vector.Right)) break;
                            }
                            Loc.Get(Model.Data).TypeOfMurphy = DirectionType.Right;
                            Model.GridScrollingHorisontal = 1;
                            return "R";
                        }
                        if (isBall) break;
                        if (direct.Equals(Vector.Up))
                        {
                            Loc.Get(Model.Data).TypeOfMurphy = DirectionType.Up;
                            Model.GridScrollingVertical = -1;
                            return "U";
                        }
                        if (direct.Equals(Vector.Down))
                        {
                            Loc.Get(Model.Data).TypeOfMurphy = DirectionType.Down; // change Murphy picture
                            Model.GridScrollingVertical = 1;
                            return "D"; //animation property value changing
                        }
                        break;
                }
            }
            Model.Murphy.Loc.Get(Model.Data).TypeOfMurphy = DirectionType.None;
            NextLoc = Loc;
            return "";
        }
    }

    public class Ball : MovingEntity
    {
        public override CellType OperatedCell
        {
            get { return CellType.Ball;}
        }

        public Ball(GameModel model, Vector location) : base(model, location)
        {
            Pushing = false;
        }

        public bool Pushing;
        private MyCell rollCell;
        public override void Moved()
        {
            if (!MovedCommon()) return;
            if (Pushing)
                Pushing = false;
            if (rollCell != null)
            {
                rollCell.ToBeFilled = false;
                rollCell = null;
            }
            AddUpperEntities();
            Loc = NextLoc;
            Loc.Get(Model.Data).TypeOfCell = OperatedCell;
        }

        public bool CheckPushing(Vector direction)
        {
            var newLoc = Loc + direction;
            if (newLoc.Get(Model.Data).TypeOfCell == CellType.None)
            {
                if (Pushing)
                {
                    Loc.Get(Model.Data).MovingAnimationDir = direction.Equals(Vector.Left)? "L":"R";
                    NextLoc = newLoc;
                    return true;
                }
                Pushing = true;
            }
            return false;
        }

        public override string Moving(Vector direct, out MyCell cell, MoveType typeOfMoving)
        {
            Vector newLoc = Loc + direct;
            cell = newLoc.Get(Model.Data);

            if (cell != null)
            {
                switch (cell.TypeOfCell)
                {
                    case CellType.Cookie:
                    case CellType.Ball:
                        if(typeOfMoving != MoveType.Rolling)
                            break;
                        MovingEntity downBallEntity;
                        if (Model.MovingEntities.TryGetValue(newLoc, out downBallEntity) && !downBallEntity.Loc.Equals(downBallEntity.NextLoc))
                            break;
                        Vector nl = newLoc + Vector.Left;
                        cell = nl.Get(Model.Data);
                        var passCell = (Loc + Vector.Left).Get(Model.Data);
                        if (CheckRolling(cell, passCell))
                        {
                            NextLoc = nl;
                            cell.ToBeFilled = true;
                            passCell.ToBeFilled = true;
                            rollCell = passCell;
                            return "DL";
                        }
                        nl = newLoc + Vector.Right;
                        passCell = (Loc + Vector.Right).Get(Model.Data);
                        cell = nl.Get(Model.Data);
                        if (CheckRolling(cell, passCell))
                        {
                            NextLoc = nl;
                            cell.ToBeFilled = true;
                            passCell.ToBeFilled = true;
                            rollCell = passCell;
                            return "DR";
                        }
                        break;
                    case CellType.Murphy:
                        if (IsFalling)
                            Model.GameFinishing(false);
                        break;
                    case CellType.Scissors:
                        Model.MovingEntities[newLoc].Explode(true);
                        break;
                    case CellType.None:
                        NextLoc = newLoc;
                        cell.ToBeFilled = true;
                        return "D";

                }
            }
            NextLoc = Loc;
            return "";
        }

        private bool CheckRolling(MyCell targCell, MyCell passCell)
        {
            return targCell != null && !targCell.ToBeFilled && !passCell.ToBeFilled
                   && targCell.TypeOfCell.Equals(CellType.None)
                   && passCell.TypeOfCell == CellType.None;
        }
    }

    public class Cookie : Ball
    {
        public override CellType OperatedCell
        {
            get { return CellType.Cookie; }
        }

        public Cookie(GameModel model, Vector location) : base(model, location)
        {
        }

    }

    public class Scissors: MovingEntity
    {
        private HashSet<Vector> _passedPoints; 
        private Vector MoveDir
        {
            get
            {
                var dirNumb = (int) Loc.Get(Model.Data).TypeOfScissors - 1;
                if (dirNumb != -1)
                    return PathBranch.CommonDirectons[dirNumb];
                return Vector.Left;
            }
        }

        private readonly Random _directRandomizer;
        public Scissors(GameModel model, Vector location) : base(model, location)
        {
            _passedPoints = new HashSet<Vector>();
            _directRandomizer = new Random();
        }

        public override CellType OperatedCell
        {
            get { return CellType.Scissors; }
        }

        public override void Moved()
        {
            Model.MovingEntities[NextLoc] = this;
            if (!MovedCommon()) return;
            AddUpperEntities();
            AddMovingEntity(Loc.X + 1, Loc.Y);
            AddMovingEntity(Loc.X - 1, Loc.Y);
            var cell = Loc.Get(Model.Data);
            var scType = cell.TypeOfScissors;
            cell.TypeOfScissors = DirectionType.None;
            Loc = NextLoc;
            cell = Loc.Get(Model.Data);
            cell.TypeOfCell = OperatedCell;
            cell.TypeOfScissors = scType;
        }

        public override string Moving(Vector direct, out MyCell cell, MoveType typeOfMoving)
        {
            cell = null;
            return "";
        }

        private IEnumerable<Vector> GetDirectionsFromLeft()
        {
            var checkDirections = new List<Vector> {Vector.Left, Vector.Up, Vector.Right, Vector.Down};
            var cnt = 0;
            var strInd = checkDirections.IndexOf(MoveDir) - 1;
            for (int i = strInd == -1 ? checkDirections.Count - 1 :strInd; cnt != 4; i++)
            {
                cnt++;
                yield return checkDirections[i];
                if (i == checkDirections.Count - 1)
                    i = -1;
            }
        }

        public bool IsNoWalls()
        {
            var cntr = 0;
            var dirs = PathBranch.CommonDirectons.Concat(PathBranch.DiagDirectons);
            foreach (var dir in dirs)
            {
                var cell = (Loc + dir).Get(Model.Data);
                if (cell != null && cell.TypeOfCell == CellType.None)
                    cntr++;
            }
            return cntr == 6;
        }

        private bool IsCellAccessible(MyCell cell)
        {
            return cell != null && (cell.TypeOfCell == CellType.None || cell.TypeOfCell == CellType.Murphy) &&
                   !cell.ToBeFilled;
        }

        private bool _isDirectChosen;
        public string Moving()
        {
            Vector bufferDir = Vector.Null;
            var directs = new List<Vector>();
            foreach (var direct in GetDirectionsFromLeft())
            {
                var nextCell = (Loc + direct).Get(Model.Data);
                if (nextCell != null && (nextCell.TypeOfCell == CellType.None || nextCell.TypeOfCell == CellType.Murphy) &&
                    !nextCell.ToBeFilled)
                {
                    if (!_passedPoints.Contains(direct))
                        directs.Add(direct);
                    else
                        if (bufferDir == Vector.Null)
                            bufferDir = direct;
                }
            }
            // rotate scissors
            var scissorsType = Loc.Get(Model.Data).TypeOfScissors;
            if (!directs.Contains(MoveDir) && MoveDir != bufferDir)
            {
                Vector nextDir;
                if (directs.Count != 0)
                {
                    nextDir = directs[0];
                    _passedPoints.Add(nextDir);
                }
                else
                {
                    if (bufferDir == Vector.Null)
                        return "";
                    nextDir = bufferDir;
                    _passedPoints.Clear();
                }
                scissorsType = (DirectionType)(Array.IndexOf(PathBranch.CommonDirectons, nextDir) + 1);
                Loc.Get(Model.Data).TypeOfScissors = scissorsType;
                return "";
            }
            // moove in the direction set
            NextLoc = Loc + MoveDir;
            var newCell = NextLoc.Get(Model.Data);
            newCell.ToBeFilled = true;
            if (newCell.TypeOfCell == CellType.Murphy)
            {
                Model.GameFinishing(false);
                return "";
            }
            switch (scissorsType)
            {
                case DirectionType.Down:
                    return "D";
                case DirectionType.Up:
                    return "U";
                case DirectionType.Left:
                    return "L";
                default:
                    return "R";
            }
        }
    }
}
