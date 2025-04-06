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
            { typeof(string), ParamType.String },

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

            if (type.IsPrimitive || type == typeof(string))
            {
                if (type == typeof(string))
                {
                    return Marshal.StringToHGlobalAnsi((string)obj);
                }

                var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(type));
                Marshal.StructureToPtr(obj, ptr, false);
                return ptr;
            }

            // ReSharper disable once InvertIf
            if (type.IsValueType && type.IsLayoutSequential)
            {
                var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(type));
                Marshal.StructureToPtr(obj, ptr, false);
                return ptr;
            }

            throw new NotSupportedException($"Cannot marshal type {type.FullName} to unmanaged memory.");
        }
        
        
        private readonly List<object> _parameters = new List<object>();

        public Parameters Push(object value)
        {
            _parameters.Add(value);
            return this;
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
                var paramPtr = Marshal.ReadIntPtr(rsParams.params_array, i * IntPtr.Size);
                var rsParam = Marshal.PtrToStructure<RsParam>(paramPtr);

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