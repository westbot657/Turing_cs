using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Turing.Interop.Wrappers;

namespace Turing.Interop.Parameters
{

    [StructLayout(LayoutKind.Sequential)]
    public struct RsParams
    {
        public uint param_count;
        public IntPtr params_array;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RsParam
    {
        public uint type;
        public IntPtr value;
    }

    public enum ParamType
    {
        I8      = 0,
        I16     = 1,
        I32     = 2,
        I64     = 3,
        U8      = 4,
        U16     = 5,
        U32     = 6,
        U64     = 7,
        F32     = 8,
        F64     = 9,
        Boolean = 10,
        String  = 11,

        ColorNote     = 100,
        BombNote      = 101,
        Arc           = 102,
        ChainHeadNote = 103,
        ChainLinkNote = 104,
        ChainNote     = 105,
        Wall          = 106,
        Saber         = 107,
        Player        = 108,
        
        Vec3 = 200,
        Quat = 201,
        
        Unknown       = -1,
    }
    
    public class Parameters
    {
        
        private static readonly Dictionary<Type, ParamType> ParamTypes = new Dictionary<Type, ParamType>()
        {
            { typeof(sbyte), ParamType.I8 },
            { typeof(short), ParamType.I16 },
            { typeof(int), ParamType.I32 },
            { typeof(long), ParamType.I64 },

            { typeof(byte), ParamType.U8 },
            { typeof(ushort), ParamType.U16 },
            { typeof(uint), ParamType.U32 },
            { typeof(ulong), ParamType.U64 },

            { typeof(float), ParamType.F32 },
            { typeof(double), ParamType.F64 },

            { typeof(bool), ParamType.Boolean },
            { typeof(RsString), ParamType.String },

            { typeof(ColorNoteRs), ParamType.ColorNote },
            
            { typeof(Vector3), ParamType.Vec3 },
            { typeof(Quaternion), ParamType.Quat },
            // Add more custom types here as needed
        };

        private static ParamType GetParamType(Type tp)
        {
            if (ParamTypes.TryGetValue(tp, out var paramType))
                return paramType;

            var baseType = tp.BaseType;
            if (baseType != null)
            {
                var baseParamType = GetParamType(baseType);
                ParamTypes[tp] = baseParamType;
                return baseParamType;
            }

            ParamTypes[tp] = ParamType.Unknown;
            return ParamType.Unknown;
        }
        
        private static IntPtr MarshalToUnmanaged(object obj)
        {
            if (obj == null)
                return IntPtr.Zero;
            
            var type = obj.GetType();
            IntPtr ptr = IntPtr.Zero;

            if (type.IsPrimitive)
            {
                switch (obj)
                {
                    case byte b:
                        ptr = Marshal.AllocHGlobal(sizeof(byte));
                        Marshal.WriteByte(ptr, b);
                        return ptr;

                    case sbyte sb:
                        ptr = Marshal.AllocHGlobal(sizeof(sbyte));
                        Marshal.WriteByte(ptr, (byte)sb);
                        return ptr;

                    case short s:
                        ptr = Marshal.AllocHGlobal(sizeof(short));
                        Marshal.WriteInt16(ptr, s);
                        return ptr;

                    case ushort us:
                        ptr = Marshal.AllocHGlobal(sizeof(ushort));
                        Marshal.WriteInt16(ptr, (short)us);
                        return ptr;

                    case int i:
                        ptr = Marshal.AllocHGlobal(sizeof(int));
                        Marshal.WriteInt32(ptr, i);
                        return ptr;

                    case uint ui:
                        ptr = Marshal.AllocHGlobal(sizeof(uint));
                        Marshal.WriteInt32(ptr, (int)ui);
                        return ptr;

                    case long l:
                        ptr = Marshal.AllocHGlobal(sizeof(long));
                        Marshal.WriteInt64(ptr, l);
                        return ptr;

                    case ulong ul:
                        ptr = Marshal.AllocHGlobal(sizeof(ulong));
                        Marshal.WriteInt64(ptr, (long)ul);
                        return ptr;

                    case bool b:
                        ptr = Marshal.AllocHGlobal(sizeof(bool));
                        Marshal.WriteByte(ptr, b ? (byte)1 : (byte)0);
                        return ptr;

                    case float f:
                        ptr = Marshal.AllocHGlobal(sizeof(float));
                        Marshal.WriteInt32(ptr, BitConverter.ToInt32(BitConverter.GetBytes(f), 0));
                        return ptr;

                    case double d:
                        ptr = Marshal.AllocHGlobal(sizeof(double));
                        Marshal.WriteInt64(ptr, BitConverter.DoubleToInt64Bits(d));
                        return ptr;
                }
            }

            ptr = Marshal.AllocHGlobal(Marshal.SizeOf(type));
            Marshal.StructureToPtr(obj, ptr, false);
            return ptr;
        }
        
        
        private readonly List<object> _parameters = new List<object>();

