using System.Runtime.InteropServices;

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
        public static int IntPassthrough(int i)
        {
            return i;
        }

        [Converter]
        public static float FloatPassthrough(float f)
        {
            return f;
        }

        // Back and forth converters
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
        
    }
    
    
}