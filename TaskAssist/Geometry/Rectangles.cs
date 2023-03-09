using System;
using System.Drawing;
using System.Runtime.InteropServices;
#if USE_WITH_WPF
using Point = System.Windows.Point;
#else
using Point = System.Drawing.Point;
using Rect = System.Drawing.Rectangle;
#endif


namespace Stepflow.Gui.Geometry
{

    public enum StorageLayout
    {
        CenterAndScale,
        CornerAndSizes,
        AbsoluteEdges,
        SystemDefault
    }


    #region IRectangle Interface definition
    // Interface for objects representing geometrical 'Rectangle' shaped, two dimensional areas 
    // which makes able assigning and comparing instances which implement it against each other 
    // in a way which doesn't needs to care about implementation details, storage model layout

    /// <summary>
    /// IRectangleCompounds (partial interface which all IRectangle implementations implicily do implement)
    /// Every IRectangle has a 'Corner' property - always should return position of top left corner - regardles of the concrete model which a rectangle implements, 'Corner' always returns position of its top-left corner and when assigned a value to it must re-position itself so that it's top-left corner will be located at that new given coordinates then 
    /// Every IRectangle has a 'Center' property - always should return position of the rectangles center point - and any implementation should ensure when new value is assigned to 'Center' it re-positions itself so it's center position then will be locateded at that new given coordinates then
    /// Every IRectangle has a 'Sizes' propery - these always shall return the rectangles absolute width and height values - and it should ensure that when new values is assigned to 'Sizes' it's sizes will change regarding to that assigned values accordingly 
    /// ...Scale - (like radius or half size values) - also assignable 
    /// ...abstract data accessibility via Compound pointers 'A' and Compound Pointers 'B' - where 'CompoundA' points the values stoted in the lower 32bit of the structure (bits 1 to 32) values and CompoundB points the values stored in the higher bits above 32's bit (bits 32 to 64)  
    /// </summary>
    public interface IRectangleCompounds
    {
        Point32 Corner { get; set; }
        Point32 Center { get; set; }
    
        Point32 Sizes { get; set; }
        Point32 Scale { get; set; }

        PointPT CompoundA { get; }
        PointPT CompoundB { get; }
    }
    
    /// <summary>
    /// IRectangleValues (partial interface which all IRectangle implemantation also will gain implicitly)
    /// </summary>
    public interface IRectangleValues
    {
        int X{ get; set; }
        int Y{ get; set; }
        int W{ get; set; }
        int H{ get; set; }
        int L{ get; set; }
        int R{ get; set; }
        int T{ get; set; }
        int B{ get; set; }
    }
    
    public interface IRectanglePointers
    {
        IntPtr pA1{ get; set; }
        IntPtr pA2{ get; set; }
        IntPtr pB1{ get; set; }
        IntPtr pB2{ get; set; }
    }

    
    /// <summary> IRectangle (main interface for implementing rectangles) </summary>
    public interface IRectangle : IRectangleValues, IRectangleCompounds
    {
        IRectangle converted<RectangleData>() where RectangleData : struct, IRectangle;
        IRectangle<RType,DSize> cast<RType,DSize>() where RType : struct, IRectangle where DSize : struct;
        Rectangle ToRectangle();
        bool Contains( IRectangle other );
        bool Contains( Point64 point );
        bool Intersects( IRectangle other );
        StorageLayout StorageLayout { get; }
    }
    
    /// <summary> IRectanglePtrs<> (interface for implementing rectangles which interpret distant variables for rectangle) </summary>
    /// <typeparam name="RectangleData"></typeparam>
    public interface IRectanglePtrs<RectangleData>
        : IRectangle
        , IRectanglePointers
    where RectangleData
        : struct
        , IRectangle
    {
        IRectanglePtrs<RectangleData> refereTo( ref RectangleData from );
        IRectanglePtrs<RectangleData> casted();
        IRectanglePtrs<RectangleData> copied();
        RectangleData                 resolve();
    }

    /// <summary> IRectangle<> interface for distinct layout specialization via generic parameter </summary>
    /// <typeparam name="RectangleData"></typeparam>
    public interface IRectangle<RectangleData,DataSizeType> : IRectangle where RectangleData : struct, IRectangle where DataSizeType : struct
    {
        IRectangle<RectangleData,DataSizeType> casted();
        RectangleData                     copied();
        RectangleReference<RectangleData> refere();
        RectangleData                     FromRectangle( Rectangle from );
        void                              SetRawData( DataSizeType data );
        DataSizeType                      GetRawData();
    }

    #endregion

    #region ValueType based Rectangles
    // ValueType based Rectangles which using different kinds of data storage and layout models

    // RectangleData - abstract base structure for 64bit sized IRectangle ValueType implementations
    [StructLayout(LayoutKind.Explicit,Size = 8)]
    public unsafe struct RectangleData
    {
        public static readonly RectangleData Empty = new RectangleData();
        [FieldOffset(0)] public fixed byte data[8];
        [FieldOffset(0)] public UInt64  value;
        [FieldOffset(0)] public Point32 compound1;
        [FieldOffset(4)] public Point32 compound2;
    }