        public Parameters Push(object value)
        {
            _parameters.Add(value);
            return this;
        }

        public int Size()
        {
            return _parameters.Count;
        }

        public RsParams Pack()
        {
            var count = _parameters.Count;
            var ptrArray = Marshal.AllocHGlobal(IntPtr.Size * count);

            for (var i = 0; i < count; i++)
            {
                var param = _parameters[i];
                var type = GetParamType(param.GetType());

                var valuePtr = MarshalToUnmanaged(param); // alloc + copy

                var rsParam = new RsParam
                {
                    type = (uint) type,
                    value = valuePtr
                };

                var rsParamPtr = Marshal.AllocHGlobal(Marshal.SizeOf<RsParam>());
                Marshal.StructureToPtr(rsParam, rsParamPtr, false);

                Marshal.WriteIntPtr(ptrArray, i * IntPtr.Size, rsParamPtr);
            }

            return new RsParams
            {
                param_count = (uint)count,
                params_array = ptrArray
            };
        }

        public static Parameters Unpack(RsParams rsParams)
        {
            
            var parameters = new Parameters();

            for (var i = 0; i < rsParams.param_count; i++)
            {
                Plugin.Info($"1  {rsParams.params_array} ({rsParams.param_count})");
                var paramPtr = Marshal.ReadIntPtr(rsParams.params_array, i * IntPtr.Size);
                Plugin.Info("2");
                var rsParam = Marshal.PtrToStructure<RsParam>(paramPtr);

                Plugin.Info($"Loading value of type {rsParam.type}");
                
                var objPtr = rsParam.value;
                object managedValue = null;
                switch (rsParam.type)
                {
                    case (uint)ParamType.I8: managedValue = (sbyte)Marshal.ReadByte(objPtr); break;
                    case (uint)ParamType.I16: managedValue = Marshal.ReadInt16(objPtr); break;
                    case (uint)ParamType.I32: managedValue = Marshal.ReadInt32(objPtr); break;
                    case (uint)ParamType.I64: managedValue = Marshal.ReadInt64(objPtr); break;

                    case (uint)ParamType.U8: managedValue = Marshal.ReadByte(objPtr); break;
                    case (uint)ParamType.U16: managedValue = (ushort)Marshal.ReadInt16(objPtr); break;
                    case (uint)ParamType.U32: managedValue = (uint)Marshal.ReadInt32(objPtr); break;
                    case (uint)ParamType.U64: managedValue = (ulong)Marshal.ReadInt64(objPtr); break;

                    case (uint)ParamType.F32: managedValue = BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(objPtr)), 0); break;
                    case (uint)ParamType.F64: managedValue = BitConverter.Int64BitsToDouble(Marshal.ReadInt64(objPtr)); break;

                    case (uint)ParamType.Boolean: managedValue = Marshal.ReadByte(objPtr) != 0; break;
                    case (uint)ParamType.String: managedValue = Marshal.PtrToStructure<RsString>(objPtr); break;

                    case (uint)ParamType.ColorNote: managedValue = Marshal.PtrToStructure<ColorNoteRs>(objPtr); break;
                    
                    case (uint)ParamType.Vec3: managedValue = Marshal.PtrToStructure<Vector3>(objPtr); break;
                    case (uint)ParamType.Quat: managedValue = Marshal.PtrToStructure<Quaternion>(objPtr); break;
                }
                
                Plugin.Info($"Loaded value: {managedValue}");

                parameters.Push(managedValue);
            }

            return parameters;
        }
        
        public T GetParameter<T>(int index)
        {
            // Ensure the index is within bounds
            if (index < 0 || index >= _parameters.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }

            var param = _parameters[index];

            var expectedType = typeof(T);

            if (param.GetType() != expectedType)
            {
                throw new InvalidCastException($"Parameter at index {index} is not of type {expectedType.FullName}. Found: {param.GetType().FullName}");
            }

            return (T) param;
        }

    }
}