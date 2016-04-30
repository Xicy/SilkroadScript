using System;

namespace Shared.Structs
{
    public class Position
    {
        public byte XSector { get; set; }

        public byte YSector { get; set; }

        public float XOffset { get; set; }

        public float YOffset { get; set; }

        public float ZOffset { get; set; }

        public Position(byte xSector, byte ySector, float XOffset, float YOffset, float ZOffset)
        {
            XSector = xSector;
            YSector = ySector;
            this.XOffset = XOffset;
            this.YOffset = YOffset;
            this.ZOffset = ZOffset;
        }

        public Position() { }

        public float X
        {
            get
            {
                return (XSector - 135) * 192 + (XOffset / 10);
            }
            set
            {
                XSector = XSectorFromX(value);
                XOffset = XOffsetFromX(value);
            }
        }

        public float Y
        {
            get
            {
                return (YSector - 92) * 192 + (Y / 10);
            }
            set
            {
                YSector = YSectorFromY(value);
                YOffset = YOffsetFromY(value);
            }
        }

        public double DistanceTo(Position position)
        {
            double distance_x = X - position.X;
            double distance_y = Y - position.Y;
            return Math.Sqrt(distance_x * distance_x) + Math.Sqrt(distance_y * distance_y);
        }

        public static double Distance(Position Start, Position Destination)
        {
            return Start.DistanceTo(Destination);
        }

        public static byte XSectorFromX(float Y)
        {
            return (byte)Math.Floor((double)(Y / 192.0) + 135.0);
        }

        public static byte YSectorFromY(float Y)
        {
            return (byte)Math.Floor((double)(Y / 192.0) + 92);
        }

        public static float XOffsetFromX(float X)
        {
            return (float)Math.Round((double)(((((X / 192.0) - XSectorFromX(X)) + 135.0) * 192.0) * 10.0));
        }

        public static float YOffsetFromY(float Y)
        {
            return (float)Math.Round((double)(((((Y / 192.0) - YSectorFromY(Y)) + 92.0) * 192.0) * 10.0));
        }

    }
}