﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ZilLion.Core.AutoFacWrapper;


namespace Caliburn.Micro
{
#if !WinRT
    using System.Windows;
#else
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
#endif
#if !SILVERLIGHT && !WinRT
    using System.Windows.Interop;

#endif

    /// <summary>
    ///     A strategy for determining which view to use for a given model.
    /// </summary>
    public static class ViewLocator
    {
        private static readonly ILog Log = LogManager.GetLog(typeof (ViewLocator));

        //These fields are used for configuring the default type mappings. They can be changed using ConfigureTypeMappings().
        private static string defaultSubNsViews;
        private static string defaultSubNsViewModels;
        private static bool useNameSuffixesInMappings;
        private static string nameFormat;
        private static string viewModelSuffix;
        private static readonly List<string> ViewSuffixList = new List<string>();
        private static bool includeViewSuffixInVmNames;

        /// <summary>
        ///     Used to transform names.
        /// </summary>
        public static NameTransformer NameTransformer = new NameTransformer();

        /// <summary>
        ///     Separator used when resolving View names for context instances.
        /// </summary>
        public static string ContextSeparator = ".";

        /// <summary>
        ///     Retrieves the view from the IoC container or tries to create it if not found.
        /// </summary>
        /// <remarks>
        ///     Pass the type of view as a parameter and recieve an instance of the view.
        /// </remarks>
        public static Func<Type, UIElement> GetOrCreateViewType = viewType =>
        {
#if !WinRT
            if (viewType.IsInterface || viewType.IsAbstract || !typeof (UIElement).IsAssignableFrom(viewType))
                return
                    new CaliburnErrorMessageView
                    {
                        Errtext = {Text = string.Format("Cannot create {0}.", viewType.FullName)}
                    };

#else
            var viewTypeInfo = viewType.GetTypeInfo();
            var uiElementInfo = typeof(UIElement).GetTypeInfo();

            if (viewTypeInfo.IsInterface || viewTypeInfo.IsAbstract || !uiElementInfo.IsAssignableFrom(viewTypeInfo))
                return new TextBlock { Text = string.Format("Cannot create {0}.", viewType.FullName) };
#endif

            var view = ClassFactory.GetInstance(viewType) as UIElement;

            if (view != null)
            {
                InitializeComponent(view);
#if DEBUG
                Debug.WriteLine("View:{0}_Hash{1}", view.GetType(), view.GetHashCode());
#endif


                return view;
            }


            return
                new CaliburnErrorMessageView
                {
                    Errtext = {Text = string.Format("Cannot create {0}.", viewType.FullName)}
                };
        };

        /// <summary>
        ///     Modifies the name of the type to be used at design time.
        /// </summary>
        public static Func<string, string> ModifyModelTypeAtDesignTime = modelTypeName =>
        {
            if (modelTypeName.StartsWith("_"))
            {
                var index = modelTypeName.IndexOf('.');
                modelTypeName = modelTypeName.Substring(index + 1);
                index = modelTypeName.IndexOf('.');
                modelTypeName = modelTypeName.Substring(index + 1);
            }

            return modelTypeName;
        };

        /// <summary>
        ///     Transforms a ViewModel type name into all of its possible View type names. Optionally accepts an instance
        ///     of context object
        /// </summary>
        /// <returns>Enumeration of transformed names</returns>
        /// <remarks>
        ///     Arguments:
        ///     typeName = The name of the ViewModel type being resolved to its companion View.
        ///     context = An instance of the context or null.
        /// </remarks>
        public static Func<string, object, IEnumerable<string>> TransformName = (typeName, context) =>
        {
            Func<string, string> getReplaceString;

            if (context == null)
            {
                getReplaceString = r => r;
                return NameTransformer.Transform(typeName, getReplaceString);
            }

            var contextstr = ContextSeparator + context;
            var grpsuffix = string.Empty;
            if (useNameSuffixesInMappings)
            {
                //Create RegEx for matching any of the synonyms registered
                var synonymregex = "(" + string.Join("|", ViewSuffixList.ToArray()) + ")";
                grpsuffix = RegExHelper.GetCaptureGroup("suffix", synonymregex);
            }

            const string grpbase = @"\${basename}";
            var patternregex = string.Format(nameFormat, grpbase, grpsuffix) + "$";

            //Strip out any synonym by just using contents of base capture group with context string
            var replaceregex = "${basename}" + contextstr;

            //Strip out the synonym
            getReplaceString = r =>
            {
                var result = Regex.Replace(r, patternregex, replaceregex);

                return result;
            };


            //Return only the names for the context
            return NameTransformer.Transform(typeName, getReplaceString).Where(n => n.EndsWith(contextstr));
        };

