using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Turing.Wasm
{
    
    [InteropClass("BindToDll")]
    public partial class WasmInterop
    {
        
        // This name is statically defined in source gen, so do NOT change it
        public const string WASMRS = "C:/Users/Westb/Desktop/script_bs/target/release/script_bs";

        public static List<IDisposable> PersistentMemory = new List<IDisposable>();

        public static void ClearMemory()
        {
            foreach (var memory in PersistentMemory)
            {
                memory.Dispose();
            }
            PersistentMemory.Clear();
        }
        
        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //delegate void CsPrintDelegate(IntPtr message);

        [DllImport(dllName: WASMRS, CallingConvention = CallingConvention.Cdecl)]
        public static extern void register_function(IntPtr name, IntPtr func);

        
        [RustCallback("cs_print")]
        static void CsPrint(IntPtr message)
        {
            var msg = Marshal.PtrToStringAnsi(message);
            Plugin.Log.Info("[rs]: " + msg);
        }



        [DllImport(dllName: WASMRS, CallingConvention = CallingConvention.Cdecl)]
        private static extern void initialize_wasm();

        [DllImport(dllName: WASMRS, CallingConvention = CallingConvention.Cdecl)]
        private static extern void load_script(IntPtr scriptPath);

        [DllImport(dllName: WASMRS, CallingConvention = CallingConvention.Cdecl)]
        private static extern void call_script_init();

        public static void Init()
        {
            BindToDll();
            initialize_wasm();
        }
    }
}
