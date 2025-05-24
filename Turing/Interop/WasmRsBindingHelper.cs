using System;

namespace Turing.Interop
{

    [AttributeUsage(AttributeTargets.Class)]
    public class CodecClass : Attribute
    {
        
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class Converter : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RustClass : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class InteropClass : Attribute
    {
        public string RustName { get; set; }

        public InteropClass(string bindGenEntrypoint)
        {
            RustName = bindGenEntrypoint;
        }
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

    public class RustCallback : Attribute
    {
        public string RustName { get; }

        public RustCallback(string rustName)
        {
            RustName = rustName;
        }
    }
    
}
