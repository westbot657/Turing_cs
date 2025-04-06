using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Turing.Interop
{
    
    [InteropClass("BindToDll")]
    public partial class WasmInterop
    {
        
        // This name is statically defined in source gen, so do NOT change it
        private const string WasmRs = @"C:\Users\Westb\Desktop\Turing_rs\target\debug\turing_rs.dll";

        public static readonly List<IDisposable> PersistentMemory = new List<IDisposable>();

        public static void ClearMemory()
        {
            foreach (var memory in PersistentMemory)
            {
                memory.Dispose();
            }
            PersistentMemory.Clear();
        }
        

        [DllImport(dllName: WasmRs, CallingConvention = CallingConvention.Cdecl)]
        public static extern void register_function(IntPtr name, IntPtr func);

        
        [RustCallback("cs_print")]
        public static void CsPrint(string message)
        {
            Plugin.Log.Info("[rs]: " + message);
        }



        [DllImport(dllName: WasmRs, CallingConvention = CallingConvention.Cdecl)]
        private static extern void initialize_wasm();


        public static void Init()
        {
            BindToDll();
            initialize_wasm();
        }
    }
}
