using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Turing.Wasm
{

    [AttributeUsage(AttributeTargets.Class)]
    public class CodecClass : Attribute
    {
        
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class Converter : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RustClass : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RustMethod : Attribute
    {
        public string RustName { get; }

        public RustMethod(string name)
        {
            RustName = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class RustField : Attribute
    {
        public string RustName { get; }

        public RustField(string name)
        {
            RustName = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class RustWrapped : Attribute
    {
        public string RustRustBindingFunction { get; }

        public RustWrapped(string rustBindingFunction)
        {
            RustRustBindingFunction = rustBindingFunction;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class WasmRsMethod : Attribute
    {
        public string WasmName { get; }
        public Type DelegateType { get; }

        public WasmRsMethod(string name, Type delegateType)
        {
            WasmName = name;
            DelegateType = delegateType;
        }
    }

    public static class RsMethods
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

                var del = Delegate.CreateDelegate(attr.DelegateType, method);
                var funcPtr = Marshal.GetFunctionPointerForDelegate(del);

                var namePtr = Marshal.StringToHGlobalAnsi(attr.WasmName);
                RegisterMethod(namePtr, funcPtr);

                Marshal.FreeHGlobal(namePtr);
            }
        }
    }

}
