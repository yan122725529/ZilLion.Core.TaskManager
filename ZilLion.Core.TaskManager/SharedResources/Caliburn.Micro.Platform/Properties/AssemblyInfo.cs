using System;
using System.Reflection;
using System.Runtime.InteropServices;
#if !NETFX_CORE && !WINDOWS_PHONE
using System.Windows.Markup;
#endif

[assembly: AssemblyTitle("Build20160712")]
[assembly: AssemblyDescription("A small, yet powerful framework designed for Xaml platforms, Caliburn.Micro implements a variety of UI patterns for solving real-world problems. Patterns that are highlighted include MVVM (Presentation Model), MVP and MVC.")]

#if !NETFX_CORE
[assembly: CLSCompliant(true)]
#endif

[assembly: ComVisible(false)]
[assembly: Guid("6449e9cb-4d4d-4d79-8f73-71a2fc647109")]

#if !NETFX_CORE && !WINDOWS_PHONE
[assembly: XmlnsDefinition("http://www.caliburnproject.org", "Caliburn.Micro")]
[assembly: XmlnsPrefix("http://www.caliburnproject.org", "cal")]
#endif
[assembly: AssemblyCompanyAttribute("商周（R）")]
[assembly: AssemblyProductAttribute("Caliburn Micro Platform")]
[assembly: AssemblyTrademarkAttribute("商周（R）")]
[assembly: AssemblyVersionAttribute("9.0.101")]
[assembly: AssemblyFileVersionAttribute("9.0.101")]
