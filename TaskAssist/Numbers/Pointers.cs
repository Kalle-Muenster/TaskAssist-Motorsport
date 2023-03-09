using System;
using System.Collections.Generic;

namespace Stepflow.Numbers.Pointers
{
    public class PointerList
    {
        public List<IntPtr> Pointers;

        public PointerList()
        {
            Pointers = new List<IntPtr>();
        }

        public byte this[byte idx] {
            get { unsafe { return *(byte*)Pointers[idx].ToPointer(); } }
            set { unsafe { *(byte*)Pointers[idx].ToPointer() = value; } }
        }
        public sbyte this[sbyte idx] {
            get { unsafe { return *(sbyte*)Pointers[idx].ToPointer(); } }
            set { unsafe { *(sbyte*)Pointers[idx].ToPointer() = value; } }
        }
        public ushort this[ushort idx] {
            get { unsafe { return *(ushort*)Pointers[idx].ToPointer(); } }
            set { unsafe { *(ushort*)Pointers[idx].ToPointer() = value; } }
        }
        public short this[short idx] {
            get { unsafe { return *(short*)Pointers[idx].ToPointer(); } }
            set { unsafe { *(short*)Pointers[idx].ToPointer() = value; } }
        }
        public Int24 this[Int24 idx] {
            get { unsafe { return *(Int24*)Pointers[idx].ToPointer(); } }
            set { unsafe { *(Int24*)Pointers[idx].ToPointer() = value; } }
        }
        public UInt24 this[UInt24 idx] {
            get { unsafe { return *(UInt24*)Pointers[(int)(uint)idx].ToPointer(); } }
            set { unsafe { *(UInt24*)Pointers[(int)(uint)idx].ToPointer() = value; } }
        }
        public uint this[uint idx] {
            get { unsafe { return *(uint*)Pointers[(int)idx].ToPointer(); } }
            set { unsafe { *(uint*)Pointers[(int)idx].ToPointer() = value; } }
        }
        public int this[int idx] {
            get { unsafe { return *(int*)Pointers[idx].ToPointer(); } }
            set { unsafe { *(int*)Pointers[idx].ToPointer() = value; } }
        }
        public ulong this[ulong idx] {
            get { unsafe { return *(ulong*)Pointers[(int)idx].ToPointer(); } }
            set { unsafe { *(ulong*)Pointers[(int)idx].ToPointer() = value; } }
        }
        public long this[long idx] {
            get { unsafe { return *(long*)Pointers[(int)idx].ToPointer(); } }
            set { unsafe { *(long*)Pointers[(int)idx].ToPointer() = value; } }
        }
        public Float16 this[Float16 idx] {
            get { unsafe { return *(Float16*)Pointers[(int)idx].ToPointer(); } }
            set { unsafe { *(Float16*)Pointers[(int)idx].ToPointer() = value; } }
        }
        public float this[float idx] {
            get { unsafe { return *(float*)Pointers[(int)idx].ToPointer(); } }
            set { unsafe { *(float*)Pointers[(int)idx].ToPointer() = value; } }
        }
        public double this[double idx] {
            get { unsafe { return *(double*)Pointers[(int)idx].ToPointer(); } }
            set { unsafe { *(double*)Pointers[(int)idx].ToPointer() = value; } }
        }
    } 

    public class PointerPatch
    {
        private IntPtr[] Pointer;
         
        public PointerPatch( int size )
        {
            Pointer = new IntPtr[size];
        }

        public void SetPointer( int idx, IntPtr value )
        {
            Pointer[idx] = value;
        }

        public IntPtr GetPointer( int idx )
        {
            return Pointer[idx];
        }

