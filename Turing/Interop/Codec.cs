using System;
using System.Runtime.InteropServices;
using Turing.Interop.Wrappers;

namespace Turing.Interop
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

        [Converter]
        public static bool BoolPassThrough(bool b)
        {
            return b;
        }
        
        

        // Vec3
        [Converter]
        public static UnityEngine.Vector3 RsVector3ToUnityVector3(Vector3 vec)
        {
            return new UnityEngine.Vector3(vec.x, vec.y, vec.z);
        }
        [Converter]
        public static Vector3 UnityVector3ToRsVector3(UnityEngine.Vector3 vec)
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

        
        
        // string
        [Converter]
        public static RsString StringToRsString(string str)
        {
            return new RsString(str);
        }
        [Converter]
        public static string RsStringToString(RsString str)
        {
            return str.ToString();
        }

        // I will eventually make these be auto-generated,
        // full type names are required, because technically these types don't exist when source-gen processes them
        [Converter]
        public static Turing.Interop.Wrappers.ColorNoteRs ColorNoteToRsNote(ColorNote note)
        {
            return note.structInst;
        }

        [Converter]
        public static ColorNote RsNoteToColorNote(Turing.Interop.Wrappers.ColorNoteRs note)
        {
            var instance = (ColorNote) GCHandle.FromIntPtr(note.ptr).Target;
            return instance;
        }
        
    }
    
    
}