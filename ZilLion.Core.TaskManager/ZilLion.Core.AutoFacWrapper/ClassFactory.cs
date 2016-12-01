using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using AutofacContrib.DynamicProxy;
using ZilLion.Core.AutoFacWrapper.IInterceptor;

namespace ZilLion.Core.AutoFacWrapper
{
    public static class ClassFactory
    {
        static ClassFactory()
        {
            IConainer = null;
            _builder = new ContainerBuilder();
            dicILifetimeScopes = new Dictionary<string, ILifetimeScope>();
            ViewDictionary = new Dictionary<Type, string>();
        }

        #region GetInstance

        /// <summary>
        ///     获取实例 泛型方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T GetInstance<T>(params Parameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                return GetContainer().Resolve<T>();
            }
            return GetContainer().Resolve<T>(parameters);
        }


        public static T GetInstance<T>()
        {
            return GetContainer().Resolve<T>();
        }

        public static object GetInstance(Type type, params Parameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                return GetContainer().Resolve(type);
            }

            return GetContainer().Resolve(type, parameters);
        }

        public static object GetInstance(Type type)
        {
            return GetInstance(type, new Parameter[0]);
        }

        #region 窗体单例封装

        public static T GetViewSingleInstance<T>(params Parameter[] parameters)
        {
            ILifetimeScope lifetimeScope = null;
            var typeName = typeof(T).FullName.Split('.');
            var interfaceNmae = typeName[typeName.Length - 1].ToUpper();
            if (dicILifetimeScopes.ContainsKey(interfaceNmae))
            {
                lifetimeScope = dicILifetimeScopes[interfaceNmae];
            }
            else
            {
                lifetimeScope = GetContainer().BeginLifetimeScope();
                dicILifetimeScopes.Add(interfaceNmae, lifetimeScope);
            }

            return lifetimeScope.Resolve<T>(parameters);
        }

        public static object GetViewSingleInstance(Type type, params Parameter[] parameters)
        {
            ILifetimeScope lifetimeScope = null;
            var typeName = type.FullName.Split('.');
            var interfaceNmae = typeName[typeName.Length - 1].ToUpper();
            if (dicILifetimeScopes.ContainsKey(interfaceNmae))
            {
                lifetimeScope = dicILifetimeScopes[interfaceNmae];
            }
            else
            {
                lifetimeScope = GetContainer().BeginLifetimeScope();
                dicILifetimeScopes.Add(interfaceNmae, lifetimeScope);
            }
            return lifetimeScope.Resolve(type, parameters);
        }

        public static void DisposeView(string TypeName)
        {
            ILifetimeScope lifetimeScope = null;

            var typeName = TypeName.Split('.');
            var interfaceNmae = "I" + typeName[typeName.Length - 1].ToUpper();

            if (dicILifetimeScopes.ContainsKey(interfaceNmae))
            {
                lifetimeScope = dicILifetimeScopes[interfaceNmae];
            }

            dicILifetimeScopes.Remove(interfaceNmae);
            if (lifetimeScope != null)
                lifetimeScope.Dispose();
        }

        #endregion

        #endregion

        #region  Register



        #region RegisterScreen

        /// <summary>
        ///     瞬态注册窗体为接口
        /// </summary>
        /// <typeparam name="TLt"></typeparam>
        /// <typeparam name="T"></typeparam>
        public static void RegisterViewPerDependency<TLt, T>(string winId) where TLt : T
        {
            //如果找到主窗体特性

            if (!ViewDictionary.ContainsKey(typeof(T)))
                ViewDictionary.Add(typeof(T), winId);

            Builder.RegisterType<TLt>().As<T>().InstancePerDependency();
        }


        /// <summary>
        ///     注册成单例为接口
        /// </summary>
        /// <typeparam name="TLt"></typeparam>
        /// <typeparam name="T"></typeparam>
        public static void RegisterViewSingleInstance<TLt, T>(string winId) where TLt : T
        {
            //如果找到主窗体特性
            if (!ViewDictionary.ContainsKey(typeof(T)))
                ViewDictionary.Add(typeof(T), winId);
            if (typeof(T).GetCustomAttributes<MainViewAttribute>().Any())
            {
                RegisterTypePreLifeTime<TLt, T>();
            }
            else
            {
                Builder.RegisterType<TLt>().As<T>().SingleInstance();
            }
        }

        #endregion

        /// <summary>
        ///     LifetimeScope注册类型
        /// </summary>
        /// <typeparam name="TLt"></typeparam>
        /// <typeparam name="T"></typeparam>
        public static void RegisterTypePreLifeTime<TLt, T>() where TLt : T
        {
            Builder.RegisterType<TLt>().As<T>().InstancePerLifetimeScope();
        }

        /// <summary>
        ///     注册程序集
        /// </summary>
        /// <param name="parameters"></param>
        public static void RegisterAssemblis(params Assembly[] parameters)
        {
            //AsSelf 必须写，注册程序集里没有继承接口的Types
            //Below is the code for my Autofac module.  
            //Please make sure you add the .AsSelf convention.  
            //If you do NOT do this your hubs will not be created.  
            //This is because there is no interface attached so Autofac will have no idea how to create the class.
            //AsImplementedInterfaces 
            Builder.RegisterAssemblyTypes(parameters)
                .AsImplementedInterfaces()
               ;

        }

        /// <summary>
        ///     注册成单例为接口
        /// </summary>
        /// <typeparam name="TLt"></typeparam>
        /// <typeparam name="T"></typeparam>
        public static void RegisterTypeSingleInstance<TLt, T>() where TLt : T
        {
            Builder.RegisterType<TLt>().As<T>().SingleInstance();
        }

        /// <summary>
        ///     单例注册
        /// </summary>
        /// <typeparam name="TLt"></typeparam>
        public static void RegisterTypeSingleInstance<TLt>()
        {
            Builder.RegisterType<TLt>().SingleInstance();
        }


        /// <summary>
        ///     瞬态注册为接口
        /// </summary>
        /// <typeparam name="TLt"></typeparam>
        /// <typeparam name="T"></typeparam>
        public static void RegisterTypePerDependency<TLt, T>() where TLt : T
        {
            Builder.RegisterType<TLt>().As<T>().InstancePerDependency();
        }

        /// <summary>
        ///     注册泛型
        /// </summary>
        /// <typeparam name="TLt"></typeparam>
        /// <typeparam name="T"></typeparam>
        public static void RegisterTypePerDependencyWithGeneric(Type instancetype, Type interfacetype)
        {
            Builder.RegisterGeneric(instancetype).As(interfacetype).InstancePerDependency();
        }

        /// <summary>
        ///     瞬态注册
        /// </summary>
        /// <typeparam name="TLt"></typeparam>
        /// <typeparam name="T"></typeparam>
        public static void RegisterTypePerDependency<TLt>()
        {
            Builder.RegisterType<TLt>().InstancePerDependency();
        }

        #endregion

        #region 属性

        public static Func<string, Type, object, object> Getinstance;
        private static readonly ContainerBuilder _builder;
        private static readonly Dictionary<string, ILifetimeScope> dicILifetimeScopes;

        /// <summary>
        ///     readOnly
        /// </summary>
        public static ContainerBuilder Builder
        {
            get { return _builder; }
        }

        /// <summary>
        ///     readOnly
        /// </summary>
        public static IContainer IConainer { get; private set; }

        /// <summary>
        ///     IConainer 相当于单例
        /// </summary>
        /// <returns></returns>
        public static IContainer GetContainer()
        {
            return IConainer ?? (IConainer = Builder.Build(ContainerBuildOptions.None));
        }

        /// <summary>
        ///     全局主窗体哈希缓存
        /// </summary>
        public static Dictionary<Type, string> ViewDictionary { get; set; }

        #endregion
    }

    /// <summary>
    ///     主窗体特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class MainViewAttribute : Attribute
    {
    }
}