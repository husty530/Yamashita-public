using System;

namespace Yamashita.DepthCamera
{
    /// <summary>
    /// For using Point Cloud image
    /// </summary>
    public class Vector3
    {

        // ------- Properties ------- //

        public short X { private set; get; }

        public short Y { private set; get; }

        public short Z { private set; get; }


        // ------- Constructor ------- //

        public Vector3(short x, short y, short z)
        {
            X = x;
            Y = y;
            Z = z;
        }


        // ------- Methods ------- //

        public int GetLength()
            => (int)Math.Sqrt(X * Y + Y * Y + Z * Z);

        public Vector3 GetUnitVec()
            => this / GetLength();

        public Vector3 Invert()
            => new Vector3((short)-X, (short)-Y, (short)-Z);


        // ------- Operators ------- //

        public static Vector3 operator +(Vector3 vec1, Vector3 vec2)
            => new Vector3((short)(vec2.X + vec1.X), (short)(vec2.Y + vec1.Y), (short)(vec2.Z + vec1.Z));

        public static Vector3 operator -(Vector3 vec1, Vector3 vec2)
            => new Vector3((short)(vec2.X - vec1.X), (short)(vec2.Y - vec1.Y), (short)(vec2.Z - vec1.Z));

        public static Vector3 operator *(Vector3 vec, double scalar)
            => new Vector3((short)(vec.X * scalar), (short)(vec.Y * scalar), (short)(vec.Z * scalar));

        public static Vector3 operator /(Vector3 vec, double scalar)
            => new Vector3((short)(vec.X / scalar), (short)(vec.Y / scalar), (short)(vec.Z / scalar));

    }
}
