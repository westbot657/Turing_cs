using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Turing.Interop.Parameters;
using Turing.Interop.Wrappers;

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

        [RustCallback("beatmap_add_color_note")]
        public static void AddColorNoteToMap(ColorNote note)
        {
            Plugin.Info($"TODO: add note to map: {note}");
        }

        [RustCallback("create_color_note")]
        public static ColorNote CreateColorNote(float beat)
        {

            Plugin.Critical($"idk how to do this part... (make note at beat {beat})");
            
            return new ColorNote(null);
        }


        [DllImport(dllName: WasmRs, CallingConvention = CallingConvention.Cdecl)]
        private static extern void initialize_wasm();

        [DllImport(dllName: WasmRs, CallingConvention = CallingConvention.Cdecl)]
        private static extern RsParams load_script(IntPtr scriptPath);
        
        [DllImport(dllName: WasmRs, CallingConvention = CallingConvention.Cdecl)]
        private static extern RsParams call_script_function(IntPtr functionName, RsParams parameters);

        [DllImport(dllName: WasmRs, CallingConvention = CallingConvention.Cdecl)]
        private static extern void free_params(RsParams parameters);

        public static void FreeRsParams(RsParams parameters)
        {
            free_params(parameters);
        }
        
        public static void LoadScript(string scriptPath)
        {
            var s = Marshal.StringToHGlobalAnsi(scriptPath);
            var rawResult = load_script(s);
            Marshal.FreeHGlobal(s);
            
            var ret = Parameters.Parameters.Unpack(rawResult);

            if (ret.Size() != 1) return;
            var err = Codec.RsStringToString(ret.GetParameter<RsString>(0));
            throw new Exception($"RS/WASM ERROR: {err}");

        }

        public static void CallScriptFunction(string name, Parameters.Parameters parameters)
        {
            var s = Marshal.StringToHGlobalAnsi(name);
            var rawResult = call_script_function(s, parameters.Pack());
            Marshal.FreeHGlobal(s);
            
            var ret = Parameters.Parameters.Unpack(rawResult);

            if (ret.Size() != 1) return;
            var err = Codec.RsStringToString(ret.GetParameter<RsString>(0));
            throw new Exception($"RS/WASM ERROR: {err}");
        }


        public static void Init()
        {
            BindToDll();
            initialize_wasm();
            
            LoadScript(@"C:\Users\Westb\Desktop\turing_wasm\target\wasm32-unknown-unknown\debug\turing_wasm.wasm");
            
            CallScriptFunction("on_load", new Parameters.Parameters());
            
        }
    }
}
