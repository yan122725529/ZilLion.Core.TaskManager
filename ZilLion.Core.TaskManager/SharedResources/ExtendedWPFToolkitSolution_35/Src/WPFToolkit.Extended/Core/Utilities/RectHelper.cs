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
using System.Windows;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal static class RectHelper
  {
    public static Point Center( Rect rect )
    {
      return new Point( rect.Left + rect.Width / 2, rect.Top + rect.Height / 2 );
    }

    public static Nullable<Point> GetNearestPointOfIntersectionBetweenRectAndSegment( Rect rect, Segment segment, Point point )
    {
      Nullable<Point> result = null;
      double distance = double.PositiveInfinity;

      Segment leftIntersection = segment.Intersection( new Segment( rect.BottomLeft, rect.TopLeft ) );
      Segment topIntersection = segment.Intersection( new Segment( rect.TopLeft, rect.TopRight ) );
      Segment rightIntersection = segment.Intersection( new Segment( rect.TopRight, rect.BottomRight ) );
      Segment bottomIntersection = segment.Intersection( new Segment( rect.BottomRight, rect.BottomLeft ) );

      RectHelper.AdjustResultForIntersectionWithSide( ref result, ref distance, leftIntersection, point );
      RectHelper.AdjustResultForIntersectionWithSide( ref result, ref distance, topIntersection, point );
      RectHelper.AdjustResultForIntersectionWithSide( ref result, ref distance, rightIntersection, point );
      RectHelper.AdjustResultForIntersectionWithSide( ref result, ref distance, bottomIntersection, point );

      return result;
    }

    public static Rect GetRectCenteredOnPoint( Point center, Size size )
    {
      return new Rect( new Point( center.X - size.Width / 2, center.Y - size.Height / 2 ), size );
    }

    private static void AdjustResultForIntersectionWithSide( ref Nullable<Point> result, ref double distance, Segment intersection, Point point )
    {
      if( !intersection.IsEmpty )
      {
        if( intersection.Contains( point ) )
        {
          distance = 0;
          result = point;
          return;
        }

        double p1Distance = PointHelper.DistanceBetween( point, intersection.P1 );
        double p2Distance = double.PositiveInfinity;
        if( !intersection.IsPoint )
        {
          p2Distance = PointHelper.DistanceBetween( point, intersection.P2 );
        }

        if( Math.Min( p1Distance, p2Distance ) < distance )
        {
          if( p1Distance < p2Distance )
          {
            distance = p1Distance;
            result = intersection.P1;
          }
          else
          {
            distance = p2Distance;
            result = intersection.P2;
          }
        }
      }
    }
  }
}