        /// <summary>
        ///     Locates the view type based on the specified model type.
        /// </summary>
        /// <returns>The view.</returns>
        /// <remarks>
        ///     Pass the model type, display location (or null) and the context instance (or null) as parameters and receive a view
        ///     type.
        /// </remarks>
        public static Func<Type, DependencyObject, object, Type> LocateTypeForModelType =
            (modelType, displayLocation, context) =>
            {
                var viewTypeName = modelType.FullName;

                if (View.InDesignMode)
                {
                    viewTypeName = ModifyModelTypeAtDesignTime(viewTypeName);
                }

                viewTypeName = viewTypeName.Substring(
                    0,
                    viewTypeName.IndexOf('`') < 0
                        ? viewTypeName.Length
                        : viewTypeName.IndexOf('`')
                    );

                var viewTypeList = TransformName(viewTypeName, context);
                var viewType = AssemblySource.FindTypeByNames(viewTypeList);

                if (viewType == null)
                {
                    Log.Warn("View not found. Searched: {0}.", string.Join(", ", viewTypeList.ToArray()));
                }

                return viewType;
            };

        /// <summary>
        ///     Locates the view for the specified model type.
        /// </summary>
        /// <returns>The view.</returns>
        /// <remarks>
        ///     Pass the model type, display location (or null) and the context instance (or null) as parameters and receive a view
        ///     instance.
        /// </remarks>
        public static Func<Type, DependencyObject, object, UIElement> LocateForModelType =
            (modelType, displayLocation, context) => FindViewOfViewModel(modelType, displayLocation, context);

        /// <summary>
        ///     Locates the view for the specified model instance.
        /// </summary>
        /// <returns>The view.</returns>
        /// <remarks>
        ///     Pass the model instance, display location (or null) and the context (or null) as parameters and receive a view
        ///     instance.
        /// </remarks>
        public static Func<object, DependencyObject, object, UIElement> LocateForModel =
            (model, displayLocation, context) =>
            {
                var viewAware = model as IViewAware;
                if (viewAware != null)
                {
                    var view = viewAware.GetView(context) as UIElement;
                    if (view != null)
                    {
#if !SILVERLIGHT && !WinRT
                        var windowCheck = view as Window;
                        if (windowCheck == null ||
                            (!windowCheck.IsLoaded && !(new WindowInteropHelper(windowCheck).Handle == IntPtr.Zero)))
                        {
                            Log.Info("Using cached view for {0}.", model);
                            return view;
                        }
#else
                    Log.Info("Using cached view for {0}.", model);
                    return view;
#endif
                    }
                }

                return LocateForModelType(model.GetType(), displayLocation, context);
            };

        /// <summary>
        ///     Transforms a view type into a pack uri.
        /// </summary>
        public static Func<Type, Type, string> DeterminePackUriFromType = (viewModelType, viewType) =>
        {
#if !WinRT
            var assemblyName = viewType.Assembly.GetAssemblyName();
            var applicationAssemblyName = Application.Current.GetType().Assembly.GetAssemblyName();
#else
            var assemblyName = viewType.GetTypeInfo().Assembly.GetAssemblyName();
            var applicationAssemblyName = Application.Current.GetType().GetTypeInfo().Assembly.GetAssemblyName();
#endif
            var viewTypeName = viewType.FullName;

            if (viewTypeName.StartsWith(assemblyName))
                viewTypeName = viewTypeName.Substring(assemblyName.Length);

            var uri = viewTypeName.Replace(".", "/") + ".xaml";

            if (!applicationAssemblyName.Equals(assemblyName))
            {
                return "/" + assemblyName + ";component" + uri;
            }

            return uri;
        };

        static ViewLocator()
        {
            ConfigureTypeMappings(new TypeMappingConfiguration());
        }

        /// <summary>
        ///     Specifies how type mappings are created, including default type mappings. Calling this method will
        ///     clear all existing name transformation rules and create new default type mappings according to the
        ///     configuration.
        /// </summary>
        /// <param name="config">An instance of TypeMappingConfiguration that provides the settings for configuration</param>
        public static void ConfigureTypeMappings(TypeMappingConfiguration config)
        {
            if (string.IsNullOrEmpty(config.DefaultSubNamespaceForViews))
            {
                throw new ArgumentException("DefaultSubNamespaceForViews field cannot be blank.");
            }

            if (string.IsNullOrEmpty(config.DefaultSubNamespaceForViewModels))
            {
                throw new ArgumentException("DefaultSubNamespaceForViewModels field cannot be blank.");
            }

            if (string.IsNullOrEmpty(config.NameFormat))
            {
                throw new ArgumentException("NameFormat field cannot be blank.");
            }

            NameTransformer.Clear();
            ViewSuffixList.Clear();

            defaultSubNsViews = config.DefaultSubNamespaceForViews;
            defaultSubNsViewModels = config.DefaultSubNamespaceForViewModels;
            nameFormat = config.NameFormat;
            useNameSuffixesInMappings = config.UseNameSuffixesInMappings;
            viewModelSuffix = config.ViewModelSuffix;
            ViewSuffixList.AddRange(config.ViewSuffixList);
            includeViewSuffixInVmNames = config.IncludeViewSuffixInViewModelNames;

            SetAllDefaults();
        }


