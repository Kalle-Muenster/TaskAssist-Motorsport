using System;
using System.Runtime.InteropServices;
using System.Drawing;
#if USE_WITH_WPF
using Point = System.Windows.Point;
#else
using Point = System.Drawing.Point;
using Rect = System.Drawing.Rectangle;
#endif


namespace TaskAssist.Geomety
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Point32
    {
        public static readonly Point32 ZERO = new Point32(0x00000000);
        public static readonly Point32 EMPTY = new Point32(0xFFFFFFFF);

        [FieldOffset(0)]
        private UInt32 raw;
        [FieldOffset(0)]
        public Int16 x;
        [FieldOffset(2)]
        public Int16 y;

        public int X
        {
            get { return x; }
            set { x = (short)(value & 0x0000ffff); }
        }

        public int Y
        {
            get { return y; }
            set { y = (short)value; }
        }

        public Point32(UInt32 fromRawParameter) : this()
        {
            raw = fromRawParameter;
        }
        public Point32(Point fromPoint) : this()
        {
            x = (short)fromPoint.X; y = (short)fromPoint.Y;
        }
        public Point32(int ix, int yps) : this()
        {
            x = (short)ix; y = (short)yps;
        }
        public static implicit operator Point64(Point32 cast)
        {
            return new Point64(cast.X, cast.Y);
        }

        public static implicit operator Size(Point32 cast)
        {
            return new Size(cast.x, cast.y);
        }
        public static Point32 operator +(Point32 This, Point32 That)
        {
            return new Point32(This.x + That.x, This.y + That.y);
        }
        public static Point32 operator -(Point32 This, Point32 That)
        {
            return new Point32(This.x - That.x, This.y - That.y);
        }
        public static Point32 operator +(Point32 This, Point That)
        {
            return new Point32(This.x + That.X, This.y + That.Y);
        }
        public static Point32 operator -(Point32 This, Point That)
        {
            return new Point32(This.x - That.Y, This.y - That.Y);
        }
        public static Point32 operator *(Point32 This, float scalar)
        {
            This.x = (short)(This.x * scalar); This.y = (short)(This.y * scalar); return This;
        }
        public static Point32 operator /(Point32 This, short scalar)
        {
            This.y /= scalar; This.x /= scalar; return This;
        }
        public static PointF64 operator /(Point32 This, Point32 That)
        {
            return new PointF64((float)This.x / That.x, (float)This.x / That.x);
        }
        public Point32 fliped() { return new Point32(y, x); }
        public Point32 flip() { x *= -1; x += y; y -= x; x += y; return this; }
        public Point32 flypst() { return new Point32(x, -y); }
        public Point32 flixed() { return new Point32(-x, y); }
        public int Summ() { return x + y; }
        public static implicit operator Point(Point32 cast)
        {
            return new Point(cast.x, cast.y);
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public struct PointF64
    {
        float X;
        float Y;
        public PointF64(float x, float y) { X = x; Y = y; }
        public float Summ() { return X + Y; }
        public static implicit operator System.Drawing.PointF(PointF64 cast)
        {
            return new System.Drawing.PointF(cast.X, cast.Y);
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public struct Point64
    {
        public static readonly Point64 ZERO = (Point64)Point32.ZERO;
        public static readonly Point64 EMPTY = (Point64)Point32.EMPTY;
        public int x;
        public int y;
        public Point64(int X, int Y)
        {
            x = X; y = Y;
        }
        public Point64(Point point)
        {
            x = point.X; y = point.Y;
        }
        public static Point64 operator +(Point64 This, Point64 That)
        {
            return new Point64(This.x + That.x, This.y + That.y);
        }
        public static Point64 operator -(Point64 This, Point64 That)
        {
            return new Point64(This.x - That.x, This.y - That.y);
        }
        public static Point64 operator +(Point64 This, Point32 That)
        {
            return new Point64(This.x + That.x, This.y + That.y);
        }
        public static Point64 operator -(Point64 This, Point32 That)
        {
            return new Point64(This.x - That.x, This.y - That.y);
        }
        public static Point64 operator /(Point64 This, int divisor)
        {
            return new Point64(This.x / divisor, This.y / divisor);
        }
        public static Point64 operator /(Point64 This, Point64 That)
        {
            return new Point64(This.x / That.x, This.y / That.y);
        }
        public static Point64 operator *(Point64 This, Point64 That)
        {
            return new Point64(This.x * That.x, This.y * That.y);
        }
        public static Point64 operator *(Point64 This, float scalar)
        {
            return new Point64((int)(This.x * scalar),
                                (int)(This.y * scalar));
        }
        public override string ToString()
        {
            return string.Format(
                "x:{0} y:{1}", x, y
            );
        }
        public static implicit operator Point64(Point cast)
        {
            return new Point64(cast);
        }
        public static implicit operator Point(Point64 cast)
        {
            return new Point(cast.x, cast.y);
        }
        public static explicit operator Size(Point64 cast)
        {
            return new Size(cast.x, cast.y);
        }
        public static Point64 operator +(Point64 This, Point That)
        {
            return new Point64((int)(This.x + That.X), (int)(This.y + That.X));
        }
        public static Point64 operator -(Point64 This, Point That)
        {
            return new Point64((int)(This.x - That.X), (int)(This.y - That.X));
        }
        public static RECT operator +(Rect That, Point64 This)
        {
            return new RECT(That.Left + This.x, That.Top + This.y, That.Width, That.Height);
        }
        public static RECT operator -(Rect That, Point64 This)
        {
            return new RECT(That.Left - This.x, That.Top - This.y, That.Width, That.Height);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct RECT
    {
        public static readonly RECT ZERO = new RECT(0, 0, 0, 0);
        public static readonly RECT EMPTY = new RECT(-1, -1, -1, -1);
        [FieldOffset(0)]
        public int left;
        [FieldOffset(4)]
        public int top;
        [FieldOffset(8)]
        public int right;
        [FieldOffset(12)]
        public int bottom;

        public Point64 corner
        {
            get { return new Point64(left, top); }
            set
            {
                right += (value.x - left); left = value.x;
                bottom += (value.y - top); top = value.y;
            }
        }
        public Point32 scales
        {
            get { return new Point32((right - left) / 2, (bottom - top) / 2); }
            set
            {
                Point64 c = center; corner = c - value;
                right = c.x + value.x; bottom = c.y + value.y;
            }
        }
        public Point32 length
        {
            get { return new Point32(right - left, bottom - top); }
            set { right = left + value.x; bottom = top + value.y; }
        }
        public Point64 center
        {
            get { return corner + scales; }
            set
            {
                Point64 s = scales; corner = value - s;
                right = value.x + s.x; bottom = value.y + s.y;
            }
        }

        public RECT(int x, int y, int w, int h)
        {
            left = x; top = y; right = x + w; bottom = y + h;
        }
        public RECT(Rect rect)
        {
            left = rect.Left; right = rect.Right; top = rect.Top; bottom = rect.Bottom;
        }
        public override string ToString()
        {
            return corner.ToString() + string.Format(
                " w:{0} h:{1}", right - left, bottom - top
            );
        }
        public static implicit operator Rect(RECT cast)
        {
            return new Rect(cast.corner, cast.length);
        }
        public bool Contains(Point location)
        {
            return ((Rect)this).Contains(location);
        }
    }

    /// <summary> PointPT (struct)
    /// A structure which defines a point in 2D space via two 
    /// individual pointer values, each pointing a distant variables.
    /// </summary>
    public unsafe struct PointPT
    {
        public IntPtr pX;
        public IntPtr pY;

        public PointPT(ref Point32 refereto)
        {
            fixed (short* p = &refereto.x)
            {
                pX = new IntPtr(p);
                pY = new IntPtr(p + 1);
            }
        }

        public PointPT(ref short ixo, ref short ypso)
        {
            fixed (short* p = &ixo)
            {
                pX = new IntPtr(p);
            }
            fixed (short* p = &ypso)
            {
                pY = new IntPtr(p);
            }
        }

        public PointPT(IntPtr xptr, IntPtr ypsp)
        {
            pX = xptr;
            pY = ypsp;
        }

        public static implicit operator Point32(PointPT cast)
        {
            return new Point32(
                *(short*)cast.pX.ToPointer(),
                *(short*)cast.pY.ToPointer()
            );
        }

        public int X
        {
            get { return *(short*)pX.ToPointer(); }
            set { *(short*)pX.ToPointer() = (short)value; }
        }
        public int Y
        {
            get { return *(short*)pY.ToPointer(); }
            set { *(short*)pY.ToPointer() = (short)value; }
        }

        public void Set(Point32 point)
        {
            X = point.x;
            Y = point.y;
        }

    }
}