    /// <summary>
    /// CenterAndScale - IRectangle implementation - storage: identity 'Center' + 'Scale' values (multipliers for identity width and height - effectively that what would be radius for a circle) for x/y axis  
    /// </summary>
    [StructLayout(LayoutKind.Explicit,Size = 8)]
    public unsafe struct CenterAndScale
        : IRectangle<CenterAndScale,ulong> {
        public static readonly CenterAndScale Empty = new CenterAndScale();
        [FieldOffset(0)] internal RectangleData Abstract; 
        [FieldOffset(0)] public   Point32 Center;
        [FieldOffset(4)] public   Point32 Scale;
    
 
        public CenterAndScale( int centerX, int centerY, int scaleX, int scaleY ) : this() {
            Center.x = (short)centerX;
            Center.y = (short)centerY;
            Scale.x = (short)scaleX;
            Scale.y = (short)scaleY;
        }

        Point32 IRectangleCompounds.Center {
            get {return  Center; }
            set { Center = value; }
        }
        Point32 IRectangleCompounds.Scale {
            get { return Scale; }
            set { Scale = value; }
        }
    
        public Point32 Corner {
            get { return Center - Scale; }
            set { Center = value + Scale; }
        }
        public Point32 Sizes {
            get { return Scale * 2; }
            set { Center = (Center - Scale) + (Scale = (value / 2)); }
        }
        
        public int X { get { return (short)(Center.x - Scale.x); } set { Center.X = value + Scale.X; } }
        public int Y { get { return (short)(Center.y - Scale.y); } set { Center.Y = value + Scale.Y; } }
        public int W { get { return (short)(Scale.x + Scale.x); } set { Center.x += (short)((value /= 2)-Scale.x); Scale.X = value; } }
        public int H { get { return (short)(Scale.y + Scale.y); } set { Center.y += (short)((value /= 2)-Scale.y); Scale.Y = value; } }
        public int L { get { return X; } set { value = (short)( ( value - X ) / 2 ); Center.X += value; Scale.X += value; } }
        public int T { get { return Y; } set { value = (short)( ( value - Y ) / 2 ); Center.Y += value; Scale.Y += value; } }
        public int R { get { return (short)(Center.x + Scale.x); } set { value = (short)((value - R) / 2); Center.X += value; Scale.X += value; } }
        public int B { get { return (short)(Center.y + Scale.y); } set { value = (short)((value - B) / 2); Center.Y += value; Scale.Y += value; } }

        public PointPT CompoundA { get{ return new PointPT(ref Center); } }
        public PointPT CompoundB { get{ return new PointPT(ref Scale); } }

        public StorageLayout StorageLayout {
            get { return StorageLayout.CenterAndScale; }
        }

        public IRectangle<CenterAndScale,ulong> casted() {
            return this;
        }
        IRectangle<RType,DSize> IRectangle.cast<RType,DSize>()
        {
            if( typeof(RType) != typeof(CenterAndScale) )
                return Rectangle<CenterAndScale>.Convert<RType>( this ).cast<RType,DSize>();
            else 
                return casted() as IRectangle<RType,DSize>;
        }
        IRectangle IRectangle.converted<RectangleData>() {
            return Rectangle<CenterAndScale>.Convert<RectangleData>(this);
        }

        RectangleReference<CenterAndScale> IRectangle<CenterAndScale,ulong>.refere()
        {
            return new CenterAndScalePointers( ref this );
        }

        CenterAndScale IRectangle<CenterAndScale,ulong>.copied()
        {
            CenterAndScale clone = CenterAndScale.Empty;
            clone.Center = Center;
            clone.Scale = Scale;
            return clone;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle( X, Y, W, H );
        }
        public static CenterAndScale FromRectangle( Rectangle from )
        {
            CenterAndScale to = CenterAndScale.Empty;
            to.Scale = new Point32( (Point)from.Size ) / 2;
            to.Center = to.Scale + from.Location;
            return to;
        }

        CenterAndScale IRectangle<CenterAndScale,ulong>.FromRectangle( Rectangle from )
        {
            return CenterAndScale.FromRectangle( from );
        }

        public void SetRawData( ulong rawdata )
        {
            Abstract.value = rawdata;
        } 

        public ulong GetRawData()
        {
            return Abstract.value;
        }

        public bool Contains( IRectangle other )
        {
            return L <= other.L && T <= other.T
                && R >= other.R && B >= other.B;   
        }

        public bool Contains( Point64 point )
        {
            return point.x > L && point.x < R
                && point.y > T && point.y < B; 
        }

        public bool Intersects( IRectangle other )
        {
            return Contains(other.Corner)
                || Contains(other.Center + other.Scale)
                || Contains(other.Center + other.Scale.flypst())
                || Contains(other.Center + other.Scale.flixed());
        }

        public static implicit operator ValueTuple<short,short,short,short>( CenterAndScale cast )
        {
            return ((short)cast.X,(short)cast.Y,(short)cast.W,(short)cast.H);
        }
    }

    /// <summary>
    /// CornerAndSize - position is top-left corner. sizes are absoulute size values width and height
    /// </summary>
    [StructLayout(LayoutKind.Explicit,Size = 8)]
    public unsafe struct CornerAndSize
        : IRectangle<CornerAndSize,ulong> {
        public static readonly CornerAndSize Empty = new CornerAndSize();
        [FieldOffset(0)] internal RectangleData Abstract; 
        [FieldOffset(0)] public Point32 Corner;
        [FieldOffset(0)] public short   X;
        [FieldOffset(0)] public short   L;
        [FieldOffset(2)] public short   Y;
        [FieldOffset(2)] public short   T;
        [FieldOffset(4)] public Point32 Sizes;
        [FieldOffset(4)] public short   W;
        [FieldOffset(6)] public short   H;

        public CornerAndSize( int x, int y, int w, int h ) : this() {
            X = (short)x;
            Y = (short)y;
            W = (short)w;
            H = (short)h;
        }
    
        Point32 IRectangleCompounds.Corner { get { return Corner; } set { Corner = value; } }
        Point32 IRectangleCompounds.Sizes { get { return Sizes; } set { Sizes = value; } }

        int IRectangleValues.X { get {return X; } set { X = (short)value; } }
        int IRectangleValues.Y { get {return Y; } set { Y = (short)value; } }
        int IRectangleValues.W { get {return W; } set { W = (short)value; } }
        int IRectangleValues.H { get {return H; } set { H = (short)value; } }
        int IRectangleValues.L { get {return L; } set { L = (short)value; } }
        int IRectangleValues.T { get {return T; } set { T = (short)value; } }
    
    
        public Point32 Center { get{ return Corner + Scale; } set{ Scale = value - Corner; } }
        public Point32 Scale { get{ return Sizes / 2; } set{ Sizes = value * 2;  } }
        public int R { get{ return (short)(X + W); } set{ L = (short)(value - W); } }
        public int B { get{ return (short)(Y + H); } set{ T = (short)(value - H); } }

        public PointPT CompoundA { get{ return new PointPT(ref Corner); } }
        public PointPT CompoundB { get{ return new PointPT(ref Sizes); } }

        public StorageLayout StorageLayout {
            get { return StorageLayout.CornerAndSizes; }
        }

        IRectangle IRectangle.converted<RectangleData>() {
            return Rectangle<CornerAndSize>.Convert<RectangleData>(this);
        }

        CornerAndSize IRectangle<CornerAndSize,ulong>.copied() {
            CornerAndSize clone = Empty;
            clone.Abstract = Abstract;
            return clone;
        }

        RectangleReference<CornerAndSize> IRectangle<CornerAndSize,ulong>.refere() {
            return new CornerAndSizePointers( ref this );
        }

        public IRectangle<CornerAndSize,ulong> casted() {
            return this;
        }

        IRectangle<RType,DSize> IRectangle.cast<RType,DSize>() {
            if( typeof(RType) != typeof(CornerAndSize) )
                return Rectangle<CornerAndSize>.Convert<RType>(this).cast<RType,DSize>();
            else 
                return casted() as IRectangle<RType,DSize>;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle( X, Y, W, H );
        }

        public static CornerAndSize FromRectangle( Rectangle from )
        {
            return new CornerAndSize( from.X, from.Y, from.Width, from.Height );
        }

        CornerAndSize IRectangle<CornerAndSize,ulong>.FromRectangle( Rectangle from )
        {
            return CornerAndSize.FromRectangle( from );
        }

        public void SetRawData( ulong rawdata )
        {
            Abstract.value = rawdata;
        }

        public ulong GetRawData()
        {
            return Abstract.value;
        }

        public bool Contains(IRectangle other)
        {
            return L <= other.L && T <= other.T
                && R >= other.R && B >= other.B;   
        }

        public bool Contains( Point64 point )
        {
            return point.x > L && point.x < R
                && point.y > T && point.y < B; 
        }

        public bool Intersects( IRectangle other )
        {
            return Contains(other.Corner)
                || Contains(other.Center + other.Scale)
                || Contains(other.Center + other.Scale.flypst())
                || Contains(other.Center + other.Scale.flixed());
        }

        public static implicit operator ValueTuple<short,short,short,short>( CornerAndSize cast )
        {
            return (cast.Corner.x,cast.Corner.y,cast.Sizes.x, cast.Sizes.y);
        }
    }

