using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supaplex
{
    class ModelManager
    {
        public Vector MurphLoc
        {
            get { return Model.Murphy != null ? Model.Murphy.Loc : Model.MurfLoc(); }
        }

        public GameModel Model;
        public List<List<Vector>> EntitiesMotions; 

        public ModelManager(GameModel model)
        {
            Model = model;
            EntitiesMotions = new List<List<Vector>>();
            Model.EntityMoved += AddMovedEntities;
        }

        private void Move(GameModel model)
        {
            model.CheckMoving();
            model.Replace();
        }
        public bool TryMoving(WayPoint nextPoint)
        {
            Model.DirectVector = nextPoint.Location - MurphLoc;
            var lastLoc = MurphLoc;
            Move(Model);
            Model.DirectVector = Vector.Null;
            return !MurphLoc.Equals(lastLoc);
        }

        private bool _isSubscribed;
        public void EstimateResult(Vector lastLoc, Vector newLoc)
        {
            if (!_isSubscribed)
            {
                Model.EntityMoved += EstimateResult;
                _isSubscribed = true;
            }
            Move(Model);
            if (_isSubscribed)
            {
                Model.EntityMoved -= EstimateResult;
            }
            Move(Model);
        }

        private void AddMovedEntities(Vector lastLoc, Vector newLoc)
        {
            foreach (var entitiesMotion in EntitiesMotions)
            {
                if (entitiesMotion.Last() == lastLoc)
                {
                    entitiesMotion.Add(newLoc);
                    return;
                }
            }
            EntitiesMotions.Add(new List<Vector>{lastLoc, newLoc});
        }

        public ModelManager Copy()
        {
            var newMovedEntList = new List<List<Vector>>();
            foreach (var moution in EntitiesMotions)
            {
                newMovedEntList.Add(moution.ToList());
            }
            var checkModel = new ModelManager(Model.DeepCopy()){EntitiesMotions = newMovedEntList};
            return checkModel;
        }

        //public bool IsMovable()
        //{
            
        //}
    }
}
