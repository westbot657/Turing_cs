using System;
using System.Runtime.InteropServices;

namespace Turing.Wasm.Wrappers
{
    public class PrimitiveWrappers
    {
        
    }
    
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RustBool
    {
        public byte value;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct RsString
    {
        public IntPtr strPtr;
        
        public RsString(string s)
        {
            strPtr = Marshal.StringToHGlobalUni(s);
        }

        public override string ToString()
        {
            return Marshal.PtrToStringUni(strPtr) ?? string.Empty;
        }

        public void Free()
        {
            Marshal.FreeHGlobal(strPtr);
        }

    }
    
    
}