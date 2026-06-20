using System.Reactive.Concurrency;

namespace ViewModelServices
{
    public static class SchedulerExtensions
    {
        /// <summary>
        /// Esegue una funzione (come la risoluzione di Splat) sul Main Thread della UI 
        /// e restituisce il risultato in modo asincrono tramite un Task.
        /// </summary>
        public static Task<T> RunOnMainThread<T>(this IScheduler scheduler, Func<T> action)
        {
            var tcs = new TaskCompletionSource<T>();

            scheduler.Schedule(() =>
            {
                try
                {
                    tcs.SetResult(action());
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}