        private static void SetAllDefaults()
        {
            if (useNameSuffixesInMappings)
            {
                //Add support for all view suffixes
                ViewSuffixList.Apply(AddDefaultTypeMapping);
            }
            else
            {
                AddSubNamespaceMapping(defaultSubNsViewModels, defaultSubNsViews);
            }
        }

        /// <summary>
        ///     Adds a default type mapping using the standard namespace mapping convention
        /// </summary>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddDefaultTypeMapping(string viewSuffix = "View")
        {
            if (!useNameSuffixesInMappings)
            {
                return;
            }

            //Check for <Namespace>.<BaseName><ViewSuffix> construct
            AddNamespaceMapping(string.Empty, string.Empty, viewSuffix);

            //Check for <Namespace>.ViewModels.<NameSpace>.<BaseName><ViewSuffix> construct
            AddSubNamespaceMapping(defaultSubNsViewModels, defaultSubNsViews, viewSuffix);
        }

        /// <summary>
        ///     This method registers a View suffix or synonym so that View Context resolution works properly.
        ///     It is automatically called internally when calling AddNamespaceMapping(), AddDefaultTypeMapping(),
        ///     or AddTypeMapping(). It should not need to be called explicitly unless a rule that handles synonyms
        ///     is added directly through the NameTransformer.
        /// </summary>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View".</param>
        public static void RegisterViewSuffix(string viewSuffix)
        {
            if (ViewSuffixList.Count(s => s == viewSuffix) == 0)
            {
                ViewSuffixList.Add(viewSuffix);
            }
        }

        /// <summary>
        ///     Adds a standard type mapping based on namespace RegEx replace and filter patterns
        /// </summary>
        /// <param name="nsSourceReplaceRegEx">RegEx replace pattern for source namespace</param>
        /// <param name="nsSourceFilterRegEx">RegEx filter pattern for source namespace</param>
        /// <param name="nsTargetsRegEx">Array of RegEx replace values for target namespaces</param>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddTypeMapping(string nsSourceReplaceRegEx, string nsSourceFilterRegEx,
            string[] nsTargetsRegEx, string viewSuffix = "View")
        {
            RegisterViewSuffix(viewSuffix);

            var replist = new List<string>();
            var repsuffix = useNameSuffixesInMappings ? viewSuffix : string.Empty;
            const string basegrp = "${basename}";

            foreach (var t in nsTargetsRegEx)
            {
                replist.Add(t + string.Format(nameFormat, basegrp, repsuffix));
            }

            var rxbase = RegExHelper.GetNameCaptureGroup("basename");
            var suffix = string.Empty;
            if (useNameSuffixesInMappings)
            {
                suffix = viewModelSuffix;
                if (!viewModelSuffix.Contains(viewSuffix) && includeViewSuffixInVmNames)
                {
                    suffix = viewSuffix + suffix;
                }
            }
            var rxsrcfilter = string.IsNullOrEmpty(nsSourceFilterRegEx)
                ? null
                : string.Concat(nsSourceFilterRegEx, string.Format(nameFormat, RegExHelper.NameRegEx, suffix), "$");
            var rxsuffix = RegExHelper.GetCaptureGroup("suffix", suffix);

            NameTransformer.AddRule(
                string.Concat(nsSourceReplaceRegEx, string.Format(nameFormat, rxbase, rxsuffix), "$"),
                replist.ToArray(),
                rxsrcfilter
                );
        }