    /// <summary>
    /// AbsoluteEdges - defines position only (for left top right and botom edges - size implies to distance between these) 
    /// </summary>
    [StructLayout(LayoutKind.Explicit,Size = 8)]
    public unsafe struct AbsoluteEdges
        : IRectangle<AbsoluteEdges,ulong> {
        public static readonly AbsoluteEdges Empty = new AbsoluteEdges();
        [FieldOffset(0)] internal RectangleData Abstract; 
        [FieldOffset(0)] public Point32 Corner;
        [FieldOffset(0)] public short L;
        [FieldOffset(2)] public short T;
        [FieldOffset(4)] public short R;
        [FieldOffset(6)] public short B;
    
        public AbsoluteEdges( int l, int r, int t, int b ) : this() {
            L = (short)l;
            R = (short)r;
            T = (short)t;
            B = (short)b;
        }

        Point32 IRectangleCompounds.Corner { get { return  Corner; } set { Corner = value; } }

        int IRectangleValues.L { get { return  L; } set { L = (short)value; } }
        int IRectangleValues.T { get { return  T; } set { T = (short)value; } }
        int IRectangleValues.R { get { return  R; } set { R = (short)value; } }
        int IRectangleValues.B { get { return  B; } set { B = (short)value; } }
    
        
        public Point32 Center { get{ return new Point32((L+R)/2,(T+B)/2); } set{ Point32 m = value - Center; Corner += m; R += m.x; B += m.y; } }
        public Point32 Sizes { get{ return new Point32(W,H); } set{ W = value.x; H = value.y; } } 
        public Point32 Scale { get{ return Center - Corner; } set{ Point32 s = Scale;  value -= s; Corner -= value; R += value.x; B += value.y; } }

        public int X { get { return L; } set { R += (short)(value - L); L = (short)value; } }
        public int Y { get { return T; } set { B += (short)(value - T); T = (short)value; } }
        public int W { get { return (short)(R - L); } set { R = (short)(L + value); } } // set { L = (short)((R = Center.x) - (value/=2)); R += (short)value; } }
        public int H { get { return (short)(B - T); } set { B = (short)(T + value); } } // set { T = (short)((B = Center.y) - (value/=2)); B += (short)value; } }

        public PointPT CompoundA { get{ return new PointPT(ref Abstract.compound1); } } 
        public PointPT CompoundB { get{ return new PointPT(ref Abstract.compound2); } }

        public StorageLayout StorageLayout {
            get { return StorageLayout.AbsoluteEdges; }
        }

        IRectangle IRectangle.converted<RectangleData>()
        {
            return Rectangle<AbsoluteEdges>.Convert<RectangleData>(this);
        }

        public IRectangle<AbsoluteEdges,ulong> casted()
        {
            return this;
        }

        AbsoluteEdges IRectangle<AbsoluteEdges,ulong>.copied()
        {
            AbsoluteEdges clone = Empty;
            clone.Abstract = this.Abstract;
            return clone;
        }

        RectangleReference<AbsoluteEdges> IRectangle<AbsoluteEdges,ulong>.refere()
        {
            return new AbsoluteEdgesPointers( ref this );
        }


        IRectangle<RType,DSize> IRectangle.cast<RType,DSize>()
        {
            if( typeof(RType) != typeof(AbsoluteEdges) )
                return Rectangle<AbsoluteEdges>.Convert<RType>(this).cast<RType,DSize>();
            else 
                return casted() as IRectangle<RType,DSize>;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(X,Y,W,H);
        }

        public static AbsoluteEdges FromRectangle( Rectangle from )
        {
            AbsoluteEdges to = AbsoluteEdges.Empty;
            to.Corner = new Point32(from.Location);
            to.R = (short)from.Right;
            to.B = (short)from.Bottom;
            return to;
        }

        AbsoluteEdges IRectangle<AbsoluteEdges,ulong>.FromRectangle( Rectangle from )
        {
            return AbsoluteEdges.FromRectangle( from );
        }

        public void SetRawData( ulong rawdata )
        {
            Abstract.value = rawdata;
        }

        public ulong GetRawData()
        {
            return Abstract.value;
        }

        public bool Contains(IRectangle other)
        {
            return L <= other.L && T <= other.T
                && R >= other.R && B >= other.B;   
        }

        public bool Contains(Point64 point)
        {
            return point.x > L && point.x < R
                && point.y > T && point.y < B; 
        }

        public bool Intersects( IRectangle other )
        {
            return Contains(other.Corner)
                || Contains(other.Center + other.Scale)
                || Contains(other.Center + other.Scale.flypst())
                || Contains(other.Center + other.Scale.flixed());
        }

        public static implicit operator ValueTuple<short,short,short,short>( AbsoluteEdges cast )
        {
            return (cast.L, cast.R,(short)cast.W,(short)cast.H);
        }
    }

