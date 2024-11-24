using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Beavior
{
    internal class CollectionViewScrollBehavior : Behavior<CollectionView>
    {
        private CollectionView _collectionView;
        private IList _previousCollection;

        protected override void OnAttachedTo(CollectionView bindable)
        {
            base.OnAttachedTo(bindable);
            _collectionView = bindable;

            if (_collectionView.ItemsSource is INotifyCollectionChanged observableCollection)
            {
                observableCollection.CollectionChanged += OnCollectionChanged;
            }
        }

        protected override void OnDetachingFrom(CollectionView bindable)
        {
            if (_collectionView.ItemsSource is INotifyCollectionChanged observableCollection)
            {
                observableCollection.CollectionChanged -= OnCollectionChanged;
            }
            _collectionView = null;
            base.OnDetachingFrom(bindable);
        }

        private async void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add || _collectionView == null)
                return;

            if (_collectionView.ItemsSource is IList items && items.Count > 0)
            {
                try
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        // 少し待機してスクロールを実行（UIの更新を待つため）
                        await Task.Delay(100);
                        _collectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.End, animate: true);
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Scroll failed: {ex.Message}");
                }
            }
        }
    }
}
