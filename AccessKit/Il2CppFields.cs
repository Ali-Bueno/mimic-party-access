using Il2CppSystem.Reflection;

namespace AccessKit;

/// <summary>Reads fields by name through IL2CPP's own reflection, for conventions shared by many game types.</summary>
internal static class Il2CppFields
{
    private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    public static Il2CppSystem.Object? Read(Il2CppSystem.Object? target, string fieldName)
    {
        if (target == null)
            return null;
        var field = target.GetIl2CppType().GetField(fieldName, InstanceFields);
        return field?.GetValue(target);
    }
}
