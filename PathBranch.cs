using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Supaplex
{
    public enum PathCondition
    {
        TargetsFound = -1,
        None,
        TargetReplaced,
        Danger,
        TargetsLost
    }
    class PathBranch
    {
        public enum MapType
        {
            MapRoom,
            MapTarget,
            MapCheck
        }

        public ModelManager ModelUsage;
        public Dictionary<Vector, WayPoint> MapedPoints;
        public List<WayPoint> OptimalPath;
        public MyCell[][] Zone;
        public static readonly Vector[] CommonDirectons = { Vector.Up, Vector.Down, Vector.Left, Vector.Right };//связан с DirectionType (ножницы)
        public static readonly Vector[] DiagDirectons = { new Vector(-1, -1), new Vector(1, 1), new Vector(-1, 1), new Vector(1, -1)};
        public Dictionary<Vector, FallPoint> FallPoints;
        public Dictionary<Vector, KeyPoint> KeyPoints;
        public MapType TypeOfMap;
        public List<WayPoint> CurrPath;
        public List<WayPoint> AllPath;
        public bool IsToFinish;
        public WayPoint Target;
        public Vector[] ExceptPoints = new Vector[0];
        public PathCondition PathValue = 0;
        public List<Vector> Targets;
        public int BallFalledNumb = 0;
        public double PathEffect;
        public Vector ExceptTerget;

        public CellType TargetCell
        {
            get
            {
                return ModelUsage.Model.AllScoresCollected ? CellType.Exit : CellType.Cookie;
            }
        }
        private List<int> _motionsStartIndexes;

        public PathBranch(ModelManager gameModel)
        {
            _motionsStartIndexes = new List<int>();
            FallPoints = new Dictionary<Vector, FallPoint>();
            KeyPoints = new Dictionary<Vector, KeyPoint>();
            MapedPoints = new Dictionary<Vector, WayPoint>();
            ModelUsage = gameModel;
            Zone = ModelUsage.Model.Data;
            CurrPath = new List<WayPoint>();
            Targets = new List<Vector>();
        }

        public bool TryCompletePath(out Vector currTarget)//path through all the cookies one by one and to the exit 
        {
            CurrPath.Clear();
            currTarget = new Vector(-1,-1);
            if (AllPath == null)
            {
                AllPath = new List<WayPoint> {new WayPoint(ModelUsage.MurphLoc, 0)};
                Targets.Clear();
            }
            TypeOfMap = MapedPoints.Count == 0 ? MapType.MapRoom : MapType.MapTarget;
            Target = null;
            Map(new WayPoint(ModelUsage.MurphLoc, 0));
            if (Target == null)//no target - no way
                return false;
            AddPoint(Target);
            if(!RestorePath(Target))
                return false;
            TypeOfMap = MapType.MapCheck;
            Map(new WayPoint(ModelUsage.MurphLoc, 0));
            foreach (var pathPoint in OptimalPath)
            {
                ModelUsage.TryMoving(pathPoint);
                AllPath.Add(pathPoint);
            }

            PathValue = GetPathValue();
            currTarget = Target.Location;
            return true;
        }


        private PathCondition GetPathValue()//random path evaluation
        {
            var value = PathCondition.None;
            if (ModelUsage.EntitiesMotions.Count != 0) //if something has fallen
                BallFalledNumb ++;
            foreach (var motion in ModelUsage.EntitiesMotions)
            {
                if (motion[0] == Target.Location)//target replaced
                    value = PathCondition.TargetReplaced;
            }
            var checkModel = ModelUsage;
            checkModel.EstimateResult(Vector.Null, Vector.Null);
            foreach (var motion in ModelUsage.EntitiesMotions)
                _motionsStartIndexes.Add(motion.Count - 1);//filling indexes to start the checkup
            //for (int i = 0; i < ModelUsage.EntitiesMotions.Count; i++)
            //{
            //    var motions = ModelUsage.EntitiesMotions[i];
            //    var startIndex = _motionsStartIndexes[i];
            //    var lastCell = checkModel.Model.Data[motions.Last().Y][motions.Last().X];
            //    checkModel.Model.Data[motions[startIndex].Y][motions[startIndex].X].TypeOfCell = lastCell.TypeOfCell;
            //    lastCell.TypeOfCell = CellType.None;
            //}
            if (checkModel.Model.Result == GameResult.Failed)//player has died
                return PathCondition.Danger;
            //number of potential targets has changed
            TypeOfMap = MapType.MapCheck;
            var lastTargetsCnt = Targets.Count;
            Map(new WayPoint(ModelUsage.MurphLoc, 0));
            if (lastTargetsCnt > Targets.Count)
                value = PathCondition.TargetsLost;
            if (lastTargetsCnt < Targets.Count)
                value = PathCondition.TargetsFound;
            return value;
        }

        private bool IsFallChanged()//if the fallen ball changed something
        {
            foreach (var motion in ModelUsage.EntitiesMotions)
            {
                var newLoc = motion[1];
                var cnt = 0;
                foreach (var diagDirecton in DiagDirectons)
                {
                    var elemType = (newLoc + diagDirecton).Get(Zone).TypeOfCell;
                    if (elemType == CellType.Ball || elemType == CellType.Stone)
                        cnt++;
                }
                if (cnt >= 2) return true;
            }
            return false;
        }

        public bool RestorePath(WayPoint end)//way to the target by indexes
        {
            var currPoint = end;
            while (currPoint.Index != 1)
            {
                WayPoint newPoint;
                while (true)
                {
                    newPoint = CheckNeighbours(currPoint.Index - 1, currPoint.Location);
                    if (newPoint == null)
                        return false;
                    break;
                }
                AddPoint(newPoint);
                currPoint = newPoint;
            }
            CurrPath.Reverse(0, CurrPath.Count);
            OptimalPath = CurrPath;
            return true;
        }

        private WayPoint CheckNeighbours(int ruquiredIndex, Vector currentLoc)
        {
            WayPoint resPoint = null;
            WayPoint bufferResPoint = null;
            foreach (var directon in CommonDirectons)
            {
                var loc = directon + currentLoc;
                WayPoint nextPoint;
                if (!MapedPoints.TryGetValue(loc, out nextPoint) || nextPoint.Index != ruquiredIndex )
                    continue;
                //getting to the key point
                KeyPoint keyPoint;
                if (KeyPoints.TryGetValue(loc, out keyPoint))
                {
                    bufferResPoint = nextPoint;
                    PathTreeBuilder.AddKey(loc);//adding passed key point => potetial new branch
                    //check the mooving consequenses
                    foreach (var leanedEntLoc in keyPoint.LeanedEntities)
                    {
                        foreach (var points in FallPoints[leanedEntLoc].LeanPoints)
                        {
                            if (!points.Contains(ModelUsage.MurphLoc))
                                continue;
                            foreach (var point in points)
                            {
                                if (points.Count != 1 && !KeyPoints[point].isStepped) continue;
                                //ball is going to fall
                                //DO SOMETHING
                            }
                        }
                    }
                    continue;
                }
                resPoint=nextPoint;
                break;
            }
            return resPoint ?? bufferResPoint;
        }

        private void AddPoint(WayPoint pntToAdd)
        {
            var loc = pntToAdd.Location;
            CurrPath.Add(pntToAdd);
            KeyPoint keyPoint;
            if (KeyPoints.TryGetValue(loc, out keyPoint))
            {
                keyPoint.isStepped = true;
            }
            FallPoint thisFallPoint;
            if (FallPoints.TryGetValue(loc, out thisFallPoint))
            {
                thisFallPoint.Remove(KeyPoints);
                FallPoints.Remove(loc);
            }
        }

        public void Map(WayPoint strPoint)//cells indexing
        {
            MapedPoints.Clear();
            var newPntList = AddNeighbours(strPoint);
            while (!IsToFinish)
            {
                var nextWawePnts = new List<WayPoint>();
                foreach (var point in newPntList)
                {
                    foreach (var nhbrPoint in AddNeighbours(point))
                    {
                        nextWawePnts.Add(nhbrPoint);
                    }
                }
                newPntList = nextWawePnts;
                if (nextWawePnts.Count == 0)
                    IsToFinish = true;
            }
            IsToFinish = false;
        }

        private IEnumerable<WayPoint> AddNeighbours(WayPoint point)
        {
            var nhbrs = new List<WayPoint>();
            foreach (var directon in CommonDirectons)
            {
                AddNeighbour(nhbrs, point, directon);
            }
            return nhbrs;
        }
        private bool IsLocked( WayPoint point )//falling ball on the way
        {
            if(TypeOfMap != MapType.MapCheck)return false;
            if (ModelUsage.EntitiesMotions.Count != 0)
                TypeOfMap = TypeOfMap;
            for (int i = 0; i < ModelUsage.EntitiesMotions.Count; i++)
            {
                if (point.Index == ModelUsage.EntitiesMotions[i].IndexOf(point.Location) - _motionsStartIndexes[i])
                    return true;
            }
            return false;
        }
        private void AddNeighbour(List<WayPoint> nhbrs, WayPoint point, Vector nhbDirection)
        {
            var loc = point.Location + nhbDirection;
            if (MapedPoints.ContainsKey(loc))
                return;
            var index = point.Index + 1;
            var cell = loc.Get(Zone);
            if(cell == null)
                return;
            var newPoint = new WayPoint(loc, index);
            if (cell.TypeOfCell != CellType.Ball && cell.TypeOfCell != CellType.Stone && (ExceptPoints == null || !ExceptPoints.Contains(loc)) && !IsLocked(newPoint))
            {
                nhbrs.Add(newPoint);
                MapedPoints.Add(loc, newPoint);
                MovingEntity entity;
                var isTargetFalling = ModelUsage.Model.MovingEntities.TryGetValue(loc, out entity) && entity.IsFalling;
                if (cell.TypeOfCell == TargetCell && !isTargetFalling )//defining the path
                {
                    if (Target == null && loc != ExceptTerget)
                        Target = newPoint;
                    if (TypeOfMap != MapType.MapTarget && !Targets.Contains(newPoint.Location))
                        Targets.Add(newPoint.Location);//potential targets
                    if (TypeOfMap == MapType.MapTarget && Target != null)
                        IsToFinish = true;//stopping the wawe
                }
            }
            if ((cell.TypeOfCell == CellType.Ball || cell.TypeOfCell == CellType.Cookie) && TypeOfMap == MapType.MapRoom)//запись падабильных обьектов и их "опор"
            {
                var fallPoint = new FallPoint(ModelUsage.Model, loc);
                if (!FallPoints.ContainsKey(loc))
                    FallPoints.Add(loc, fallPoint);
                else return;
                foreach (var pointLoc in fallPoint.FindLeanPoints())
                {
                    KeyPoint existPoint;
                    if (!KeyPoints.TryGetValue(pointLoc, out existPoint))
                        KeyPoints.Add(pointLoc, new KeyPoint(pointLoc, new List<Vector> {loc}));
                    else
                        existPoint.LeanedEntities.Add(fallPoint.Location);
                }
            }
        }
    }
    public class WayPoint
    {
        public readonly Vector Location;
        public int Index;
        public WayPoint(Vector location, int index)
        {
            Location = location;
            Index = index;
        }
        public override int GetHashCode()
        {
            return Location.GetHashCode();
        }
    }

    public class KeyPoint
    {
        public bool isStepped;
        public List<Vector> LeanedEntities;
        public Vector Location;
        public KeyPoint(Vector location, List<Vector> leanedEntities)
        {
            LeanedEntities = leanedEntities;
            Location = location;
            isStepped = false;
        }

        public void Remove(Dictionary<Vector, FallPoint> fallPointsList)
        {
            foreach (var entityLoc in LeanedEntities)
            {
                var leanedEnt = fallPointsList[entityLoc];
                foreach (var points in leanedEnt.LeanPoints)
                {
                    if (points.Contains(Location))
                        points.Remove(Location);
                }
            }
        }
    }

    public class FallPoint
    {
        private static readonly Vector[][] LeanPointsToCheck =
        {
            new[] {Vector.Down},
            new[] {Vector.Left, new Vector(-1, 1)},
            new[] {Vector.Right, new Vector(1, 1)}
        };
        public List<List<Vector>> LeanPoints;
        public MovingEntity ThisEntity;
        private MyCell[][] _data;
        public Vector Location;
        public FallPoint(GameModel model, Vector loc)
        {
            ThisEntity = MovingEntity.CreateEntity(model, loc);
            LeanPoints = new List<List<Vector>>();
            _data = model.Data;
            Location = loc;
        }

        public void Remove(Dictionary<Vector, KeyPoint> keyPointsList)
        {
            foreach (var leanPoints in LeanPoints)
            {
                foreach (var point in leanPoints)
                {
                    if (!keyPointsList.ContainsKey(point))
                        return;
                    keyPointsList[point].LeanedEntities.Remove(Location);
                    if (keyPointsList[point].LeanedEntities.Count == 0)
                        keyPointsList.Remove(point);
                }
            }
            LeanPoints.Clear();
        }

        //on wich the object lies
        public List<Vector> FindLeanPoints()
        {
            var points = new List<Vector>();
            foreach (var leanPoints in LeanPointsToCheck)
            {
                List<Vector> newLans;
                if (TryGetLeanPoints(leanPoints.ToList(), out newLans))
                {
                    LeanPoints.Add(newLans);
                    points.AddRange(newLans);
                }
            }
            return points;
        }

        private bool TryGetLeanPoints(List<Vector> pointsDir, out List<Vector> pointsToAdd)
        {
            var addPoints = new Dictionary<Vector, CellType>();
            foreach (var dir in pointsDir)
            {
                var loc = Location + dir;
                var cell = loc.Get(_data);
                if (cell.TypeOfCell == CellType.Cookie || cell.TypeOfCell == CellType.Grass)
                {
                    addPoints.Add(loc, cell.TypeOfCell);
                    cell.TypeOfCell = CellType.None;
                }
            }
            MyCell target;
            ThisEntity.Moving(Vector.Down, out target, MoveType.Falling);
            ThisEntity.Moving(Vector.Down, out target, MoveType.Rolling);
            var canFall = !ThisEntity.Loc.Equals(ThisEntity.NextLoc);
            foreach (var point in addPoints.Keys)
            {
                var changedCell = point.Get(_data);
                changedCell.TypeOfCell = addPoints[point];
                changedCell.ToBeFilled = false;
                changedCell.MovingAnimationDir = "";
            }
            ThisEntity.NextLoc = ThisEntity.Loc;
            pointsToAdd = addPoints.Keys.ToList();
            return canFall;
        }
    }

}
