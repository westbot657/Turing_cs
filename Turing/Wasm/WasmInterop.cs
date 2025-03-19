using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Turing.Wasm
{
    public interface IWasmMemoryObject
    {
        int ReferenceId { get; set; }
    }
    
    public class WasmInterop
    {
        public const string WASMRS = "C:/Users/Westb/Desktop/script_bs/target/release/script_bs";

        private static int currentId = 0;
        private static Dictionary<int, IWasmMemoryObject> WasmRefLookupTable = new Dictionary<int, IWasmMemoryObject>();

        public static IWasmMemoryObject getObject(int id)
        {
            return WasmRefLookupTable.ContainsKey(id) ? WasmRefLookupTable[id] : null;
        }

        public static int InsertObject(IWasmMemoryObject obj)
        {
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
            //CsPrintDelegate csPrintFunc = new CsPrintDelegate(CsPrint);
            //set_function(csPrintFunc);

            RsMethods.Register();

            //ColorNote note = new ColorNote
            //{
            //    position = new Vector3 { x = 1.0f, y = 2.0f, z = 3.0f },
            //    orientation = new Quaternion { x = 0.0f, y = 0.707f, z = 0.0f, w = 0.707f }
            //};

            initialize_wasm();

            //dll_print(note);

        }
    }
}
