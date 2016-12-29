using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ZilLion.Core.Unities.ExtensionMethods.Wpf
{
    public static class SubscribeExtension
    {
        public static void SubscribeNotify(this object @obj, PropertyChangedEventHandler propertyChangedEventHandler)
        {
            @obj.SaftyInvoke<INotifyPropertyChanged>(notifyInfo =>
            {
                notifyInfo.PropertyChanged += propertyChangedEventHandler;
                //notifyInfo.PropertyChanged += WeakPropertyChangedEventHandlerFactory.MakeWeak(propertyChangedEventHandler, (handler, param) =>
                //{
                //    notifyInfo.PropertyChanged -= handler;
                //}, null);
            });
        }

        public static void RemoveNotify(this object obj, PropertyChangedEventHandler propertyChangedEventHandler)
        {
            obj.SaftyInvoke<INotifyPropertyChanged>(notifyInfo =>
            {
                notifyInfo.PropertyChanged -= propertyChangedEventHandler;
            });
        }

        public static void SubscribeCollectionNotify<T>(this ObservableCollection<T> obj, NotifyCollectionChangedEventHandler collectionChangeEventHandler)
        {
            obj.SaftyInvoke(notifyInfo =>
            {
                notifyInfo.CollectionChanged += collectionChangeEventHandler;
                //notifyInfo.CollectionChanged += WeakNotifyCollectionChangedEventHandlerFactory.MakeWeak(collectionChangeEventHandler, (handler, param) =>
                //{
                //    notifyInfo.CollectionChanged -= handler;
                //}, null);
            });
        }

        public static void RemoveCollectionNotify<T>(this ObservableCollection<T> obj, NotifyCollectionChangedEventHandler collectionChangeEventHandler)
        {
            obj.CollectionChanged -= collectionChangeEventHandler;
        }

        public static void SubscribeCollectionNotify<T>(this IList<T> obj, NotifyCollectionChangedEventHandler collectionChangeEventHandler)
        {
            obj.SaftyInvoke<ObservableCollection<T>>(obColl => obColl.SubscribeCollectionNotify(collectionChangeEventHandler));
        }

        public static void RemoveCollectionNotify<T>(this IList<T> obj, NotifyCollectionChangedEventHandler collectionChangeEventHandler)
        {
            obj.SaftyInvoke<ObservableCollection<T>>(obColl => obColl.RemoveCollectionNotify(collectionChangeEventHandler));
        }

        public static void SubscribeCollectionNotify(this ICollectionView obj, NotifyCollectionChangedEventHandler collectionChangeEventHandler)
        {
            if (obj==null) return;
            obj.CollectionChanged += collectionChangeEventHandler;
            //obj.CollectionChanged += WeakNotifyCollectionChangedEventHandlerFactory.MakeWeak(collectionChangeEventHandler, (handler, param) =>
            //{
            //    obj.CollectionChanged -= handler;
            //}, null);
        }

        public static void RemoveCollectionNotify(this ICollectionView obj, NotifyCollectionChangedEventHandler collectionChangeEventHandler)
        {
            obj.CollectionChanged -= collectionChangeEventHandler;
        }
    }
}