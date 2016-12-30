namespace Caliburn.Micro {
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///   Enables easy marshalling of code to the UI thread.
    /// </summary>
    public static class Execute {
        /// <summary>
        ///   Indicates whether or not the framework is in design-time mode.
        /// </summary>
        public static bool InDesignMode {
            get {
                return PlatformProvider.Current.InDesignMode;
            }
        }

        /// <summary>
        ///   Executes the action on the UI thread asynchronously.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void BeginOnUiThread(this Action action) {
            PlatformProvider.Current.BeginOnUiThread(action);
        }

        /// <summary>
        ///   Executes the action on the UI thread asynchronously.
        /// </summary>
        /// <param name = "action">The action to execute.</param>
        public static Task OnUiThreadAsync(this Action action) {
            return PlatformProvider.Current.OnUiThreadAsync(action);
        }

        /// <summary>
        ///   Executes the action on the UI thread.
        /// </summary>
        /// <param name = "action">The action to execute.</param>
        public static void OnUiThread(this Action action) {
            PlatformProvider.Current.OnUiThread(action);
        }
    }
}
