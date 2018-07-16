using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace NsisoLauncher.Utils
{
    ///*
    // * Copyright thomaslevesque
    // * https://gist.github.com/thomaslevesque/10023516
    // */
    //public class AsyncObservableCollection<T> : ObservableCollection<T>
    //{
    //    private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

    //    public AsyncObservableCollection() { }

    //    public AsyncObservableCollection(IEnumerable<T> list) : base(list) { }

    //    private void ExecuteOnSyncContext(Action action)
    //    {
    //        if (SynchronizationContext.Current == _synchronizationContext)
    //        {
    //            action();
    //        }
    //        else
    //        {
    //            _synchronizationContext.Send(_ => action(), null);
    //        }
    //    }

    //    protected override void InsertItem(int index, T item) { ExecuteOnSyncContext(() => base.InsertItem(index, item)); }

    //    protected override void RemoveItem(int index) { ExecuteOnSyncContext(() => base.RemoveItem(index)); }

    //    protected override void SetItem(int index, T item) { ExecuteOnSyncContext(() => base.SetItem(index, item)); }

    //    protected override void MoveItem(int oldIndex, int newIndex) { ExecuteOnSyncContext(() => base.MoveItem(oldIndex, newIndex)); }

    //    protected override void ClearItems() { ExecuteOnSyncContext(() => base.ClearItems()); }
    //}

    /// <summary>
    ///     The async observable collection.
    /// </summary>
    /// <typeparam>
    ///     <name>T</name>
    /// </typeparam>
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        #region Public Events

        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Methods

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            //NotifyCollectionChangedEventHandler CollectionChanged = this.CollectionChanged;
            if (CollectionChanged != null)
            {
                foreach (NotifyCollectionChangedEventHandler nh in CollectionChanged.GetInvocationList())
                {
                    var dispObj = nh.Target as DispatcherObject;
                    if (dispObj != null)
                    {
                        if (dispObj.Dispatcher != null && !dispObj.Dispatcher.CheckAccess())
                        {
                            dispObj.Dispatcher.BeginInvoke(
                                (Action)(() => nh.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))),
                                DispatcherPriority.DataBind);
                            continue;
                        }
                    }
                    nh.Invoke(this, e);
                }
            }
            //CollectionChanged?.Invoke(this, e);
        }

        #endregion
    }
}
