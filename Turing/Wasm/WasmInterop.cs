using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Turing.Wasm
{
    public class WasmInterop
    {
        private const string WASMRS = "C:/Users/Westb/Desktop/script_bs/target/release/script_bs";


        [StructLayout(LayoutKind.Sequential)]
        struct Vector3
        {
            public float x, y, z;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Quaternion
        {
            public float x, y, z, w;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ColorNote
        {
            public Vector3 position;
            public Quaternion orientation;
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