    [StructLayout(LayoutKind.Explicit,Size = 16)]
    public unsafe struct SystemDefault
        : IRectangle<SystemDefault,Rectangle>
    {
        [FieldOffset(0)]
        internal Rectangle data;
        [FieldOffset(0)]
        internal short x;
        [FieldOffset(0)]
        public int X;
        [FieldOffset(4)]
        internal short y;
        [FieldOffset(4)]
        public int Y;
        [FieldOffset(8)]
        internal short w;
        [FieldOffset(8)]
        public int W;
        [FieldOffset(12)]
        internal short h;
        [FieldOffset(12)]
        public int H;

        public SystemDefault( Rectangle from ) : this()
        {
            data = from;
        }
 
        public static implicit operator Rectangle( SystemDefault cast )
        {
            return cast.data;
        }

        public Rectangle Assign( Rectangle rectangle )
        {
            return data = rectangle;
        }

        public int L {
            get { return data.Left; }
            set { int r = R; data.X = value; data.Width = (r - value);
                if( data.Width < 0 ) {
                    data.Width = -data.Width;
                    data.X = r;
                }
            }
        }
        
        public int R {
            get { return data.Right; }
            set { data.Width = value - data.X;
                if( data.Width < 0 ) {
                    int neuX = R;
                    data.Width = -data.Width;
                    data.X = neuX;
                }
            }
        }

        public int T {
            get { return data.Top; }
            set { int b = B; data.Y = value; data.Width = (b - value);
                if( data.Height < 0 ) {
                    data.Height = -data.Height;
                    data.Y = b;
                }
            }
        }

        public int B {
            get { return data.Bottom; }
            set { data.Height = value - data.Y;
                if( data.Height < 0 ) {
                    int neuY = B;
                    data.Width = -data.Width;
                    data.Y = neuY;
                }
            }
        }

        public Point32 Center {
            get { return new Point32(data.X + data.Width / 2, data.Y + data.Height / 2); }
            set { Point32 delta = value - Center;  data.X += delta.x;  data.Y += delta.y; }
        }

        public PointPT CompoundA {
            get { return new PointPT( ref x, ref y ); }
        }

        public PointPT CompoundB {
            get { return new PointPT( ref w, ref h ); }
        }

        public Point32 Corner {
            get { return new Point32(x, y); }
            set { x = value.x; y = value.y; }
        }

        int IRectangleValues.W {
            get { return data.Width; }
            set { data.Width = value; }
        }

        int IRectangleValues.H {
            get { return data.Height; }
            set { data.Height = value; }
        }

        public Point32 Scale {
            get { return new Point32(data.Width / 2, data.Height / 2); }
            set { Point32 center = Center; data.Size = value; Center = center; }
        }

        public Point32 Sizes {
            get { return new Point32(x,y); }
            set { data.Size = value; }
        }

        public StorageLayout StorageLayout {
            get { return StorageLayout.SystemDefault; }
        }

        int IRectangleValues.X {
            get { return data.X; }
            set { data.X = value; }
        }

        int IRectangleValues.Y {
            get { return data.Y; }
            set { data.Y = value; }
        }

        public IRectangle<RType,DSize> cast<RType,DSize>() where RType : struct, IRectangle where DSize : struct
        {
            if( typeof(RType) != typeof(SystemDefault) )
                return Rectangle<SystemDefault>.Convert<RType>(this).cast<RType,DSize>();
            else 
                return casted() as IRectangle<RType,DSize>;
        }

        public IRectangle<SystemDefault,Rectangle> casted()
        {
            return this;
        }

        public bool Contains( Point64 point )
        {
            return data.Contains( point );
        }

        public bool Contains( IRectangle other )
        {
            return data.Contains( other.ToRectangle() );
        }

        public IRectangle converted<RectangleData>() where RectangleData : struct, IRectangle
        {
            return Rectangle<RectangleData>.Create( StorageLayout, data.X, data.Y, data.Width, data.Height );
        }

        public SystemDefault copied()
        {
            return new SystemDefault( data );
        }

        public static SystemDefault FromRectangle( Rectangle from )
        {
            return new SystemDefault( from );
        }

        SystemDefault IRectangle<SystemDefault,Rectangle>.FromRectangle( Rectangle from )
        {
            return SystemDefault.FromRectangle( from );
        }

        public void SetRawData( Rectangle rawdata )
        {
            data = rawdata;
        }

        public Rectangle GetRawData()
        {
            return data;
        }

        public bool Intersects( IRectangle other )
        {
            return data.IntersectsWith( other.ToRectangle() );
        }

        public RectangleReference<SystemDefault> refere()
        {
            return new SystemRectanglePointers(ref x, ref y, ref w, ref h );
        }

        public Rectangle ToRectangle()
        {
            return data;
        }

        public static implicit operator ValueTuple<short,short,short,short>( SystemDefault cast )
        {
            return ((short)cast.X, (short)cast.Y, (short)cast.W, (short)cast.H);
        }
    }
    #endregion

    #region Reference based Rectangles
    // (consisting from 4 pointers, each independantly pointing a distant variable)

