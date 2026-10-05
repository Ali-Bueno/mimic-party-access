// Unity 6 interop's UnityEngine.CoreModule exposes these attributes publicly with incompatible constructors;
// defining them here makes the compiler use ours (see reference/engines/bepinex/bepinex-project-setup.md).
#pragma warning disable CS0436
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
internal sealed class NullableAttribute : Attribute
{
    public readonly byte[] NullableFlags;
    public NullableAttribute(byte flag) => NullableFlags = new[] { flag };
    public NullableAttribute(byte[] flags) => NullableFlags = flags;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
internal sealed class NullableContextAttribute : Attribute
{
    public readonly byte Flag;
    public NullableContextAttribute(byte flag) => Flag = flag;
}
