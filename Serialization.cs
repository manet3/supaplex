using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;

namespace Supaplex
{
    //just writes to files without saving the object
    public abstract class Serialisation
    {
        #region Public properties

        public int scoresNeed = 0;
        public abstract int VisibleFieldY { get; set; }
        public abstract int VisibleFieldX { get; set; }
        public abstract int XSize { get; }
        public abstract int YSize { get; }
        public string Level { get; set; }


        public abstract IEnumerable<IEnumerable<MyCell>> FieldData { get; set; }

        #endregion

        protected Serialisation(string level)
        {
            Level = level;
        }

        public void Serialize()
        {
            var name = "Resources/Levels/" + Level + ".lvl";
            using (FileStream fs = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                var bw = new BinaryWriter(fs);
                bw.Write(scoresNeed);
                bw.Write(VisibleFieldY);
                bw.Write(VisibleFieldX);
                bw.Write(YSize);
                bw.Write(XSize);
                foreach (var row in FieldData)
                {
                    foreach (var item in row)
                    {
                        bw.Write((int)item.TypeOfCell);
                    }
                }
            }
        }
        public Vector MurfLoc()
        {
            var x = 0;
            var y = 0;
            foreach (var row in FieldData)
            {
                foreach (var cell in row)
                {
                    if (cell.TypeOfCell == CellType.Murphy)
                    {
                        return new Vector(x, y);
                    }
                    x++;
                }
                x = 0;
                y++;
            }
            throw new ApplicationException("No player found (some stupid bitch has not placed Murphy)!!!");
        }

        public void Deserialize()
        {
            var extent = Level != MainMenuParameters.DefaultLevel ? ".lvl" : "";
            var name = "Resources/Levels/" + Level + extent;
            using (FileStream fs = new FileStream(name, FileMode.OpenOrCreate))
            {
                BinaryReader br = new BinaryReader(fs);
                scoresNeed = br.ReadInt32();
                VisibleFieldY = br.ReadInt32();
                VisibleFieldX = br.ReadInt32();
                var ySize = br.ReadInt32();
                var xSize = br.ReadInt32();
                FieldData = GetAllRows(br, ySize, xSize);
            }
            var ran = new Random();
            foreach (var row in FieldData)
            {
                foreach (var myCell in row)
                {
                    if (myCell.TypeOfCell == CellType.Scissors)
                        myCell.TypeOfScissors = (DirectionType)ran.Next(0, 4);
                }
            }
        }

        private static IEnumerable<MyCell> GetRow(BinaryReader br, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new MyCell((CellType) br.ReadInt32());
            }
        }

        private static IEnumerable<IEnumerable<MyCell>> GetAllRows(BinaryReader br, int rowCount, int colCount)
        {
            for (int i = 0; i < rowCount; i++)
            {
                yield return GetRow(br, colCount);
            }
        }
    }
}