    public abstract class RectangleReference<RactanglePointerType>
        : IRectanglePtrs<RactanglePointerType>
    where RactanglePointerType
        : struct, IRectangle
    {
        public IntPtr pA1;
        public IntPtr pA2;
        public IntPtr pB1;
        public IntPtr pB2;

        public RectangleReference()
        {
            pA1 = IntPtr.Zero;
            pA2 = IntPtr.Zero;
            pB1 = IntPtr.Zero;
            pB2 = IntPtr.Zero;
        }

        public RectangleReference( ref Point32 a, ref Point32 b )
        { unsafe {
              fixed ( Point32* p = &a ) {
                pA1 = new IntPtr( &p->x );
                pA2 = new IntPtr( &p->y );
            } fixed ( Point32* p = &b ) {
                pB1 = new IntPtr( &p->x );
                pB2 = new IntPtr( &p->y );
            } }
        }

        public RectangleReference( ref short a1, ref short a2, ref short b1, ref short b2 )
        { unsafe {
              fixed( short* p = &a1 ) {
                pA1 = new IntPtr(p); 
            } fixed( short* p = &a2 ) {
                pA2 = new IntPtr(p);
            } fixed( short* p = &b1 ) {
                pB1 = new IntPtr(p);
            } fixed( short* p = &b2 ) {
                pB2 = new IntPtr(p);
            }
          }
        }

        public RectangleReference( IntPtr a1, IntPtr a2, IntPtr b1, IntPtr b2 )
        {
            pA1 = a1;
            pA2 = a2;
            pB1 = b1;
            pB2 = b2;
        }

        public void RefereCompound( int cN, ref Point32 point )
        { unsafe {
            fixed( Point32* p = &point ) {
                if( cN == 1 ) {
                         pA1 = new IntPtr( &p->x );
                         pA2 = new IntPtr( &p->y );
                } else { pB1 = new IntPtr( &p->x );
                         pB2 = new IntPtr( &p->y );
                }
            }
        } }
        public void RefereCompound( int cN, ref short x, ref short y )
        { unsafe {
              fixed( short* p = &x ) {
                if( cN == 1 ) pA1 = new IntPtr(p);
                         else pB1 = new IntPtr(p);
            } fixed( short* p = &y ) {
                if( cN == 1 ) pA2 = new IntPtr(p);
                         else pB2 = new IntPtr(p);
            }
        } }
        public void RefereValue( int idx, IntPtr val )
        {
            switch( idx ) {
                case 0: pA1 = val; break;
                case 1: pA2 = val; break;
                case 2: pB1 = val; break;
                case 3: pB2 = val; break;
            }
        }

        public abstract int X { get; set; }
        public abstract int Y { get; set; }
        public abstract int W { get; set; }
        public abstract int H { get; set; }
        public abstract int L { get; set; }
        public abstract int R { get; set; }
        public abstract int T { get; set; }
        public abstract int B { get; set; }
        public abstract Point32 Corner { get; set; }
        public abstract Point32 Center { get; set; }
        public abstract Point32 Sizes { get; set; }
        public abstract Point32 Scale { get; set; }
        public abstract PointPT CompoundA { get; }
        public abstract PointPT CompoundB { get; }

        public abstract StorageLayout StorageLayout { get; }

        IntPtr IRectanglePointers.pA1 {
            get { return pA1; }
            set { pA1 = value; }
        }

        IntPtr IRectanglePointers.pA2 {
            get { return pA2; }
            set { pA2 = value; }
        }

        IntPtr IRectanglePointers.pB1 {
            get { return pB1; }
            set { pB1 = value; }
        }

        IntPtr IRectanglePointers.pB2 {
            get { return pB2; }
            set { pB2 = value; }
        }

        public abstract IRectangle converted<RectangleData>()
            where RectangleData
                : struct, IRectangle;

        IRectangle<RType,DSize> IRectangle.cast<RType,DSize>()
        {
            if ( typeof(RType) != typeof(RactanglePointerType) ) {
                return Rectangle<RectangleReference<RactanglePointerType>>.Convert<RType>( this ).cast<RType,DSize>();
            } else {
                return converted<RType>().cast<RType,DSize>();
            }
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle( X, Y, W, H );
        }

        public bool Contains( IRectangle other )
        {
            return L <= other.L && T <= other.T
                && R >= other.R && B >= other.B;   
        }

        public bool Contains( Point64 point )
        {
            return point.x > L && point.x < R
                && point.y > T && point.y < B; 
        }

        public bool Intersects( IRectangle other )
        {
            return Contains( other.Center + other.Scale )
                || Contains( other.Center - other.Scale )
                || Contains( other.Center + other.Scale.flixed() )
                || Contains( other.Center + other.Scale.flypst() );
        }

        public RactanglePointerType resolve()
        {
            return (RactanglePointerType)Rect64.Create<RactanglePointerType>( CompoundA.X, CompoundA.Y, CompoundB.X, CompoundB.Y );
        }

        public IRectanglePtrs<RactanglePointerType> refereTo( ref RactanglePointerType that )
        {
            pA1 = that.CompoundA.pX;
            pA2 = that.CompoundA.pY;
            pB1 = that.CompoundB.pX;
            pB2 = that.CompoundB.pY;
            return this;
        }

        public IRectanglePtrs<RactanglePointerType> casted()
        {
            return this;
        }

        public IRectanglePtrs<RactanglePointerType> copied()
        {
            IRectanglePtrs<RactanglePointerType> clone = null;
            switch( this.StorageLayout ) {
                case StorageLayout.CornerAndSizes: clone = (IRectanglePtrs<RactanglePointerType>)Rectangle<CornerAndSizePointers>.Create(); break;
                case StorageLayout.CenterAndScale: clone = (IRectanglePtrs<RactanglePointerType>)Rectangle<CenterAndScalePointers>.Create(); break;
                case StorageLayout.AbsoluteEdges: clone = (IRectanglePtrs<RactanglePointerType>)Rectangle<AbsoluteEdgesPointers>.Create(); break;
            }
            clone.pA1 = this.pA1;
            clone.pA2 = this.pA2;
            clone.pB1 = this.pB1;
            clone.pB2 = this.pB2;
            return clone;
        }

        public static implicit operator ValueTuple<short,short,short,short>( RectangleReference<RactanglePointerType> cast )
        {
            return ((short)cast.X,(short)cast.Y,(short)cast.W,(short)cast.H);
        }
    }

    public class CornerAndSizePointers : RectangleReference<CornerAndSize>
    {
        public static readonly CornerAndSizePointers Empty = new CornerAndSizePointers();

        public CornerAndSizePointers() : base() {}

        public CornerAndSizePointers(ref Point32 a, ref Point32 b)
            : base(ref a, ref b) {
        }

        public CornerAndSizePointers(ref short x, ref short y, ref short w, ref short h)
            : base(ref x, ref y, ref w, ref h) {
        }

        public CornerAndSizePointers( IntPtr a1, IntPtr a2, IntPtr b1, IntPtr b2 )
            : base(a1,a2,b1,b2) {
        }

        public CornerAndSizePointers( ref CornerAndSize strect )
            : base( ref strect.Abstract.compound1, ref strect.Abstract.compound2 ) {
        }

        override public Point32 Corner {
            get { unsafe { return new Point32( *(short*)pA1.ToPointer(), *(short*)pA2.ToPointer() ); } }
            set { unsafe { *(short*)pA1.ToPointer()=value.x; *(short*)pA2.ToPointer() = value.y; } }
        }
        override public Point32 Sizes {
            get { unsafe { return new Point32( *(short*)pB1.ToPointer(), *(short*)pB2.ToPointer() ); } }
            set { unsafe { *(short*)pB1.ToPointer()=value.x; *(short*)pB2.ToPointer() = value.y; } }
        }
        override public Point32 Center {
            get{ return Corner + Scale; }
            set{ Scale = value - Corner; }
        }
        override public Point32 Scale {
            get{ return Sizes / 2; }
            set{ Sizes = value * 2;  }
        }

        override public int X { get { unsafe{ return *(short*)pA1.ToPointer(); } } set{ unsafe{ *(short*)pA1.ToPointer() = (short)value; } } }
        override public int Y { get { unsafe{ return *(short*)pA2.ToPointer(); } } set{ unsafe{ *(short*)pA2.ToPointer() = (short)value; } } }
        override public int W { get { unsafe{ return *(short*)pB1.ToPointer(); } } set{ unsafe{ *(short*)pB1.ToPointer() = (short)value; } } }
        override public int H { get { unsafe{ return *(short*)pB2.ToPointer(); } } set{ unsafe{ *(short*)pB2.ToPointer() = (short)value; } } }
        override public int L { get { return  X; } set { X = value; } }
        override public int T { get { return  Y; } set { Y = value; } }
        override public int R { get{ return (short)(X + W); } set{ L = (short)(value - W); } }
        override public int B { get{ return (short)(Y + H); } set{ T = (short)(value - H); } }

        public override PointPT CompoundA { get { return new PointPT( pA1, pA2 ); } }

        public override PointPT CompoundB { get { return new PointPT( pB1, pB2 ); } }

        public override StorageLayout StorageLayout {
            get { return StorageLayout.CornerAndSizes; }
        }

        public override IRectangle converted<RectangleData>() {
            return Rectangle<CornerAndSizePointers>.Convert<RectangleData>( this );
        }
    }

    public class CenterAndScalePointers : RectangleReference<CenterAndScale>
    {
        public static readonly CenterAndScalePointers Empty = new CenterAndScalePointers();

        public CenterAndScalePointers() : base() {}

        public CenterAndScalePointers( ref CenterAndScale structangle )
            : base( ref structangle.Abstract.compound1,
                    ref structangle.Abstract.compound2 ) {
        }

