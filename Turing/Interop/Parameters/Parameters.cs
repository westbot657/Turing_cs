using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

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
        public IntPtr type;
        public IntPtr value;
    }
    
    
    
    public class Parameters
    {
        public List<object> parameters;

        public Parameters(RsParams rsParams)
        {
            parameters = new List<object>();

            var raw = new IntPtr[rsParams.param_count];
            
            Marshal.Copy(rsParams.params_array, raw, 0, (int)rsParams.param_count);

            foreach (var ptr in raw)
            {
                var param = Marshal.PtrToStructure<RsParam>(ptr);
                
                var param_type = Marshal.PtrToStringAnsi(param.type);
                
                var converter = Codec.getConverter(param_type);

                if (converter != null)
                {
                    
                }
                
            }
            
        }
        
    }
}