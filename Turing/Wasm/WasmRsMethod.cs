using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Turing.Wasm
{
    class WasmRsMethod : Attribute
    {
        public string WasmName { get; }
        public Type DelegateType { get; }

        public WasmRsMethod(string name, Type delegateType)
        {
            WasmName = name;
            DelegateType = delegateType;
        }
    }

    class RsMethods
    {
        private static void RegisterMethod(IntPtr name, IntPtr funcPtr)
        {
            WasmInterop.register_function(name, funcPtr);
        }

        public static void Register()
        {
            Plugin.Debug("cwd: " + System.IO.Directory.GetCurrentDirectory());

            var methods = typeof(WasmInterop)
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(typeof(WasmRsMethod), false).Any());

            foreach (var method in methods)
            {
                var attr = (WasmRsMethod)method.GetCustomAttributes(typeof(WasmRsMethod), false).FirstOrDefault();
                if (attr == null) continue;

                Plugin.Info($"Registering '{attr.WasmName}'");

                Delegate del = Delegate.CreateDelegate(attr.DelegateType, method);
                IntPtr funcPtr = Marshal.GetFunctionPointerForDelegate(del);

                IntPtr namePtr = Marshal.StringToHGlobalAnsi(attr.WasmName);
                RegisterMethod(namePtr, funcPtr);

                Marshal.FreeHGlobal(namePtr);
            }
        }
    }

}
