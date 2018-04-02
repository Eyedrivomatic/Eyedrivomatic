using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Eyedrivomatic.Infrastructure.Annotations;
using NullGuard;
using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic.Infrastructure
{
    public interface IProgressNotification<T> : INotification, IProgress<T>
    {
        T Progress { get; }
        event EventHandler OnDone;
    }

    public class ProgressNotification<T> : Notification, IProgressNotification<T>, INotifyPropertyChanged, IDisposable
    {
        private T _progress;
        private readonly IProgress<T> _innerProgress;

        public ProgressNotification(IProgress<T> innerProgress = null)
        {
            _innerProgress = innerProgress;
        }

        public void Report(T value)
        {
            Progress = value;
            _innerProgress?.Report(value);
        }

        public T Progress
        {
            get => _progress;
            private set
            {
                if (value.Equals(_progress)) return;
                _progress = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler OnDone;

        public void Dispose()
        {
            OnDone?.Invoke(this, EventArgs.Empty);
        }
    }
}