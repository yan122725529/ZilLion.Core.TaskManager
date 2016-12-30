﻿/************************************************************************

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

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Security;

namespace Xceed.Wpf.Toolkit.Core.Input
{
  public sealed class KeyModifierCollectionConverter : TypeConverter
  {
    #region Static Fields

    private static readonly TypeConverter _keyModifierConverter = TypeDescriptor.GetConverter( typeof( KeyModifier ) );

    #endregion

    public override bool CanConvertFrom( ITypeDescriptorContext typeDescriptorContext, Type type )
    {
      return _keyModifierConverter.CanConvertFrom( typeDescriptorContext, type );
    }

    public override bool CanConvertTo( ITypeDescriptorContext typeDescriptorContext, Type type )
    {
      return ( type == typeof( InstanceDescriptor )
          || type == typeof( KeyModifierCollection )
          || type == typeof( string ) );
    }

    public override object ConvertFrom( ITypeDescriptorContext typeDescriptorContext,
        CultureInfo cultureInfo, object value )
    {
      KeyModifierCollection result = new KeyModifierCollection();
      string stringValue = value as string;

      // convert null as None
      if( value == null
          || ( stringValue != null && stringValue.Trim() == string.Empty ) )
      {
        result.Add( KeyModifier.None );
      }
      else
      {
        // respect the following separators: '+', ' ', '|', or ','
        foreach( string token in stringValue.Split( new char[] { '+', ' ', '|', ',' },
                StringSplitOptions.RemoveEmptyEntries ) )
          result.Add( ( KeyModifier )_keyModifierConverter.ConvertFrom( typeDescriptorContext, cultureInfo, token ) );

        // if nothing added, assume None
        if( result.Count == 0 )
          result.Add( KeyModifier.None );
      }
      return result;
    }

    public override object ConvertTo( ITypeDescriptorContext typeDescriptorContext,
        CultureInfo cultureInfo, object value, Type destinationType )
    {
      // special handling for null or an empty collection
      if( value == null || ( ( KeyModifierCollection )value ).Count == 0 )
      {
        if( destinationType == typeof( InstanceDescriptor ) )
        {
          object result = null;
          try
          {
            result = ConstructInstanceDescriptor();
          }
          catch( SecurityException )
          {
          }
          return result;
        }
        else if( destinationType == typeof( string ) )
        {
          return _keyModifierConverter.ConvertTo( typeDescriptorContext,
                  cultureInfo, KeyModifier.None, destinationType );
        }
      }

      // return a '+' delimited string containing the modifiers
      if( destinationType == typeof( string ) )
      {
        string result = string.Empty;
        foreach( KeyModifier modifier in ( KeyModifierCollection )value )
        {
          if( result != string.Empty )
            result = result + '+';

          result = result + _keyModifierConverter.ConvertTo( typeDescriptorContext,
                  cultureInfo, modifier, destinationType );
        }
        return result;
      }

      // unexpected type requested so return null
      return null;
    }

    private static object ConstructInstanceDescriptor()
    {
      ConstructorInfo ci = typeof( KeyModifierCollection ).GetConstructor( new Type[] { } );
      return new InstanceDescriptor( ci, new Object[] { } );
    }
  }
}
