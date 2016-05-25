// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomaticis distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Media;


/// <summary>
/// Thanks to John Myczek http://stackoverflow.com/users/16176/john-myczek from answer at StackOverflow http://stackoverflow.com/a/636456/2529742
/// </summary>
public static class UIHelper
{
    /// <summary>
    /// Finds a parent of a given item on the visual tree.
    /// </summary>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <param name="child">A direct or indirect child of the queried item.</param>
    /// <returns>The first parent item that matches the submitted type parameter. 
    /// If not matching item can be found, a null reference is being returned.</returns>
    public static T FindVisualParent<T>(DependencyObject child)
      where T : DependencyObject
    {
        Contract.Requires<ArgumentNullException>(child != null, nameof(child));

        // get parent item
        DependencyObject parentObject = VisualTreeHelper.GetParent(child);

        // we’ve reached the end of the tree
        if (parentObject == null) return null;

        // check if the parent matches the type we’re looking for
        T parent = parentObject as T;
        if (parent != null)
        {
            return parent;
        }
        else
        {
            // use recursion to proceed with next level
            return FindVisualParent<T>(parentObject);
        }
    }
}