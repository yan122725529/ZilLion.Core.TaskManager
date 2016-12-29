﻿





using System;
using System.Reflection;

public static partial class Extensions
{
    /// <summary>A MemberInfo extension method that gets a declaraction.</summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>The declaraction.</returns>
    public static string GetDeclaraction(this MemberInfo @this)
    {
        switch (@this.MemberType)
        {
            case MemberTypes.Field:
                return ((FieldInfo) @this).GetDeclaraction();
            case MemberTypes.Property:
                return ((PropertyInfo) @this).GetDeclaraction();
            case MemberTypes.Constructor:
                return ((ConstructorInfo) @this).GetDeclaraction();
            case MemberTypes.Method:
                return ((MethodInfo) @this).GetDeclaraction();
            case MemberTypes.TypeInfo:
                return ((Type) @this).GetDeclaraction();
            case MemberTypes.NestedType:
                return ((Type) @this).GetDeclaraction();
            case MemberTypes.Event:
                return ((EventInfo) @this).GetDeclaraction();
            default:
                return null;
        }
    }
}