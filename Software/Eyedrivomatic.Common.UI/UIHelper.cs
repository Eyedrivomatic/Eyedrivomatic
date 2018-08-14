//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


using System.Windows;
using System.Windows.Media;

namespace Eyedrivomatic.Common.UI
{
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
            // get parent item
            var parentObject = VisualTreeHelper.GetParent(child);

            // we’ve reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            var parent = parentObject as T;
            return parent ?? FindVisualParent<T>(parentObject);
        }
    }
}