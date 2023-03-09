using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Security.Cryptography;
#if USE_WITH_WPF
using Point = System.Windows.Point;
#else
using Point = System.Drawing.Point;
using Rect = System.Drawing.Rectangle;
#endif


namespace Stepflow.Gui.Geometry
{
    public interface IPoint 
    {
        int X { get; set; }
        int Y { get; set; }
    }

    public interface IPointImpl<impl> : IPoint where impl : IPoint
    {
        impl fliped();
        impl flypst();
        impl flixed();
        impl flip();
    }

    public interface IPoint<T,impl> : IPointImpl<impl> where T : struct where impl : IPoint
    {
        T x { get; set; }
        T y { get; set; }

        T Summ();
        ValueTuple<T,T> tuple();
        IPoint<T, impl> point();
    }

    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct PointF32 : IPoint<Float16,PointF32>
    {
        [FieldOffset(0)]
        private UInt32 raw;
        [FieldOffset(0)]
        public Float16 X;
        [FieldOffset(2)]
        public Float16 Y;

        Float16 IPoint<Float16,PointF32>.x { get { return X; } set { X = value; } }
        int IPoint.X { get { return Convert.ToInt32(X); } set { X = (Float16)value; } }
        Float16 IPoint<Float16,PointF32>.y { get { return Y; } set { Y = value; } }
        int IPoint.Y { get { return Convert.ToInt32(Y); } set { Y = (Float16)value; } }

        public PointF32( UInt32 fromRawParameter ) : this()
        {
            raw = fromRawParameter;
        }
        public PointF32( Point fromPoint ) : this()
        {
            X = (Float16)fromPoint.X; Y = (Float16)fromPoint.Y;
        }
        public PointF32( Float16 ix, Float16 yps ) : this()
        {
            X = ix; Y =  yps;
        }
        public static implicit operator Point64( PointF32 cast )
        {
            return new Point64((int)cast.X,(int)cast.Y);
        }

        public static implicit operator Size( PointF32 cast )
        {
            return new Size((int)cast.X,(int)cast.Y);
        }
        public static PointF32 operator +( PointF32 This, PointF32 That )
        {
            return new PointF32(This.X + That.X, This.Y + That.Y);
        }
        public static PointF32 operator -( PointF32 This, PointF32 That )
        {
            return new PointF32(This.X - That.X, This.Y - That.Y);
        }
        public static PointF32 operator +( PointF32 This, Point That )
        {
            return new PointF32(This.X + That.X, This.Y + That.Y);
        }
        public static PointF32 operator -( PointF32 This, Point That )
        {
            return new PointF32(This.X - That.Y, This.Y - That.Y);
        }
        public static PointF32 operator *( PointF32 This, float scalar )
        {
            This.X = (short)( This.X * scalar ); This.Y = (short)( This.Y * scalar ); return This;
        }
        public static PointF32 operator /( PointF32 This, short scalar )
        {
            This.Y /= scalar; This.X /= scalar; return This;
        }
        public static PointF64 operator /( PointF32 This, PointF32 That )
        {
            return new PointF64((float)This.X / That.X, (float)This.Y / That.Y);
        }
        public IPoint<Float16,PointF32> point() { return this; }
        public PointF32 fliped() { return new PointF32(Y,X); }
        public PointF32 flip() { X *= -Float16.One; X += Y; Y -= X; X += Y; return this; }
        public PointF32 flypst() { return new PointF32(X,-Y); }
        public PointF32 flixed() { return new PointF32(-X,Y); }
        public Float16 Summ() { return X + Y; }

        ValueTuple<Float16,Float16> IPoint<Float16,PointF32>.tuple()
        {
            return this;
        }

        public static implicit operator ValueTuple<Float16,Float16>( PointF32 cast )
        {
            return ( cast.X, cast.Y );
        }

        public static implicit operator PointF( PointF32 cast )
        {
            return new PointF( (int)cast.X, (int)cast.Y );
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Point32 : IPoint<short,Point32>
    {
        public static readonly Point32 ZERO = new Point32(0x00000000);
        public static readonly Point32 EMPTY = new Point32(0xFFFFFFFF);

        [FieldOffset(0)]
        private UInt32 raw;
        [FieldOffset(0)]
        public short x;
        [FieldOffset(2)]
        public short y;

        short IPoint<short,Point32>.x { get { return x; } set { x = value; } }
        short IPoint<short,Point32>.y { get { return y; } set { y = value; } }

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
        public Point32(ValueTuple<short,short> fromTuple) : this()
        {
            x = fromTuple.Item1; y = fromTuple.Item2;
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
            return new PointF64((float)This.x / That.x, (float)This.y / That.y);
        }
        public Point32 fliped() { return new Point32(y, x); }
        public Point32 flypst() { return new Point32(x, -y); }
        public Point32 flixed() { return new Point32(-x, y); }
        public Point32 flip() { x *= -1; x += y; y -= x; x += y; return this; }
        public short   Summ() { return (short)(x + y); }

        public IPoint<short,Point32> point() { return this; }
        ValueTuple<short,short> IPoint<short,Point32>.tuple() {
            return this;
        }

        public static implicit operator Point32( ValueTuple<short,short> cast ) {
            return new Point32( cast );
        }
        public static implicit operator ValueTuple<short,short>( Point32 cast ) {
            return ( cast.x, cast.y );
        }

        public static implicit operator Point(Point32 cast) {
            return new Point( cast.x, cast.y );
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct PointF64 : IPoint<float,PointF64>
    {
        [FieldOffset(0)] public ulong raw;
        [FieldOffset(0)] public float X;
        [FieldOffset(4)] public float Y;
        public PointF64(float x, float y) : this() { X = x; Y = y; }

        public float x { get { return X; } set { X = value; } }
        public float y { get { return Y; } set { Y = value; } }

        int IPoint.X { get { return (int)X; } set { X = value; } }
        int IPoint.Y { get { return (int)Y; } set { Y = value; } }

        public PointF64 flip() { X *= -1.0f; X += Y; Y -= X; X += Y; return this; }
        public PointF64 fliped() { return new PointF64(Y,X); }
        public PointF64 flixed() { return new PointF64(-X,Y); }
        public PointF64 flypst() { return new PointF64(X,-Y); }
        public float Summ() { return X + Y; }
        public IPoint<float,PointF64> point() { return this; }

        public static implicit operator ValueTuple<float,float>(PointF64 cast)
        {
            return (cast.X,cast.Y);
        }
        ValueTuple<float,float> IPoint<float,PointF64>.tuple()
        {
            return this;
        }

        public static implicit operator PointF64( ValueTuple<float,float> cast )
        {
            return new PointF64(cast.Item1,cast.Item2);
        }
        public static implicit operator PointF( PointF64 cast ) {
            return new PointF( cast.X, cast.Y );
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct Point64 : IPoint<int,Point64>
    {
        public static readonly Point64 ZERO = (Point64)Point32.ZERO;
        public static readonly Point64 EMPTY = (Point64)Point32.EMPTY;
        [FieldOffset(0)] public ulong raw;
        [FieldOffset(0)] public int x;
        [FieldOffset(4)] public int y;
        public int X { get { return x; } set { x = value; } }
        public int Y { get { return y; } set { y = value; } }

        int IPoint<int,Point64>.x { get { return X; } set { X = value; } }
        int IPoint<int,Point64>.y { get { return Y; } set { Y = value; } }

        public Point64(int X, int Y) : this()
        {
            x = X; y = Y;
        }
        public Point64(Point point) : this()
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

        public Point64 flip() { X *= -1; X += Y; Y -= X; X += Y; return this; }
        public Point64 fliped() { return new Point64(Y, X); }
        public Point64 flixed() { return new Point64(-X, Y); }
        public Point64 flypst() { return new Point64(X, -Y); }
        public int Summ() { return X + Y; }

        public static implicit operator ValueTuple<int,int>(Point64 cast)
        {
            return (cast.x, cast.y);
        }

        public static implicit operator Point64( ValueTuple<int,int> cast ) 
        {
            return new Point64(cast.Item1, cast.Item2);
        }

        ValueTuple<int,int> IPoint<int,Point64>.tuple()
        {
            return this;
        }

        public IPoint<int,Point64> point() { return this; }

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
    public unsafe struct PointPT : IPoint<short,Point32>
    {
        public IntPtr pX;
        public IntPtr pY;

        public PointPT( ref Point32 refereto )
        {
            fixed (short* p = &refereto.x)
            {
                pX = new IntPtr(p);
                pY = new IntPtr(p + 1);
            }
        }

        public PointPT( ref short ixo, ref short ypso )
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

        public PointPT( IntPtr xptr, IntPtr ypsp )
        {
            pX = xptr;
            pY = ypsp;
        }

        public static implicit operator Point32( PointPT cast )
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

        public short x {
            get { return *(short*)pX.ToPointer(); }
            set { *(short*)pX.ToPointer() = value; }
        }
        public short y {
            get { return *(short*)pY.ToPointer(); }
            set { *(short*)pY.ToPointer() = value; }
        }

        public void Set( Point32 point )
        {
            X = point.x;
            Y = point.y;
        }

        public Point32 flip() { X *= -1; X += Y; Y -= X; X += Y; return new Point32(X,Y); }
        public Point32 fliped() { return new Point32(Y, X); }
        public Point32 flixed() { return new Point32(-X, Y); }
        public Point32 flypst() { return new Point32(X,-Y); }
        public short Summ() { return (short)(X + Y); }

        public ValueTuple<short,short> tuple()
        {
            return ( x, y );
        }

        public IPoint<short,Point32> point()
        {
            return this;
        }

        public static implicit operator ValueTuple<short,short>( PointPT cast )
        {
            return cast.tuple();
        }
    }
}
