using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Stepflow.Numbers.Conversions
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
    public struct ReInterpret
    {
        [FieldOffset(0)] public Float16 AsFloat16;
        [FieldOffset(0)] public float   AsFloat32;
        [FieldOffset(0)] public double  AsFloat64;
        [FieldOffset(0)] public long   AsSigned64;
        [FieldOffset(0)] public int    AsSigned32;
        [FieldOffset(0)] public uint   UnSigned32;
        [FieldOffset(0)] public Int24  AsSigned24;
        [FieldOffset(0)] public short  AsSigned16;
        [FieldOffset(0)] public ulong  UnSigned64;
        [FieldOffset(0)] internal unsafe fixed byte bytes[8];

        public bool IsNegative {
            get { unsafe {
                bool isneg = false;
                fixed( void* ptr = bytes ) {
                    sbyte* chrs = (sbyte*)ptr;
                    if( chrs[7] < 0 ) return true;
                    for( int i = 7; i >= 0; --i ) {
                        if( chrs[i] != 0 ) {
                            isneg = chrs[i] < 0;
                            break;
                        }
                    }
                } return isneg; }
            }
        }

        public new string ToString {
            get { unsafe { fixed( void* ptr = bytes )
                    return new String( (sbyte*)ptr, 0, 8 ).Trim();
                }
            }
        }

        public byte[] ToArray() { unsafe { byte[] cast = new byte[8];
            for( int i = 0; i < 8; ++i ) cast[i] = bytes[i];
                return cast;
            }
        }

        public ReInterpret( byte[] data ) : this()
        {
            if( data == null ) UnSigned64 = 0;
            else if( data.Length > 8 ) AsSigned64 = -1;
            else unsafe { fixed( byte* ptr = bytes ) { 
                for( int i = 0; i < data.Length; ++i ) 
                    ptr[i] = data[i];
                }
            }
        }

        public ReInterpret( ArraySegment<byte> data ) : this()
        {
            if( data.Array == null ) UnSigned64 = 0;
            if( data.Count == 0 ) UnSigned64 = 0;
            else unsafe { fixed( byte* ptr = bytes ) {
                for( int i = 0, I = data.Offset; i < data.Count; ++i, ++I )
                     ptr[i] = data.Array[I];
                }
            }
        }

        public ReInterpret( short value ) : this() { AsSigned16 = value; }
        public ReInterpret( int value ) : this() { AsSigned32 = value; }
        public ReInterpret( uint value ) : this() { UnSigned32 = value; }
        public ReInterpret( Int24 value ) : this() { AsSigned24 = value; }
        public ReInterpret( long value ) : this() { AsSigned64 = value; }
        public ReInterpret( Float16 value ) : this() { AsFloat16 = value; }
        public ReInterpret( float value ) : this() { AsFloat32 = value; }
        public ReInterpret( double value ) : this() { AsFloat64 = value; }
        public ReInterpret( ulong value ) : this() { UnSigned64 = value; }
        public ReInterpret( string data ) : this( Encoding.Default.GetBytes(data) ) { }

        public static ReInterpret Cast( short value )
        {
            return new ReInterpret(value);
        }
        public static ReInterpret Cast( int value )
        {
            return new ReInterpret(value);
        }
        public static ReInterpret Cast( Int24 value )
        {
            return new ReInterpret(value);
        }
        public static ReInterpret Cast( long value )
        {
            return new ReInterpret(value);
        }
        public static ReInterpret Cast( ulong value )
        {
            return new ReInterpret(value);
        }
        public static ReInterpret Cast( float value )
        {
            return new ReInterpret(value);
        }
        public static ReInterpret Cast( double value )
        {
            return new ReInterpret(value);
        }
        public static ReInterpret Cast( string data )
        {
            return new ReInterpret(data);
        }
        public static ReInterpret Cast( Float16 data )
        {
            return new ReInterpret(data);
        }
        public static ReInterpret Cast( uint data )
        {
            return new ReInterpret(data);
        }
        public static ReInterpret Cast( byte[] data )
        {
            return new ReInterpret(data);
        }
        public static ReInterpret Cast( ArraySegment<byte> data )
        {
            return new ReInterpret(data);
        }

        public static ReInterpret[] Array( byte[] data )
        {
            unsafe {
                int s = (data.Length + (( data.Length + 8 ) % 8)) / 8;
                ReInterpret[] ret = new ReInterpret[s];
                for( int i = 0; i < s; ++i ) {
                    ret[i].UnSigned64 = Cast( new ArraySegment<byte>( data, i*8, 8 ) ).UnSigned64;
                } return ret;
            } 
        }

        public T As<T>( int index ) where T : struct
        {
            unsafe { fixed( void* d = bytes ) return *( (T*)d + index ); }
        }

        public byte this[byte index] {
            get { unsafe { return bytes[index]; } }
            set { unsafe { bytes[index] = value; } }
        }

        public short this[short index] {
            get { unsafe { fixed( void* d = bytes ) return *( (short*)d + index ); } }
            set { unsafe { fixed( byte* d = bytes ) *( (short*)d + index ) = value; } }
        }
        public Float16 this[Float16 index] {
            get { unsafe { fixed( void* d = bytes ) return *((Float16*)d + (int)index); } }
            set { unsafe { fixed( byte* d = bytes ) *((Float16*)d + (int)index) = value; } }
        }

        public int this[int index] {
            get { unsafe { fixed( void* d = bytes ) return *( (int*)d + index ); } }
            set { unsafe { fixed( byte* d = bytes ) *( (int*)d + index ) = value; } }
        }
        public float this[float index] {
            get { unsafe { fixed( void* d = bytes ) return *( (float*)d + (int)index ); } }
            set { unsafe { fixed( byte* d = bytes ) *( (float*)d + (int)index ) = value; } }
        }
    }

    public static class NumericValue
    {
        public static readonly char DecimalSeparator =
            System.Globalization.CultureInfo.CurrentCulture
           .NumberFormat.CurrencyDecimalSeparator[0];

        public static byte[] GetBytes( object any )
        {
            byte[] data = Array.Empty<byte>();
            if( any == null ) return data;
            if( any is byte[] ) return any as byte[];
            ReInterpret converse = new ReInterpret( 0 );
            if( any is Enum ) unsafe {
                converse.UnSigned64 = Convert.ToUInt64( any as Enum );
            } else {
                string rep = any is string ? any as string : string.Format("{0}",any);
                if( rep.Contains('.') || rep.Contains(',') ) {
                    rep.Replace('.', DecimalSeparator);
                    rep.Replace(',', DecimalSeparator);
                    if( double.TryParse(rep, out converse.AsFloat64) )
                        if( converse.AsFloat64 == 0.0 ) return new byte[8];
                } else {
                    if( long.TryParse(rep, out converse.AsSigned64) )
                        if( converse.AsSigned64 == 0 ) return new byte[4];
                }
            } bool seemsNegative = converse.IsNegative;
            for( int i = 7; i >= 0; --i ) unsafe {
                    if( seemsNegative ? ( converse.bytes[i] < 255 )
                                      : ( converse.bytes[i] > 0 ) ) {
                        data = new byte[i + 1]; break;
                    }
                }
            for( int i = 0; i < data.Length; ++i ) unsafe {
                    data[i] = converse.bytes[i];
                }
            return data;
        }

        public static long GetInteger( byte[] data )
        {
            if( data == null ) { return (long)Convert.DBNull; }
            switch( data.Length ) {
                case 0: return (long)Convert.DBNull;
                case 1: return ReInterpret.Cast(data).As<sbyte>(0);
                case 2: return ReInterpret.Cast(data).AsSigned16;
                case 3: return ReInterpret.Cast(data).AsSigned24;
                case 4: return ReInterpret.Cast(data).AsSigned32;
                default: return ReInterpret.Cast(data).AsSigned64;
            }
        }

        public static long GetInteger( ArraySegment<byte> data )
        {
            if( data.Array == null ) return (long)Convert.DBNull;
            switch( data.Count ) {
                case 0: return (long)Convert.DBNull;
                case 1: return ReInterpret.Cast(data).As<sbyte>(0);
                case 2: return ReInterpret.Cast(data).AsSigned16;
                case 3: return ReInterpret.Cast(data).AsSigned24;
                case 4: return ReInterpret.Cast(data).AsSigned32;
                default: return ReInterpret.Cast(data).AsSigned64;
            }
        }

        public static ulong GetUnsigned( byte[] data )
        {
            if( data == null ) { return (ulong)Convert.DBNull; }
            switch( data.Length ) {
                case 0: return (UInt64)Convert.DBNull;
                case 1: return data[0];
                case 2: return (UInt16)ReInterpret.Cast(data).AsSigned24;
                case 3: return (UInt24)ReInterpret.Cast(data).AsSigned32;
                case 4: return (UInt32)ReInterpret.Cast(data).AsSigned64;
                default: return ReInterpret.Cast(data).UnSigned64;
            }
        }

        public static ulong GetUnsigned( ArraySegment<byte> data )
        {
            if( data.Array == null ) return (ulong)Convert.DBNull;
            switch( data.Count ) {
                case 0: return (UInt64)Convert.DBNull;
                case 1: return data.Array[data.Offset];
                case 2: return (UInt16)ReInterpret.Cast(data).AsSigned24;
                case 3: return (UInt24)ReInterpret.Cast(data).AsSigned32;
                case 4: return (UInt32)ReInterpret.Cast(data).AsSigned64;
                default: return ReInterpret.Cast(data).UnSigned64;
            }
        }

        public static double GetDecimal( byte[] data )
        {
            if( data == null ) { return (double)Convert.DBNull; }
            switch( data.Length ) {
                case 0: return (double)Convert.DBNull;
                case 1: return (double)data[0];
                case 2: return (double)ReInterpret.Cast(data).AsFloat16;
                case 3: return (double)ReInterpret.Cast(data).AsSigned24;
                case 4: return (double)ReInterpret.Cast(data).AsFloat32;
                default: return ReInterpret.Cast(data).AsFloat64;
            }
        }

        public static double GetDecimal( ArraySegment<byte> data )
        {
            if( data.Array == null ) return (double)Convert.DBNull;
            switch( data.Count ) {
                case 0: return (double)Convert.DBNull;
                case 1: return (double)data.Array[data.Offset];
                case 2: return (double)ReInterpret.Cast(data).AsFloat16;
                case 3: return (double)ReInterpret.Cast(data).AsSigned24;
                case 4: return (double)ReInterpret.Cast(data).AsFloat32;
                default: return ReInterpret.Cast(data).AsFloat64;
            }
        }
    }

    public static class ValuesExtension
    {
        public static Int32 ToInt32( this Enum value )
        {
            return Convert.ToInt32( value );
        }

        public static UInt32 ToUInt32( this Enum value )
        {
            return Convert.ToUInt32( value );
        }

        public static UInt64 ToUInt64( this Enum value )
        {
            return Convert.ToUInt64( value );
        }

        public static Int32 ToFourCC( this string fourCC )
        {
            return ReInterpret.Cast( fourCC ).AsSigned32;
        }

        public static Int64 ToLongCC( this string longCC )
        {
            return ReInterpret.Cast( longCC ).AsSigned64;
        }

        public static string AsString( this Int32 fourCC ) 
        {
            return ReInterpret.Cast( fourCC ).ToString;
        }

        public static string AsString( this Int64 longCC )
        {
            return ReInterpret.Cast( longCC ).ToString;
        }
    }
}
