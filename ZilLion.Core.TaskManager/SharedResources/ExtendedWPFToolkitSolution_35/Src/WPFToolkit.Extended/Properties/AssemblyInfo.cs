/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

#region Using directives

using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Markup;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "Xceed Extended WPF Toolkit" )]
[assembly: AssemblyDescription("This assembly implements various Windows Presentation Framework controls.")]

[assembly: AssemblyCompany("Xceed Software Inc.")]
[assembly: AssemblyProduct( "Xceed Extended WPF Toolkit" )]
[assembly: AssemblyCopyright( "Copyright © Xceed Software Inc. 2010-2012" )]
[assembly: AssemblyCulture( "" )]


// Needed to enable xbap scenarios
[assembly: AllowPartiallyTrustedCallers]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.SourceAssembly, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
)]

[assembly: XmlnsPrefix("http://schemas.xceed.com/wpf/xaml/toolkit", "xctk")]
[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit")]
[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.Core.Converters" )]
[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.Core.Input" )]
[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.Primitives")]
[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.PropertyGrid")]
[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.PropertyGrid.Attributes")]
[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.PropertyGrid.Commands")]
[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.PropertyGrid.Converters")]
[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.PropertyGrid.Editors")]
[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.Zoombox" )]


#pragma warning disable 1699
[assembly: AssemblyDelaySign( false )]
[assembly: AssemblyKeyFile( @"sn.snk" )]
[assembly: AssemblyKeyName( "" )]
#pragma warning restore 1699

