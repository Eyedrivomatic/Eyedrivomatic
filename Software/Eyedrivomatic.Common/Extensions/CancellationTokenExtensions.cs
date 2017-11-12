using System.Threading;
using System.Threading.Tasks;

namespace Eyedrivomatic.Infrastructure.Extensions
{
    public static class CancellationTokenExtensions
    {
        public static Task AsTask(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(() => tcs.SetCanceled());
            return tcs.Task;
        }

        public static Task<T> AsTask<T>(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>();
            cancellationToken.Register(() => tcs.SetCanceled());
            return tcs.Task;
        }

    }


}
