using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Turing.Wasm
{
    public interface IWasmMemoryObject
    {
        int ReferenceId { get; set; }
    }
    
    [InteropClass("BindToDll")]
    public partial class WasmInterop
    {
        
        // This name is statically defined in source gen, so do NOT change it
        public const string WASMRS = "C:/Users/Westb/Desktop/script_bs/target/release/script_bs";

        private static int currentId = 0;
        private static Dictionary<int, IWasmMemoryObject> WasmRefLookupTable = new Dictionary<int, IWasmMemoryObject>();

        public static IWasmMemoryObject getObject(int id)
        {
            return WasmRefLookupTable.TryGetValue(id, out var value) ? value : null;
        }

        public static int InsertObject(IWasmMemoryObject obj)
        {
            obj.ReferenceId = currentId;
            WasmRefLookupTable.Add(currentId, obj);
            return currentId++;
        }

        public static void RemoveObject(int id)
        {
            
            WasmRefLookupTable.Remove(id);
        }


        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //delegate void CsPrintDelegate(IntPtr message);

        [DllImport(dllName: WASMRS, CallingConvention = CallingConvention.Cdecl)]
        public static extern void register_function(IntPtr name, IntPtr func);


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void _CsPrint(IntPtr message);

        [WasmRsMethod("cs_print", typeof(_CsPrint))]
        static void CsPrint(IntPtr message)
        {
            string msg = Marshal.PtrToStringAnsi(message);
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
            RsMethods.Register();
            BindToDll();
            initialize_wasm();
            
        }
    }
}
