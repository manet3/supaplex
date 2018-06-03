using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Supaplex
{
    class PathTreeBuilder
    {
        private  List<PathBranch> _analizedBranches;
        public readonly List<PathBranch> MajorBranches; 
        private readonly List<Vector> _keyPoints;
        private readonly HashSet<Vector> _exceptedPoints;
        private readonly ModelManager _modelManager;
        public static Action<Vector> AddKey;
        public PathTreeBuilder()
        {
            _exceptedPoints = new HashSet<Vector>();
            _keyPoints = new List<Vector>();
            _modelManager = AiModel.ModelUsage;
            MajorBranches = new List<PathBranch>();
            _analizedBranches = new List<PathBranch> { new PathBranch(_modelManager.Copy()) };
            AddKey += AddNewKey;
        }

        public List<WayPoint> GetFinalPath()
        {
            StartAnalizeBranches();
            for (int i = 0; i < MajorBranches.Count; i ++)
            {
                _exceptedPoints.Clear();
                _keyPoints.Clear();
                if(MajorBranches[i].ModelUsage.Model.Result != GameResult.None)
                    return MajorBranches[i].AllPath;
                _analizedBranches = new List<PathBranch> { MajorBranches[i]};
                StartAnalizeBranches();
                if (i == MajorBranches.Count - 1)
                    i = -1;
            }
            return null;
        }
        public List<PathBranch> AnalizeBranches()
        {
            //token/messed keypoits combination
            var keyPoints = _keyPoints.ToArray();
            _keyPoints.Clear();
            double n = 0;
            for (var j = 0; j < keyPoints.Length; j++)//number of variants
                n += Math.Pow(2, j);
            //if (n == 0)
            //{
            //    var branch = new PathBranch(_modelManager.Copy());
            //    branch.TryCompletePath(out target);
            //}
            for (var i = 1; i <= n; i ++)
            {
                var isExcept = new BitArray(BitConverter.GetBytes(i));
                var points = new List<Vector>();
                for (var l = 0; l < keyPoints.Length; l++)
                {
                    if(isExcept[l])
                        points.Add(keyPoints[l]);
                }
                var branch = new PathBranch(_modelManager.Copy())
                {
                    ExceptPoints = points.ToArray()
                };
                Vector target;
                if (!branch.TryCompletePath(out target))//deadlock branch
                {
                    _keyPoints.Clear();
                    continue; 
                }
                _analizedBranches.Add(branch);
                if (points.Count != 0 && _keyPoints.Count != 0)
                {
                    var pntsToAdd = _keyPoints;
                    AddRangePnts(pntsToAdd);
                    AnalizeBranches();
                    RemoveRangePnts(pntsToAdd);
                }
            }
            return _analizedBranches;
        }

        public List<PathBranch> StartAnalizeBranches()
        {
            Vector target;
            //_analizedBranches[0].TryCompletePath(out target);
            var res = new List<PathBranch>();
            while (res.Count == 0)
            {
                res = AnalizeBranches();
                _analizedBranches[0].TryCompletePath(out target);
            }
            AddRangePnts(_keyPoints);
            GetMajorBarnches();
            return res;
        }
        private void AddRangePnts(List<Vector> points)
        {
            foreach (var keyPoint in points)
            {
                if (!_exceptedPoints.Contains(keyPoint))
                    _exceptedPoints.Add(keyPoint);
            }
        }
        private void RemoveRangePnts(List<Vector> points)
        {
            foreach (var keyPoint in points)
            {
                if (_exceptedPoints.Contains(keyPoint))
                    _exceptedPoints.Remove(keyPoint);
            }
        }
        private void GetMajorBarnches()
        {
            MajorBranches.Clear();
            foreach (var analizedBranch in _analizedBranches)
            {
                if (analizedBranch.PathValue == PathCondition.TargetsFound)
                {
                    MajorBranches.Add(analizedBranch);
                    return;
                }
            }
            var notChgBranch = GetNoneChgBranche();
            if (notChgBranch != null)
            {
                MajorBranches.Add(notChgBranch);
                return;
            }
            foreach (var analizedBranch in _analizedBranches)
            {
                //if (analizedBranch.TargetCell == CellType.Exit)
                //{
                //    MajorBranches.Add(analizedBranch);
                //    return;
                //}
                MajorBranches.Add(new PathBranch(_modelManager.Copy()) {Target = null, ExceptTerget = analizedBranch.Target.Location});
            }
        }

        private PathBranch GetNoneChgBranche()
        {
            Func<PathBranch, PathBranch, int> comparator =
                (br1, br2) => br1.BallFalledNumb.CompareTo(br2.BallFalledNumb);
            _analizedBranches.Sort(comparator.Invoke);
            var resBranches = new List<PathBranch>();
            foreach (var analizedBranch in _analizedBranches)
            {
                if (analizedBranch.PathValue != PathCondition.None)
                    continue;
                if (resBranches.Count == 0)
                {
                    resBranches.Add(analizedBranch);
                    continue;
                }
                //добавляем только пути, меньшие последнего добавленного
                if (resBranches.Last().CurrPath.Count <= analizedBranch.CurrPath.Count)
                    continue;
                if (comparator(analizedBranch, resBranches.Last()) == 0)
                    resBranches[resBranches.Count - 1] = analizedBranch;
                else resBranches.Add(analizedBranch);
            }
            var alternPaths = new List<PathBranch>();
            for (int i = 1; i < resBranches.Count - 1; i++)
            {
                resBranches[i].PathEffect = (resBranches[0].CurrPath.Count - resBranches[i].CurrPath.Count)/
                                            (resBranches.Last().BallFalledNumb - resBranches[i].BallFalledNumb);
                alternPaths.Add(resBranches[i]);
            }
            alternPaths.Sort((b1, b2) => b1.PathEffect.CompareTo(b2.PathEffect));
            return resBranches.Count ==0?null: (alternPaths.Count != 0? alternPaths[0]:resBranches[0]);
        }

        public void AddNewKey(Vector exceptPnt)
        {
            if(_exceptedPoints.Contains(exceptPnt))return;
            _keyPoints.Add(exceptPnt);
        }
    }
}
