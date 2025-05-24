using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Turing.Interop.Wrappers;

namespace Turing.Interop.Parameters
{

    [StructLayout(LayoutKind.Sequential)]
    public struct InteroperableError
    {
        public IntPtr type;
        public IntPtr message;
    }

    public struct InteropError
    {

        public InteropError(string Type, string Message)
        {
            this.Type = Type;
            this.Message = Message;
        }
        
        public string Type;
        public string Message;
    }

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

        Object  = 100,
        
        FuncRef = 200,
        
        InteropError = 900,
        
        Unknown       = -1,
    }

    public struct RsObject
    {
        public IntPtr ptr;
    }
    
    public struct Parameters
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

            { typeof(RsObject), ParamType.Object },
            
            { typeof(InteroperableError), ParamType.InteropError }
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
            IntPtr ptr;

            if (type.IsPrimitive)
            {
                ptr = Marshal.AllocHGlobal(Marshal.SizeOf(type));
                switch (obj)
                {
                    case byte b: Marshal.WriteByte(ptr, b); break;
                    case sbyte sb: Marshal.WriteByte(ptr, (byte)sb); break;
                    
                    case short s: Marshal.WriteInt16(ptr, s); break;
                    case ushort us: Marshal.WriteInt16(ptr, (short)us); break;
                    
                    case int i: Marshal.WriteInt32(ptr, i); break;
                    case uint ui: Marshal.WriteInt32(ptr, (int)ui); break;
                    
                    case long l: Marshal.WriteInt64(ptr, l); break;
                    case ulong ul: Marshal.WriteInt64(ptr, (long)ul); break;
                    
                    case bool b: Marshal.WriteByte(ptr, b ? (byte)1 : (byte)0); break;
                    
                    case float f: Marshal.WriteInt32(ptr, BitConverter.ToInt32(BitConverter.GetBytes(f), 0)); break;
                    case double d: Marshal.WriteInt64(ptr, BitConverter.DoubleToInt64Bits(d)); break;
                }

                return ptr;
            }

            ptr = Marshal.AllocHGlobal(Marshal.SizeOf(type));
            Marshal.StructureToPtr(obj, ptr, false);
            return ptr;
        }
        
        
        private List<object> _parameters;

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
            
            var parameters = new Parameters{ _parameters = new List<object>() };

            for (var i = 0; i < rsParams.param_count; i++)
            {
                var rsParam = Marshal.PtrToStructure<RsParam>(Marshal.ReadIntPtr(rsParams.params_array + (i * IntPtr.Size)));
                
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

                    case (uint)ParamType.Object: managedValue = Marshal.PtrToStructure<RsObject>(objPtr); break;
                    
                    case (uint)ParamType.InteropError: managedValue = Marshal.PtrToStructure<InteroperableError>(objPtr); break;
                }

                parameters.Push(managedValue);
            }
            
            Free(rsParams);

            return parameters;
        }

        [CanBeNull]
        public InteropError? CheckError()
        {
            if (_parameters.Count == 0) return null;
            
            var param = _parameters[0];

            if (param is InteroperableError err)
            {
                return Codec.RsErrorToCsError(err);
            }

            return null;
        }

        public void CheckErrorAndThrow()
        {
            var error = CheckError();
            if (error != null)
            {
                throw new Exception($"rs/wasm exception: {error?.Type}:\n{error?.Message}");
            }
        }

        private static void Free(RsParams rsParams)
        {
            WasmInterop.FreeRsParams(rsParams);
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

            if (param.GetType() == expectedType) return (T)param;
            
            if (param is InteroperableError err)
            {
                var error = Codec.RsErrorToCsError(err);
                throw new Exception($"rs/wasm exception: {error.Type}:\n{error.Message}");
            }

            throw new InvalidCastException($"Parameter at index {index} is not of type {expectedType.FullName}. Found: {param.GetType().FullName}");

        }

    }
}