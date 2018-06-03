using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supaplex
{
    public struct Vector
    {
        public int X;
        public int Y;

        public Vector(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode()
        {
            return string.Format("{0}/{1}", X, Y).GetHashCode();
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }

        public static bool operator ==(Vector a, Vector b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector a, Vector b)
        {
            return a.X != b.X || a.Y != b.Y;
        }


        public MyCell Get(MyCell[][] data)
        {
            return (Y < data.Length && X < data[0].Length && Y >= 0 && X >= 0)
                ? data[Y][X]
                : null;
        }

        public static Vector Left
        {
            get { return new Vector(-1, 0); }
        }

        public static Vector Right
        {
            get { return new Vector(1, 0); }
        }

        public static Vector Up
        {
            get { return new Vector(0, -1); }
        }

        public static Vector Down
        {
            get { return new Vector(0, 1); }
        }

        public static Vector Null
        {
            get { return new Vector(0, 0); }
        }

    }
}
