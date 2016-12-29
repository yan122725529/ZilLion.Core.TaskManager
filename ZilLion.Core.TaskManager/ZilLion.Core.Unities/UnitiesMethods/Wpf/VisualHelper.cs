using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ZilLion.Core.Unities.UnitiesMethods.Wpf
{
    public static class VisualHelper
    {
       

       

      

        #region 方法

        /// <summary>
        ///     遍历视觉树向上查找目标propertyToFind=propertyValue的DependencyObject
        /// </summary>
        /// <param name="root">查找的元素</param>
        /// <param name="propertyToFind">查找的属性</param>
        /// <param name="propertyValue">对应属性的值</param>
        /// <returns></returns>
        public static DependencyObject GetVisualParentByProperty(DependencyObject root, DependencyProperty propertyToFind, object propertyValue)
        {
            while (true)
            {
                var dpParent = VisualTreeHelper.GetParent(root);
                if (dpParent == null)
                {
                    return null;
                }
                if (dpParent.GetValue(propertyToFind).Equals(propertyValue))
                {
                    return dpParent;
                }
                root = dpParent;
            }
        }

        /// <summary>
        ///     遍历视觉树向下查找目标propertyToFind=propertyValue的DependencyObject
        /// </summary>
        /// <param name="root">查找的元素</param>
        /// <param name="propertyToFind">查找的属性</param>
        /// <param name="propertyValue">对应属性的值</param>
        /// <returns></returns>
        public static DependencyObject GetVisualChildByProperty(DependencyObject root, DependencyProperty propertyToFind,
            object propertyValue)
        {
            var iNum = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < iNum; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child.GetValue(propertyToFind).Equals(propertyValue))
                {
                    return child;
                }
                var nowChild = GetVisualChildByProperty(child, propertyToFind, propertyValue);
                if (nowChild != null)
                {
                    return nowChild;
                }
            }
            return null;
        }

        /// <summary>
        ///     遍历视觉树向上查找目标Name的DependencyObject
        /// </summary>
        /// <param name="root">查找的元素</param>
        /// <param name="strNameToFind">需要查找的Name</param>
        /// <returns></returns>
        public static DependencyObject GetVisualParentByName(DependencyObject root, string strNameToFind)
        {
            var dpParent = VisualTreeHelper.GetParent(root);
            if (dpParent == null)
            {
                return null;
            }
            return dpParent.GetValue(FrameworkElement.NameProperty).ToString().Equals(strNameToFind) ? dpParent : GetVisualParentByName(dpParent, strNameToFind);
        }

        /// <summary>
        ///     遍历视觉树查找目标Name的DependencyObject
        /// </summary>
        /// <param name="root">查找的元素</param>
        /// <param name="strNameToFind">需要查找的Name</param>
        /// <returns></returns>
        public static DependencyObject GetVisualChildByName(DependencyObject root, string strNameToFind)
        {
            var iNum = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < iNum; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child.GetValue(FrameworkElement.NameProperty).ToString().Equals(strNameToFind))
                {
                    return child;
                }
                var nowChild = GetVisualChildByName(child, strNameToFind);
                if (nowChild != null)
                {
                    return nowChild;
                }
            }
            return null;
        }

        /// <summary>
        ///     遍历逻辑树查找目标Name的DependencyObject
        /// </summary>
        /// <param name="root">查找的元素</param>
        /// <param name="strNameToFind">需要查找的Name</param>
        /// <returns></returns>
        public static DependencyObject GetChildByName(DependencyObject root, string strNameToFind)
        {
            var dp = LogicalTreeHelper.GetChildren(root);
            return dp.OfType<DependencyObject>().Select(dpChild => dpChild.DependencyObjectType.Name == strNameToFind ? dpChild : GetChildByName(dpChild, strNameToFind)).FirstOrDefault();
        }

        #endregion
    }
}