        public CenterAndScalePointers( PointPT a, PointPT b )
            : base( a.pX, a.pY, b.pX, b.pY ) {
        }

        public CenterAndScalePointers( ref short a1, ref short a2, ref short b1, ref short b2 )
            : base(ref a1, ref a2, ref b1, ref b2) {
        }

        public CenterAndScalePointers( IntPtr a1, IntPtr a2, IntPtr b1, IntPtr b2 )
            : base( a1, a2, b1, b2 ) {
        }

        override public int X {
            get { unsafe { return (short)(*(short*)pA1.ToPointer() - *(short*)pB1.ToPointer()); } }
            set { value = (short)((value -= X) / 2); unsafe{ *(short*)pA1.ToPointer() += (short)value; *(short*)pB1.ToPointer() += (short)value; } }
        }
        override public int Y {
            get { unsafe { return (short)(*(short*)pA2.ToPointer() - *(short*)pB2.ToPointer()); } }
            set { value = (short)((value -= Y) / 2); unsafe{ *(short*)pA2.ToPointer() += (short)value; *(short*)pB2.ToPointer() += (short)value; } }
        }
        override public int W {
            get { unsafe { return (short)(*(short*)pB1.ToPointer() * 2); } }
            set { unsafe { *(short*)pA1.ToPointer() += (short)((value /= 2)-*(short*)pB1.ToPointer()); *(short*)pB1.ToPointer() = (short)value; } }
        }
        override public int H {
            get { unsafe { return (short)(*(short*)pB2.ToPointer() * 2); } }
            set { unsafe { *(short*)pA2.ToPointer() += (short)((value /= 2)-*(short*)pB2.ToPointer()); *(short*)pB2.ToPointer() = (short)value; } }
        }

        override public int L { get { return  X; } set { X = value; } }
        override public int T { get { return  Y; } set { Y = value; } }

        override public int R {
            get { unsafe { return (short)(*(short*)pA1.ToPointer() + *(short*)pB1.ToPointer()); } } 
            set { value = (short)((value -= R) / 2); unsafe{ *(short*)pA1.ToPointer() += (short)value; *(short*)pB1.ToPointer() += (short)value; } }
        }
        override public int B {
            get { unsafe { return (short)(*(short*)pA2.ToPointer() + *(short*)pB2.ToPointer()); } } 
            set { value = (short)((value -= B) / 2); unsafe{ *(short*)pA2.ToPointer() += (short)value; *(short*)pB2.ToPointer() += (short)value; } }
        }

        override public Point32 Corner {
            get { return Center - Scale; }
            set { Center = value + Scale; }
        }
        override public Point32 Sizes {
            get { return Scale * 2; }
            set { Center = (Center - Scale) + (Scale = (value / 2)); }
        }
        override public Point32 Center { 
            get{ unsafe {return new Point32(*(short*)pA1.ToPointer(),*(short*)pA2.ToPointer()); } }
            set{ unsafe { *(short*)pA1.ToPointer() = value.x; *(short*)pA2.ToPointer() = value.y; } }
        }
        override public Point32 Scale { 
            get{ unsafe {return new Point32(*(short*)pB1.ToPointer(),*(short*)pB2.ToPointer()); } }
            set{ unsafe { *(short*)pB1.ToPointer() = value.x; *(short*)pB2.ToPointer() = value.y; } }
        }

        public override PointPT CompoundA { get{ return new PointPT(pA1,pA2); } }

        public override PointPT CompoundB { get{ return new PointPT(pB1,pB2); } }

        public override StorageLayout StorageLayout {
            get { return StorageLayout.CenterAndScale; }
        }

        public override IRectangle converted<RectangleData>()
        {
            return Rectangle<CenterAndScalePointers>.Convert<RectangleData>( this );
        }
    }

    public class AbsoluteEdgesPointers : RectangleReference<AbsoluteEdges>
    {
        public static readonly AbsoluteEdgesPointers Empty = new AbsoluteEdgesPointers();

        public AbsoluteEdgesPointers() : base() {}

        public AbsoluteEdgesPointers(ref short a1, ref short a2, ref short b1, ref short b2)
            : base(ref a1, ref a2, ref b1, ref b2) {
        }

        public AbsoluteEdgesPointers(ref Point32 LT, ref Point32 RB)
            : base( ref LT, ref RB) {
        }

        public AbsoluteEdgesPointers( ref AbsoluteEdges rect )
            : base( ref rect.Abstract.compound1, ref rect.Abstract.compound2 ) {
        }

        override public int L { get { unsafe{ return *(short*)pA1.ToPointer(); } } set{ unsafe{ *(short*)pA1.ToPointer() = (short)value; } } }
        override public int T { get { unsafe{ return *(short*)pA2.ToPointer(); } } set{ unsafe{ *(short*)pA2.ToPointer() = (short)value; } } }
        override public int R { get { unsafe{ return *(short*)pB1.ToPointer(); } } set{ unsafe{ *(short*)pB1.ToPointer() = (short)value; } } }
        override public int B { get { unsafe{ return *(short*)pB2.ToPointer(); } } set{ unsafe{ *(short*)pB2.ToPointer() = (short)value; } } }
        override public int X { get { return  L; } set { L = value; } }
        override public int Y { get { return  T; } set { T = value; } }
        override public int W { get { return (short)(R - L); } set { L = (short)((R = Center.x) - (value/=2)); R += value; } }
        override public int H { get { return (short)(B - T); } set { T = (short)((B = Center.y) - (value/=2)); B += value; } }

        override public Point32 Corner { get { return new Point32(X, Y); } set { X = value.x; Y = value.y; } }
        override public Point32 Center { get{ return new Point32( (L+R)/2, (T+B)/2 ); } set{ Point32 m = value - Center; Corner += m; R += m.x; B += m.y; } }
        override public Point32 Sizes { get{ return new Point32(W,H); } set{ W = value.x; H = value.y; } } 
        override public Point32 Scale { get{ return Center - Corner; } set{ Point32 s = Scale;  value -= s; Corner -= value; R += value.x; B += value.y; } }

        public override PointPT CompoundA { get{ return new PointPT(pA1,pA2); } }

        public override PointPT CompoundB { get{ return new PointPT(pB1,pB2); } }

        public override StorageLayout StorageLayout {
            get { return StorageLayout.AbsoluteEdges; }
        }

        public override IRectangle converted<RectangleData>() {
             return Rectangle<AbsoluteEdgesPointers>.Convert<RectangleData>( this );
        }
    }

    public class SystemRectanglePointers : RectangleReference<SystemDefault>
    {

