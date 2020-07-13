using System;
using System.Collections.Generic;
using System.Reflection;

public static class ReflectionHelper
{
    public static TResult ReflectionStaticCall<TResult>(this Type type, string methodName)
    {
        MethodInfo method;
        object result;

        method = type.GetMethod(methodName, Type.EmptyTypes);

        if (method == null)
        {
            throw new MissingMethodException(methodName);
        }

        result = method.Invoke(null, null);

        return (TResult)result;
    }

    public static TResult ReflectionPrivateStaticCall<TResult>(this Type type, string methodName)
    {
        MethodInfo method;
        object result;

        method = type.GetMethod(methodName,BindingFlags.NonPublic | BindingFlags.Static);

        if (method == null)
        {
            throw new MissingMethodException(methodName);
        }

        result = method.Invoke(null, null);

        return (TResult)result;
    }
    

    public static TResult ReflectionStaticCall<TResult, TArg>(this Type type, string methodName, TArg arg)
    {
        MethodInfo method;
        object result;

        method = type.GetMethod(methodName, new Type[] { typeof(TArg) });

        if (method == null)
        {
            throw new MissingMethodException(methodName);
        }

        result = method.Invoke(null, new object[] { arg });

        return (TResult)result;
    }

    public static TResult ReflectionStaticCall<TResult, TArg1, TArg2>(this Type type, string methodName, TArg1 arg, TArg2 arg2)
    {
        MethodInfo method;
        object result;

        method = type.GetMethod(methodName, new Type[] { typeof(TArg1), typeof(TArg2) });

        if (method == null)
        {
            throw new MissingMethodException(methodName);
        }

        result = method.Invoke(null, new object[] { arg, arg2 });
        return (TResult)result;
    }

    public static TResult ReflectionCall<TResult>(this object obj, string methodName)
    {
        object result = obj.ReflectionCall(methodName);
        return (TResult)result;
    }


    public static object ReflectionCall(this object obj, string methodName)
    {
        MethodInfo method;
        object result;


        method = obj.GetType().GetMethod(methodName, Type.EmptyTypes);

        if (method == null)
        {
            throw new MissingMethodException(methodName);
        }

        result = method.Invoke(obj, null);

        return result;
    }

    public static object ReflectionCall<TArg1>(this object obj, string methodName, TArg1 arg1)
    {
        MethodInfo method;
        object result;

        method = obj.GetType().GetMethod(methodName, new Type[] { typeof(TArg1) });
        if (method == null)
        {
            throw new MissingMethodException(methodName);
        }

        result = method.Invoke(obj, new object[] { arg1 });

        return result;
    }
    public static TResult ReflectionCall<TResult, TArg1>(this object obj, string methodName, TArg1 arg1)
    {
        object result = obj.ReflectionCall<TArg1>(methodName, arg1);
        return (TResult)result;
    }


    public static object ReflectionCall<TArg1, TArg2>(this object obj, string methodName, TArg1 arg1, TArg2 arg2)
    {
        MethodInfo method;
        object result;

        method = obj.GetType().GetMethod(methodName, new Type[] { typeof(TArg1), typeof(TArg2) });
        if (method == null)
        {
            throw new MissingMethodException(methodName);
        }

        result = method.Invoke(obj, new object[] { arg1, arg2 });

        return result;
    }
    public static TResult ReflectionCall<TResult, TArg1, TArg2>(this object obj, string methodName, TArg1 arg1, TArg2 arg2)
    {
        object result = obj.ReflectionCall<TArg1, TArg2>(methodName, arg1, arg2);
        return (TResult)result;
    }

    public static TResult ReflectionGetField<TResult>(this object obj, string fieldName)
    {
        FieldInfo fieldInfo;
        object result;

        fieldInfo = obj.GetType().GetField(fieldName);
        if (fieldInfo == null)
        {
            throw new MissingFieldException(fieldName);
        }

        result = fieldInfo.GetValue(obj);

        return (TResult)result;
    }

    public static object ReflectionNew(this Type type)
    {
        ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
        if (constructor == null)
        {
            string constructorName = type.FullName + "." + type.Name + "()";
            throw new MissingMethodException(constructorName);
        }

        return constructor.Invoke(null);
    }