        public Byte this[Byte idx] {
            get { unsafe { return *(Byte*)Pointer[idx].ToPointer(); } }
            set { unsafe { *(Byte*)Pointer[idx].ToPointer() = value; } }
        }
        public SByte this[SByte idx] {
            get { unsafe { return *(SByte*)Pointer[idx].ToPointer(); } }
            set { unsafe { *(SByte*)Pointer[idx].ToPointer() = value; } }
        }
        public UInt16 this[UInt16 idx] {
            get { unsafe { return *(UInt16*)Pointer[idx].ToPointer(); } }
            set { unsafe { *(UInt16*)Pointer[idx].ToPointer() = value; } }
        }
        public Int16 this[Int16 idx] {
            get { unsafe { return *(Int16*)Pointer[idx].ToPointer(); } }
            set { unsafe { *(Int16*)Pointer[idx].ToPointer() = value; } }
        }
        public Int24 this[Int24 idx] {
            get { unsafe { return *(Int24*)Pointer[idx].ToPointer(); } }
            set { unsafe { *(Int24*)Pointer[idx].ToPointer() = value; } }
        }
        public UInt24 this[UInt24 idx] {
            get { unsafe { return *(UInt24*)Pointer[idx].ToPointer(); } }
            set { unsafe { *(UInt24*)Pointer[idx].ToPointer() = value; } }
        }
        public UInt32 this[UInt32 idx] {
            get { unsafe { return *(UInt32*)Pointer[idx].ToPointer(); } }
            set { unsafe { *(UInt32*)Pointer[idx].ToPointer() = value; } }
        }
        public Int32 this[Int32 idx] {
            get { unsafe { return *(Int32*)Pointer[idx].ToPointer(); } }
            set { unsafe { *(Int32*)Pointer[idx].ToPointer() = value; } }
        }
        public UInt64 this[UInt64 idx] {
            get { unsafe { return *(UInt64*)Pointer[idx].ToPointer(); } }
            set { unsafe { *(UInt64*)Pointer[idx].ToPointer() = value; } }
        }
        public Int64 this[Int64 idx] {
            get { unsafe { return *(Int64*)Pointer[idx].ToPointer(); } }
            set { unsafe { *(Int64*)Pointer[idx].ToPointer() = value; } }
        }
        public Float16 this[Float16 idx] {
            get { unsafe { return *(Float16*)Pointer[(int)idx].ToPointer(); } }
            set { unsafe { *(Float16*)Pointer[(int)idx].ToPointer() = value; } }
        }
        public Single this[Single idx] {
            get { unsafe { return *(Single*)Pointer[(int)idx].ToPointer(); } }
            set { unsafe { *(Single*)Pointer[(int)idx].ToPointer() = value; } }
        }
        public Double this[Double idx] {
            get { unsafe { return *(Double*)Pointer[(int)idx].ToPointer(); } }
            set { unsafe { *(Double*)Pointer[(int)idx].ToPointer() = value; } }
        }
    }

    public class PinAccessor 
    {
        private ControllerBase  c;

        public PinAccessor(ControllerBase controller)
        {
            c = controller;
        }
        public void Set(ControllerBase controlledvalue)
        {
            c = controlledvalue;
        }
        public CT Get<CT>() where CT : ControllerBase
        {
            return (CT)c;
        }
        public byte this[byte idx] {
            get { unsafe { return *(byte*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(byte*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public sbyte this[sbyte idx] {
            get { unsafe { return *(sbyte*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(sbyte*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public ushort this[ushort idx] {
            get { unsafe { return *(ushort*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(ushort*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public short this[short idx] {
            get { unsafe { return *(short*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(short*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public Int24 this[Int24 idx]
        {
            get { unsafe { return *(Int24*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(Int24*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public UInt24 this[UInt24 idx]
        {
            get { unsafe { return *(UInt24*)c.GetPin((int)(uint)idx).ToPointer(); } }
            set { unsafe { *(UInt24*)c.GetPin((int)(uint)idx).ToPointer() = value; } }
        }
        public uint this[uint idx] {
            get { unsafe { return *(uint*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(uint*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public int this[int idx] {
            get { unsafe { return *(int*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(int*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public ulong this[ulong idx] {
            get { unsafe { return *(ulong*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(ulong*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public long this[long idx] {
            get { unsafe { return *(long*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(long*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public Float16 this[Float16 idx] {
            get { unsafe { return *(Float16*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(Float16*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public float this[float idx] {
            get { unsafe { return *(float*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(float*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public double this[double idx] {
            get { unsafe { return *(double*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(double*)c.GetPin((int)idx).ToPointer() = value; } }
        }
    }
}
