using System;
using System.Runtime.InteropServices;
using Turing.Wasm.Wrappers;

namespace Turing.Wasm
{
    
    
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public float x, y, z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Quaternion
    {
        public float x, y, z, w;
    }

    
    /// <summary>
    ///  Helper class to convert between C# types and C structs
    /// </summary>
    [CodecClass]
    public static partial class Codec
    {

        // Passthrough converters. These types are already compatible with rust
        [Converter]
        public static int IntPassthrough(int i) // i32
        {
            return i;
        }

        [Converter]
        public static uint UIntPassthrough(uint i) // u32
        {
            return i;
        }

        [Converter]
        public static long LongPassthrough(long l) // i64
        {
            return l;
        }
        
        [Converter]
        public static ulong ULongPassthrough(ulong l) // u64
        {
            return l;
        }

        [Converter]
        public static float FloatPassthrough(float f) // f32
        {
            return f;
        }
        [Converter]
        public static double DoublePassthrough(double d) // f64
        {
            return d;
        }
        
        

        // Vec3
        [Converter]
        public static UnityEngine.Vector3 RsVecToUnityVec(Vector3 vec)
        {
            return new UnityEngine.Vector3(vec.x, vec.y, vec.z);
        }
        [Converter]
        public static Vector3 UnityVecToRsVec(UnityEngine.Vector3 vec)
        {
            return new Vector3 { x = vec.x, y = vec.y, z = vec.z };
        }
        
        
        
        // Quaternion
        [Converter]
        public static UnityEngine.Quaternion RsQuatToUnityQuat(Quaternion quat)
        {
            return new UnityEngine.Quaternion(quat.x, quat.y, quat.z, quat.w);
        }
        [Converter]
        public static Quaternion UnityQuatToRsQuat(UnityEngine.Quaternion quat)
        {
            return new Quaternion { x = quat.x, y = quat.y, z = quat.z, w = quat.w };
        }
        
        
        
        // Boolean
        [Converter]
        public static RustBool CsBoolToRsBool(bool b)
        {
            return new RustBool { value = (byte)(b ? 1 : 0) };
        }
        [Converter]
        public static bool RsBoolToCsBool(RustBool rustBool)
        {
            return rustBool.value != 0;
        }
        
    }
    
    
}