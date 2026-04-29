using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace IVSoftware.Portable.Internal
{
    /// <summary>
    /// A lightweight dictionary whose indexer tolerates missing keys by returning null semantics instead of throwing.
    /// </summary>
    /// <remarks>
    /// This internal helper mirrors the tolerant read behavior of the canonical
    /// <c>TolerantDictionary</c> hosted in <c>IVSoftware.Portable.Collections</c>,
    /// but without introducing a dependency on that package.
    ///
    /// Accessing a key that does not exist returns <c>default</c> through a nullable
    /// projection (<c>TValue?</c>) rather than throwing <see cref="KeyNotFoundException"/>.
    /// The underlying storage remains a normal <see cref="Dictionary{TKey,TValue}"/>; only
    /// the indexer access semantics are relaxed.
    ///
    /// When <typeparamref name="TValue"/> is a value type, the indexer surface exposes it
    /// as <c>Nullable&lt;TValue&gt;</c>, allowing callers to distinguish between "key not present"
    /// (<c>null</c>) and an actual stored value.
    ///
    /// Assigning <c>null</c> is permitted only when <typeparamref name="TValue"/> itself
    /// supports null (reference types or nullable value types). If <typeparamref name="TValue"/>
    /// is a non-nullable value type, attempting to assign <c>null</c> triggers a hard exception,
    /// since the underlying dictionary cannot represent that state.
    ///
    /// This implementation exists solely as a minimal internal utility when reduced tolerant
    /// lookup behavior is sufficient in libraries that must remain dependency-light.
    /// </remarks>
    internal class TolerantDictionary<TKey, TValue> where TKey : notnull
    {
        public TolerantDictionary()
        {
            AsReadOnly = new(@base);
        }
        Dictionary<TKey, TValue> @base = new();
        public IEnumerable<TKey> Keys => @base.Keys;
        public IEnumerable<TValue> Values => @base.Values;
        public ReadOnlyDictionary<TKey, TValue> AsReadOnly { get; }

        [Indexer]
        public TValue? this[TKey key]
        {
            get
            {
                if (!@base.TryGetValue(key, out TValue? value))
                {
                    return default!;
                }
                return value;
            }
            /// <summary>
            /// Throws when a null assignment is attempted for a dictionary whose TValue is a non-nullable value type.
            /// </summary>
            /// <remarks>
            /// The tolerant indexer allows missing keys to return null semantics via <c>TValue?</c>.
            /// However, if <typeparamref name="TValue"/> is a non-nullable value type (e.g. <c>int</c>, <c>bool</c>, <c>DateTime</c>),
            /// assigning <c>null</c> cannot be represented in the underlying <see cref="Dictionary{TKey,TValue}"/> storage.
            /// This guard prevents that invalid assignment and signals the mismatch between the nullable indexer contract
            /// and the concrete storage type.
            /// </remarks>
            set
            {
                if (value is null
                    && typeof(TValue).IsValueType
                    && Nullable.GetUnderlyingType(typeof(TValue)) is null)
                {
                    this.ThrowHard<InvalidOperationException>(
                        "because {nameof(TValue)} ({typeof(TValue).Name}) is a non-nullable value type.");
                    // This must return, even if the Throw is deescalated.
                    return;
                }
                CollectionChangingEventArgs ePre;
                NotifyCollectionChangedEventArgs ePost;
                if (@base.TryGetValue(key, out TValue? current) && current is not null)
                {
                    if (!Equals(current, value))
                    {
                        ePre = new CollectionChangingEventArgs(
                            CollectionChangingAction.Replace,
                            oldValue: new(key, current),
                            newValue: new DictionaryEntry(key, value));
                        CollectionChanging?.Invoke(this, ePre);
                        if (ePre.Cancel) return;
                        @base[key] = value!;

                        ePost = new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Replace,
                            newItem: new DictionaryEntry(key!, value),
                            oldItem: new DictionaryEntry(key!, current));

                        OnCollectionChanged(ePost);
                    }
                }
                else
                {
                    ePre = new CollectionChangingEventArgs(
                        CollectionChangingAction.Replace,
                        oldValue: null,
                        newValue: new DictionaryEntry(key, value));
                    CollectionChanging?.Invoke(this, ePre);
                    if (ePre.Cancel) return;
                    @base[key] = value!;

                    ePost = new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        changedItem: new DictionaryEntry(key!, value));

                    OnCollectionChanged(ePost);
                }
            }
        }
        public bool Remove(TKey key)
        {
            if (@base.TryGetValue(key, out TValue? current) && current is not null)
            {
                var ePre = new CollectionChangingEventArgs(
                    CollectionChangingAction.Remove,
                    oldValue: new DictionaryEntry(key, current),
                    newValue: null);

                CollectionChanging?.Invoke(this, ePre);
                if (ePre.Cancel)
                {
                    return false;
                }

                @base.Remove(key);

                var ePost = new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    changedItem: new DictionaryEntry(key, current));

                OnCollectionChanged(ePost);

                return true;
            }
            else
            {
                return false;
            }
        }
        public void Clear()
        {
            @base.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset));
        }
        public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs ePost)
        {
            CollectionChanged?.Invoke(this, ePost);
        }

        public bool TryGetValue(TKey key, out TValue? value) => @base.TryGetValue(key, out value);

        public event EventHandler<CollectionChangingEventArgs>? CollectionChanging;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
    }

    internal class CollectionChangingEventArgs : CancelEventArgs
    {
        public CollectionChangingEventArgs(CollectionChangingAction action, DictionaryEntry? oldValue, DictionaryEntry? newValue)
        {
            Action = action;
            OldValue = oldValue;
            NewValue = newValue;
        }
        public CollectionChangingAction Action { get; }
        public DictionaryEntry? OldValue { get; }
        public DictionaryEntry? NewValue { get; }
    }
    internal enum CollectionChangingAction
    {
        Add = NotifyCollectionChangedAction.Add,
        Remove = NotifyCollectionChangedAction.Remove,
        Replace = NotifyCollectionChangedAction.Replace,
        Move = NotifyCollectionChangedAction.Move,
        Reset = NotifyCollectionChangedAction.Reset,
    }
}
