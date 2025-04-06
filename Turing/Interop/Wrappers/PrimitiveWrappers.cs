using System;
using System.Runtime.InteropServices;

namespace Turing.Interop.Wrappers
{
    public class PrimitiveWrappers
    {
        
    }
    

    [StructLayout(LayoutKind.Sequential)]
    public struct RsString
    {
        public IntPtr strPtr;
        
        public RsString(string s)
        {
            strPtr = Marshal.StringToHGlobalAnsi(s);
        }

        public override string ToString()
        {
            return Marshal.PtrToStringAnsi(strPtr) ?? string.Empty;
        }

        public void Free()
        {
            Marshal.FreeHGlobal(strPtr);
        }

    }
    
    
}