    public static object ReflectionNew<TArg1>(this Type type, TArg1 arg1)
    {
        ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(TArg1) });
        if (constructor == null)
        {
            string constructorName = string.Format("{0}.{1}({2})", type.FullName, type.Name, typeof(TArg1).Name);
            throw new MissingMethodException(constructorName);
        }

        return constructor.Invoke(new object[] { arg1 });
    }

    public static object ReflectionNew<TArg1, TArg2>(this Type type, TArg1 arg1, TArg2 arg2)
    {
        ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(TArg1), typeof(TArg2) });
        if (constructor == null)
        {
            string constructorName = string.Format("{0}.{1}({2},{3})", type.FullName, type.Name, typeof(TArg1).Name, typeof(TArg2).Name);
            throw new MissingMethodException(constructorName);
        }

        return constructor.Invoke(new object[] { arg1, arg2 });
    }


    public static TResult ReflectionGetProperty<TResult>(this object obj, string propertyName)
    {
        Type type = obj.GetType();
        PropertyInfo p = type.GetProperty(propertyName);

        if (p == null)
        {
            throw new MissingMemberException(propertyName);
        }

        return (TResult)p.GetValue(obj,null);
    }


    public static object ReflectionGetProperty(this object obj, string propertyName)
    {
        return obj.ReflectionGetProperty<object>(propertyName);
    }

    public static TResult ReflectionStaticGetProperty<TResult>(this Type type, string propertyName)
    {
        PropertyInfo p = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty);
        if (p == null)
        {
            throw new MissingMemberException(propertyName);
        }

        return (TResult)p.GetValue(null,null);
    }

    public static TResult ReflectionPrivateGetField<TResult>(this object obj, string propertyName)
    {
        Type type = obj.GetType();
        FieldInfo p = type.GetField(propertyName,BindingFlags.NonPublic| BindingFlags.Instance | BindingFlags.GetField);

        if (p == null)
        {
            throw new MissingMemberException(propertyName);
        }

        return (TResult)p.GetValue(obj);
    }

    public static TResult ReflectionPrivateCall<TResult>(this object obj, string methodName)
    {
        MethodInfo method;
        object result;


        method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

        if (method == null)
        {
            throw new MissingMethodException(methodName);
        }

        result = method.Invoke(obj, null);

        return (TResult)result;
    }
    public static T CreateDelegate<T>(this MethodInfo method) where T : class
    {
        return Delegate.CreateDelegate(typeof(T), method) as T;
    }
    public static TResult ReflectionPrivateCall<TResult,Arg1>(this object obj, string methodName, Arg1 arg)
    {
        MethodInfo method;
        object result;


        method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

        if (method == null)
        {
            throw new MissingMethodException(methodName);
        }

        result = method.Invoke(obj, new object[] { arg });

        return (TResult)result;
    }

    private const BindingFlags ALL_BIND_FLAG = System.Reflection.BindingFlags.NonPublic |
                                               System.Reflection.BindingFlags.Public |
                                               System.Reflection.BindingFlags.Instance
                                               | System.Reflection.BindingFlags.Static;
    public static System.Object GetValue(System.Object obj, string fieldName)
    {
        var property = obj.GetType().GetProperty(fieldName, ALL_BIND_FLAG);
        if (property == null)
        {
            var field = obj.GetType().GetField(fieldName, ALL_BIND_FLAG);
            if (field != null)
                return field.GetValue(obj);
            else
                throw new System.Exception("Failt to find property: " + fieldName);
        }
        else
        {
            return property.GetValue(obj, null);
        }
    }

    public static void SetValue(System.Object obj, string fieldName, System.Object value)
    {
        var property = obj.GetType().GetProperty(fieldName, ALL_BIND_FLAG);
        if (property == null)
        {
            var field = obj.GetType().GetField(fieldName, ALL_BIND_FLAG);
            if (field != null)
                field.SetValue(obj, value);
            else
                throw new System.Exception("Failt to find property: " + fieldName);
        }
        else
        {
            property.SetValue(obj, value, null);
        }
    }

    public static System.Object InvokMethod(System.Object obj, string methodName, params object[] args)
    {
        var method = obj.GetType().GetMethod(methodName, ALL_BIND_FLAG);
        if (method == null)
            throw new MissingMethodException(methodName);
        return method.Invoke(obj, args);
    }
}

/// <summary>
/// Provides access to the internal UnityEngine.NoAllocHelpers methods.
// </summary>
public static class NoAllocHelpers
{
    static Func<object, System.Array> DExtractArrayFromList;
    public static T[] ExtractArrayFromList<T>(List<T> list)
    {
        if (DExtractArrayFromList == null)
        {
            var ass = Assembly.GetAssembly(typeof(UnityEngine.Mesh)); // any class in UnityEngine
            var type = ass.GetType("UnityEngine.NoAllocHelpers");
            var mExtractArrayFromList = type.GetMethod("ExtractArrayFromList", BindingFlags.Static | BindingFlags.Public);
            DExtractArrayFromList = mExtractArrayFromList.CreateDelegate<Func<object, System.Array>>();
        }
        return DExtractArrayFromList(list) as T[];
    }


    static Action<object,int> DResizeList;
    /// <summary>
    /// Resize a list.
    /// </summary>
    /// <typeparam name="T"><see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to resize.</param>
    /// <param name="size">The new length of the <see cref="List{T}"/>.</param>
    public static void ResizeList<T>(List<T> list, int size)
    {
        if (DResizeList == null)
        {
            var ass = Assembly.GetAssembly(typeof(UnityEngine.Mesh)); // any class in UnityEngine
            var type = ass.GetType("UnityEngine.NoAllocHelpers");
            var mResizeList = type.GetMethod("ResizeList", BindingFlags.Static | BindingFlags.Public);
            DResizeList = mResizeList.CreateDelegate <Action<object, int>>();
        }
        DResizeList.Invoke(list, size);
    }
}