        public override int X { get { unsafe { return *(int*)pA1.ToPointer(); } } set { unsafe { *(int*)pA1.ToPointer() = value; } } }
        public override int Y { get { unsafe { return *(int*)pA2.ToPointer(); } } set { unsafe { *(int*)pA2.ToPointer() = value; } } }
        public override int W { get { unsafe { return *(int*)pB1.ToPointer(); } } set { unsafe { *(int*)pB1.ToPointer() = value; } } }
        public override int H { get { unsafe { return *(int*)pB2.ToPointer(); } } set { unsafe { *(int*)pB2.ToPointer() = value; } } }
        public override int L { get { return X; } set { W -= ( value - X ); X = value; } }
        public override int R { get { return X + W; } set { W = value - X; } }
        public override int T { get { return Y; } set { H -= ( value - Y ); Y = value; } }
        public override int B { get { return Y + H; } set { H = value - Y; } }
        public override Point32 Corner { get { return new Point32(X,Y); } set { X = value.X; Y = value.Y; } }
        public override Point32 Center { get { return Corner + ( Sizes / 2 ); } set { Corner = value - ( Sizes / 2 ); } }
        public override Point32 Sizes { get { return new Point32(W,H); } set { W = value.X; H = value.Y; } }
        public override Point32 Scale { get { return Sizes / 2; } set { Sizes = value * 2; } }

        public override PointPT CompoundA { get { return new PointPT(pA1,pA2); } }

        public override PointPT CompoundB { get { return new PointPT(pB1,pB2); } }

        public override StorageLayout StorageLayout { get { return StorageLayout.CornerAndSizes; } }

        public SystemRectanglePointers()
            : base() {
        }

        public SystemRectanglePointers( ref short x, ref short y, ref short w, ref short h )
            : base(ref x, ref y, ref w, ref h) {
        }

        public SystemRectanglePointers( ref Point32 position, ref Point32 size )
            : base( ref position, ref size ) {
        }

        public SystemRectanglePointers( ref SystemDefault rect )
            : base( ref rect.x, ref rect.y, ref rect.w, ref rect.h ) {
        }

        public override IRectangle converted<RectangleData>()
        {
            RectangleData d = new RectangleData();
            d.Corner = Corner;
            d.Sizes = Sizes;
            return d;
        }
    }

    #endregion

    #region Static Helper classes
    // for helping with Construction and with Conversion between diferent storage models, etc...  

    public class Rect64
    {    
        public static IRectangle Create<Layout>( Point a, Point b )
            where Layout : IRectangle // struct, IRectangleValues, IRectangleCompounds
        {
            if( typeof(Layout) == typeof(CornerAndSize) ) {
                CornerAndSize create = CornerAndSize.Empty;
                create.Corner = new Point32(a);
                create.Sizes = new Point32(b);
                return create as IRectangle;
            } else if( typeof(Layout) == typeof(CenterAndScale) ) {
                CenterAndScale create = CenterAndScale.Empty;
                create.Corner = new Point32(a);
                create.Sizes = new Point32(b);
                return create as IRectangle;
            } else if( typeof(Layout) == typeof(AbsoluteEdges) ) {
                AbsoluteEdges create = AbsoluteEdges.Empty;
                create.Corner = new Point32(a);
                create.R = (short)b.X;
                create.B = (short)b.Y;
                return create as IRectangle;
            } return null;
        }
        /// <summary> RectangleType.Create.LayoutType() 
        /// Create a rectangle instance of type RectangleType from given values a1,a2,b1,b2
        /// where these values are interpreted being meand 'LayoutType'. 
        /// example:
        /// if 'LayoutType' parameter is 'AbsoluteEdges' then the a1 and a2 parameters are 
        /// assumed being 'Left' and 'Right' edges of that rectange which to create, and
        /// b1 and b2 parameters are assumed being positions for 'Top' and 'Bottom' edges
        ///  - regardless what kind of storage model the creted rectangle may use - 
        /// parameters will be converted so that instance to be created will have L/R edges
        /// posisioned at a1/a2 along X axis and T/B edges possitioned at b1/b2 along Y axis  
        /// </summary>   
        public static IRectangle Create<Layout>( int a1, int a2, int b1, int b2 )
            where Layout : IRectangle
        {
            if( typeof(Layout) == typeof(CornerAndSize) ) {
                CornerAndSize create = CornerAndSize.Empty;
                create.Corner = new Point32(a1,a2);
                create.Sizes = new Point32(b1,b2);
                return create as IRectangle;
            } else if( typeof(Layout) == typeof(CenterAndScale) ) {
                CenterAndScale create = CenterAndScale.Empty;
                create.Corner = new Point32(a1,a2);
                create.Sizes = new Point32(b1,b2);
                return create as IRectangle;
            } else if( typeof(Layout) == typeof(AbsoluteEdges) ) {
                AbsoluteEdges create = AbsoluteEdges.Empty;
                create.Corner = new Point32(a1,a2);
                create.R = (short)b1;
                create.B = (short)b2;
                return create as IRectangle;
            } return null;
        }

        public static RectangleReference<Layout> Refere<Layout>( ref IRectangle<Layout,ulong> rect )
            where Layout : struct, IRectangle {
            return rect.refere();
        }

        public static IRectangle Refere<Layout>( IntPtr rect )
        { unsafe{
            Type lt = typeof(Layout);
                   if( lt == typeof(CornerAndSize) ) {
                return new CornerAndSizePointers(ref *(CornerAndSize*)rect.ToPointer());
            } else if( lt == typeof(CenterAndScale) ) {
                 return new CenterAndScalePointers(ref *(CenterAndScale*)rect.ToPointer());
            } else if( lt == typeof(AbsoluteEdges) ) {
                 return new AbsoluteEdgesPointers(ref *(AbsoluteEdges*)rect.ToPointer());
            } return null;
        } }
        public static IRectangle Refere<Layout>( IntPtr pA, IntPtr pB )
        { unsafe {
            Type lt = typeof(Layout);
                   if( lt == typeof(CornerAndSize) ) {
                return new CornerAndSizePointers( ref *(Point32*)pA.ToPointer(),
                                                  ref *(Point32*)pB.ToPointer() );
            } else if( lt == typeof(CenterAndScale) ) {
                 return new CenterAndScalePointers( new PointPT(ref *(Point32*)pA.ToPointer() ),
                                                    new PointPT(ref *(Point32*)pB.ToPointer() ) );
            } else if( lt == typeof(AbsoluteEdges) ) {
                 return new AbsoluteEdgesPointers( ref *(Point32*)pA.ToPointer(),
                                                   ref *(Point32*)pB.ToPointer() );
            } return null;
        } }
        public static IRectangle Refere<Layout>( IntPtr a1, IntPtr a2, IntPtr b1, IntPtr b2 )
        { unsafe {
            Type lt = typeof(Layout);
                   if( lt == typeof(CornerAndSize) ) {
                return new CornerAndSizePointers(
                   ref *(short*)a1.ToPointer(),
                   ref *(short*)a2.ToPointer(),
                   ref *(short*)b1.ToPointer(),
                   ref *(short*)b2.ToPointer()
                );
            } else if( lt == typeof(CenterAndScale) ) {
                return new CenterAndScalePointers(
                   ref *(short*)a1.ToPointer(),
                   ref *(short*)a2.ToPointer(),
                   ref *(short*)b1.ToPointer(),
                   ref *(short*)b2.ToPointer()
                );
            } else if( lt == typeof(AbsoluteEdges) ) {
                return new AbsoluteEdgesPointers(
                   ref *(short*)a1.ToPointer(),
                   ref *(short*)a2.ToPointer(),
                   ref *(short*)b1.ToPointer(),
                   ref *(short*)b2.ToPointer()
                );
            } return null;
        } }
    }