        /// <summary>
        ///     Adds a standard type mapping based on namespace RegEx replace and filter patterns
        /// </summary>
        /// <param name="nsSourceReplaceRegEx">RegEx replace pattern for source namespace</param>
        /// <param name="nsSourceFilterRegEx">RegEx filter pattern for source namespace</param>
        /// <param name="nsTargetRegEx">RegEx replace value for target namespace</param>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddTypeMapping(string nsSourceReplaceRegEx, string nsSourceFilterRegEx, string nsTargetRegEx,
            string viewSuffix = "View")
        {
            AddTypeMapping(nsSourceReplaceRegEx, nsSourceFilterRegEx, new[] {nsTargetRegEx}, viewSuffix);
        }

        /// <summary>
        ///     Adds a standard type mapping based on simple namespace mapping
        /// </summary>
        /// <param name="nsSource">Namespace of source type</param>
        /// <param name="nsTargets">Namespaces of target type as an array</param>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddNamespaceMapping(string nsSource, string[] nsTargets, string viewSuffix = "View")
        {
            //need to terminate with "." in order to concatenate with type name later
            var nsencoded = RegExHelper.NamespaceToRegEx(nsSource + ".");

            //Start pattern search from beginning of string ("^")
            //unless original string was blank (i.e. special case to indicate "append target to source")
            if (!string.IsNullOrEmpty(nsSource))
            {
                nsencoded = "^" + nsencoded;
            }

            //Capture namespace as "origns" in case we need to use it in the output in the future
            var nsreplace = RegExHelper.GetCaptureGroup("origns", nsencoded);

            var nsTargetsRegEx = nsTargets.Select(t => t + ".").ToArray();
            AddTypeMapping(nsreplace, null, nsTargetsRegEx, viewSuffix);
        }

        /// <summary>
        ///     Adds a standard type mapping based on simple namespace mapping
        /// </summary>
        /// <param name="nsSource">Namespace of source type</param>
        /// <param name="nsTarget">Namespace of target type</param>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddNamespaceMapping(string nsSource, string nsTarget, string viewSuffix = "View")
        {
            AddNamespaceMapping(nsSource, new[] {nsTarget}, viewSuffix);
        }

        /// <summary>
        ///     Adds a standard type mapping by substituting one subnamespace for another
        /// </summary>
        /// <param name="nsSource">Subnamespace of source type</param>
        /// <param name="nsTargets">Subnamespaces of target type as an array</param>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddSubNamespaceMapping(string nsSource, string[] nsTargets, string viewSuffix = "View")
        {
            //need to terminate with "." in order to concatenate with type name later
            var nsencoded = RegExHelper.NamespaceToRegEx(nsSource + ".");

            string rxbeforetgt, rxaftersrc, rxaftertgt;
            var rxbeforesrc = rxbeforetgt = rxaftersrc = rxaftertgt = string.Empty;

            if (!string.IsNullOrEmpty(nsSource))
            {
                if (!nsSource.StartsWith("*"))
                {
                    rxbeforesrc = RegExHelper.GetNamespaceCaptureGroup("nsbefore");
                    rxbeforetgt = @"${nsbefore}";
                }

                if (!nsSource.EndsWith("*"))
                {
                    rxaftersrc = RegExHelper.GetNamespaceCaptureGroup("nsafter");
                    rxaftertgt = "${nsafter}";
                }
            }

            var rxmid = RegExHelper.GetCaptureGroup("subns", nsencoded);
            var nsreplace = string.Concat(rxbeforesrc, rxmid, rxaftersrc);

            var nsTargetsRegEx = nsTargets.Select(t => string.Concat(rxbeforetgt, t, ".", rxaftertgt)).ToArray();
            AddTypeMapping(nsreplace, null, nsTargetsRegEx, viewSuffix);
        }

        /// <summary>
        ///     Adds a standard type mapping by substituting one subnamespace for another
        /// </summary>
        /// <param name="nsSource">Subnamespace of source type</param>
        /// <param name="nsTarget">Subnamespace of target type</param>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddSubNamespaceMapping(string nsSource, string nsTarget, string viewSuffix = "View")
        {
            AddSubNamespaceMapping(nsSource, new[] {nsTarget}, viewSuffix);
        }


        private static UIElement FindViewOfViewModel(Type modelType, DependencyObject displayLocation, object context)
        {
            var viewType = LocateTypeForModelType(modelType, displayLocation, context);
            if (viewType == null)
            {
                var view =
                    new CaliburnErrorMessageView
                    {
                        Errtext = {Text = string.Format("Cannot find view for {0}.", modelType)}
                    };
                return view;
            }
            return GetOrCreateViewType(viewType);
        }

        /// <summary>
        ///     When a view does not contain a code-behind file, we need to automatically call InitializeCompoent.
        /// </summary>
        /// <param name="element">The element to initialize</param>
        public static void InitializeComponent(object element)
        {
#if !WinRT
            var method = element.GetType()
                .GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
#else
            var method = element.GetType().GetTypeInfo()
                .GetDeclaredMethod("InitializeComponent");
#endif

            if (method == null)
                return;

            method.Invoke(element, null);
        }
    }
}