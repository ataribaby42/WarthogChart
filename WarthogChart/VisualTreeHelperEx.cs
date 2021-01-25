using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WarthogChart
{
    public static class VisualTreeHelperEx
    {
        public static T FindVisualAncestorByType<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null) return default(T);
            if (dependencyObject is T) return (T)dependencyObject;
            T parent = default(T);
            parent = FindVisualAncestorByType<T>(VisualTreeHelper.GetParent(dependencyObject));
            return parent;
        }

        public static T FindVisualChildByType<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null) return default(T);
            if (dependencyObject is T) return (T)dependencyObject;
            T child = default(T);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                child = FindVisualChildByType<T>(VisualTreeHelper.GetChild(dependencyObject, i));
                if (child != null) return child;
            }
            return null;
        }

        public static List<T> FindVisualChildrenByType<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            List<T> children = new List<T>();

            if (dependencyObject is T)
            {
                children.Add((T)dependencyObject);
                return children;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                children.AddRange(FindVisualChildrenByType<T>(VisualTreeHelper.GetChild(dependencyObject, i)));
            }
            return children;
        }


        public static T FindVisualParentByType<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null) return default(T);

            T parent = default(T);
            parent = FindVisualAncestorByType<T>(VisualTreeHelper.GetParent(dependencyObject));
            return parent;
        }

    }

}
