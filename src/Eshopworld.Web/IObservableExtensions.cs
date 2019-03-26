using System;
using System.Reactive.Linq;
using Eshopworld.Core;
using Microsoft.AspNetCore.Builder;

namespace Eshopworld.Web
{
    /// <summary>
    /// extension methods for <see cref="IObservable{BaseNotification}"/>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IObservableExtensions
    {
        /// <summary>
        /// subscribe to notification
        /// </summary>
        /// <typeparam name="TNotification">type of notification (derived of <see cref="BaseNotification"/>)</typeparam>
        /// <typeparam name="TService">type of service that consumes the notification</typeparam>
        /// <param name="observable">notification observable</param>
        /// <param name="appBuilder">app builder including <see cref="IServiceProvider"/></param>
        /// <param name="observingSelector">selector to denote delegate to call when broadcasting notifications</param>
        /// <returns><see cref="IObservable{T}"/> reference back for chaining</returns>
        public static IObservable<BaseNotification> SubscribeNotification<TNotification, TService>(this IObservable<BaseNotification> observable, IApplicationBuilder appBuilder,
            Func<TService, Action<TNotification>> observingSelector) where TNotification : BaseNotification where TService : class
        {
            observable
                .OfType<TNotification>()
                .Subscribe(observingSelector.Invoke(
                    (TService)appBuilder.ApplicationServices.GetService(typeof(TService))));

            return observable;
        }

        /// <summary>
        /// subscribe to notification with <see cref="Action{T}"/> delegate passed in
        /// </summary>
        /// <typeparam name="TNotification">type of notification (derived of <see cref="BaseNotification"/></typeparam>
        /// <param name="observable">notification observable</param>
        /// <param name="onNext">delegate to broadcast notifications to</param>
        /// <returns><see cref="IObservable{T}"/> reference back for chaining</returns>
        public static IObservable<BaseNotification> SubscribeNotification<TNotification>(this IObservable<BaseNotification> observable, Action<TNotification> onNext) where TNotification : BaseNotification
        {
            observable.OfType<TNotification>().Subscribe(onNext);

            return observable;
        }
    }
}