    public class Rectangle<rType> : Rect64 where rType : IRectangle
    {
        public const uint LRTB = (uint)StorageLayout.AbsoluteEdges;
        public const uint XYWH = (uint)StorageLayout.CornerAndSizes;
        public const uint CPSC = (uint)StorageLayout.CenterAndScale;

        public static IRectangle Convert<Layout>( rType from )
            where Layout : struct, IRectangleValues, IRectangleCompounds
        {
            Type layoutType = typeof(Layout);
                   if( layoutType == typeof( CornerAndSize ) ) {
                return Create( StorageLayout.CornerAndSizes, from.X, from.Y, from.W, from.H ); 
            } else if( layoutType == typeof( CenterAndScale ) ) {
                return Create( StorageLayout.CenterAndScale, from.Center.x, from.Center.y, from.Scale.x, from.Scale.y ); 
            } else if( layoutType == typeof( AbsoluteEdges ) ) {
                return Create( StorageLayout.AbsoluteEdges, from.L, from.R, from.T, from.B ); 
            } return null;
        }

        public static IRectangle Refere( ref rType databased )
        {
            switch( databased.StorageLayout ) {
                case StorageLayout.CornerAndSizes: {
                    return Refere<CornerAndSize>(
                        databased.CompoundA.pX, databased.CompoundA.pY,
                        databased.CompoundB.pX, databased.CompoundB.pY
                    );
                }
                case StorageLayout.CenterAndScale: {
                    return Refere<CenterAndScale>(
                        databased.CompoundA.pX, databased.CompoundA.pY,
                        databased.CompoundB.pX, databased.CompoundB.pY
                    );
                }
                case StorageLayout.AbsoluteEdges: {
                    return Refere<AbsoluteEdges>(
                        databased.CompoundA.pX, databased.CompoundA.pY,
                        databased.CompoundB.pX, databased.CompoundB.pY
                    );
                }
            default: return null; }
        }

        public static IRectangle FromRectangle( Rectangle from )
        {
            if( !typeof(rType).BaseType.IsAbstract ) {
                return Create( StorageLayout.CornerAndSizes, from.X, from.Y, from.Width, from.Height ); 
            } else {
                throw new InvalidOperationException("cannot create references to abstract rectangle types");
            }
        }

        public static IRectangle Create()
        {
                   Type rType =  typeof(rType);
                   if ( rType == typeof(CenterAndScale) ) {
                return CenterAndScale.Empty;
            } else if ( rType == typeof(CornerAndSize) ) {
                return CornerAndSize.Empty;
            } else if ( rType == typeof(AbsoluteEdges) ) {
                return AbsoluteEdges.Empty;
            } else if ( rType == typeof(CenterAndScalePointers) ) {
                return CenterAndScalePointers.Empty;
            } else if ( rType == typeof(CornerAndSizePointers) ) {
                return CornerAndSizePointers.Empty;
            } else if ( rType == typeof(AbsoluteEdgesPointers) ) {
                return AbsoluteEdgesPointers.Empty;
            } else {
                return new SystemDefault( Rectangle.Empty );
            }
        }

        public static IRectangle Create( int a1, int a2, int b1, int b2 )
        {
            return Create<rType>( a1, a2, b1, b2 );
        }

        public static IRectangle Create( Point p1, Point p2 )
        {
            return Create<rType>( p1, p2 );
        }

        public static IRectangle Create( Point32 p1, Point32 p2 )
        {
            return Create<rType>( p1.x, p1.y, p2.x, p2.y );
        }

        public static IRectangle Create( StorageLayout descriptor, ulong from4ShortsRawData )
        {
            unsafe { switch( descriptor ) {
                case StorageLayout.CenterAndScale: return *(CenterAndScale*)&from4ShortsRawData;
                case StorageLayout.CornerAndSizes: return *(CornerAndSize*)&from4ShortsRawData;
                case StorageLayout.AbsoluteEdges: return *(AbsoluteEdges*)&from4ShortsRawData;
                default: {
                     IRectangle r = *(CornerAndSize*)&from4ShortsRawData;
                   return r.converted<SystemDefault>();
                }
            } }
        }

        public static IRectangle Create( StorageLayout parameterDescriptor, ValueTuple<short,short,short,short> parameterContainer )
        {
            IRectangle create = Rectangle<rType>.Create();
            switch( parameterDescriptor ) {
                case StorageLayout.CenterAndScale: {
                        create.Center = new Point32(parameterContainer.Item1,parameterContainer.Item2);
                        create.Scale = new Point32( parameterContainer.Item3,parameterContainer.Item4);
                        return create;
                    }
                case StorageLayout.SystemDefault:
                case StorageLayout.CornerAndSizes: {
                        create.Center = new Point32( parameterContainer.Item1 + parameterContainer.Item3 / 2
                                                   , parameterContainer.Item2 + parameterContainer.Item4 / 2 );
                        create.Scale = new Point32( parameterContainer.Item3 / 2, parameterContainer.Item4 / 2 );
                        return create;
                    }
                case StorageLayout.AbsoluteEdges: {
                        create.L = parameterContainer.Item1;
                        create.R = parameterContainer.Item2;
                        create.T = parameterContainer.Item3;
                        create.B = parameterContainer.Item4;
                        return create;
                    }
                default:
                return create;
            }
        }

        public static IRectangle Create( StorageLayout fromParameters, int val1, int val2, int val3, int val4 )
        {
            IRectangle create = Rectangle<rType>.Create();
            switch( fromParameters ) {
                case StorageLayout.CenterAndScale: {
                       create.Center = new Point32(val1,val2);
                       create.Scale  = new Point32(val3,val4);
                return create; }
                case StorageLayout.SystemDefault:
                case StorageLayout.CornerAndSizes: {
                       create.Center = new Point32( val1+val3/2, val2+val4/2 );
                       create.Scale = new Point32( val3/2, val4/2 );
                return create; }
                case StorageLayout.AbsoluteEdges: {
                       create.L = (short)val1;
                       create.R = (short)val2;
                       create.T = (short)val3;
                       create.B = (short)val4;
                return create; }
                default:
                return create;
            }
        }
    }

    #endregion
}
