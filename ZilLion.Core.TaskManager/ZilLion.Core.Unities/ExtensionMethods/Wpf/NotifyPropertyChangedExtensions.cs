using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ZilLion.Core.Unities.ExtensionMethods.Wpf
{
    /// <summary>
    ///     The notify property changed extensions.
    /// </summary>
    public static partial class Extensions

    {
        #region Public Methods

        /// <summary>
        ///     Raises the delegate for the property identified by a lambda expression.
        /// </summary>
        /// <typeparam name="TObject">The type of object containing the property.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="handler">The delegate to raise. If this parameter is null, then no action is taken.</param>
        /// <param name="sender">The object raising this event.</param>
        /// <param name="expression">The lambda expression identifying the property that changed.</param>
        public static void Raise<TObject, TProperty>(
            this PropertyChangedEventHandler handler,
            TObject sender,
            Expression<Func<TObject, TProperty>> expression)
        {
            handler?.Invoke(sender, new PropertyChangedEventArgs(sender.GetPropertyNameByExpression<TObject, TProperty>(expression)



            ));
        }

        /// <summary>
        ///     Raises the delegate for the items property (with the name "Items[]").
        /// </summary>
        /// <param name="handler">The delegate to raise. If this parameter is null, then no action is taken.</param>
        /// <param name="sender">The object raising this event.</param>
        public static void RaiseItems(this PropertyChangedEventHandler handler, object sender)
        {
            if (handler != null)
            {
                handler(sender, new PropertyChangedEventArgs("Items[]"));
            }
        }

        /// <summary>
        ///     Subscribes a handler to the <see cref="INotifyPropertyChanged.PropertyChanged" /> event for a specific property.
        /// </summary>
        /// <typeparam name="TObject">The type implementing <see cref="INotifyPropertyChanged" />.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="source">The object implementing <see cref="INotifyPropertyChanged" />.</param>
        /// <param name="expression">The lambda expression selecting the property.</param>
        /// <param name="handler">The handler that is invoked when the property changes.</param>
        /// <returns>The actual delegate subscribed to <see cref="INotifyPropertyChanged.PropertyChanged" />.</returns>
        public static PropertyChangedEventHandler SubscribeToPropertyChanged<TObject, TProperty>(
            this TObject source,
            Expression<Func<TObject, TProperty>> expression,
            Action<TObject> handler)
            where TObject : INotifyPropertyChanged
        {
            // This is similar but not identical to:
            var propertyValue = source.GetPropertyNameByExpression(expression);
            PropertyChangedEventHandler ret = (s, e) =>
            {
                if (e.PropertyName == propertyValue)
                {
                    handler(source);
                }
            };
            source.PropertyChanged += ret;
            return ret;
        }

        /// <summary>
        ///     The notify.
        /// </summary>
        /// <param name="notifier">
        ///     The notifier.
        /// </param>
        /// <param name="propertySelector">
        ///     The property selector.
        /// </param>
        /// <typeparam name="TValue">
        /// </typeparam>
        public static void Notify<TValue>(this Action<string> notifier, Expression<Func<TValue>> propertySelector)
        {
            if (notifier != null)
            {
                var memberExpression = propertySelector.Body as MemberExpression;
                if (memberExpression != null)
                {
                    notifier(memberExpression.Member.Name);
                }
            }
        }

        /// <summary>
        ///     The notify property changed.
        /// </summary>
        /// <param name="target">
        ///     The target.
        /// </param>
        /// <param name="propertySelector">
        ///     The property selector.
        /// </param>
        /// <param name="notifier">
        ///     The notifier.
        /// </param>
        /// <typeparam name="TValue">
        /// </typeparam>
        public static void NotifyPropertyChanged<TValue>(
            this object target, Expression<Func<TValue>> propertySelector, Action<string> notifier)
        {
            if (notifier != null)
            {
                var memberExpression = propertySelector.Body as MemberExpression;
                if (memberExpression != null)
                {
                    notifier(memberExpression.Member.Name);
                }
            }
        }

        /// <summary>
        ///     The raise.
        /// </summary>
        /// <param name="handler">
        ///     The handler.
        /// </param>
        /// <param name="propertySelector">
        ///     The property selector.
        /// </param>
        /// <typeparam name="TValue">
        /// </typeparam>
        public static void Raise<TValue>(
            this PropertyChangedEventHandler handler, Expression<Func<TValue>> propertySelector)
        {
            if (handler != null)
            {
                var memberExpression = propertySelector.Body as MemberExpression;
                if (memberExpression != null)
                {
                    var sender = ((ConstantExpression) memberExpression.Expression).Value;
                    handler(sender, new PropertyChangedEventArgs(memberExpression.Member.Name));
                }
            }
        }

        #endregion
    }
}
