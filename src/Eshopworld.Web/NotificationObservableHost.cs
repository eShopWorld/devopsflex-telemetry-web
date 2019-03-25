using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Eshopworld.Web
{
    public class NotificationObservableHost : IDisposable
    {
        //so the requirement here is hot broadcasting (Rx) entity - observable observer -
        //and out of the box subjects fits the bill (most if not all of its code would exist in custom implementation)
        //if performance becomes concern, custom implementation may be created
        private readonly Subject<object> _observable = new Subject<object>();

        /// <summary>
        /// new event to be ingested
        /// </summary>
        /// <param name="event">event instance</param>
        public void NewEvent(object @event)
        {
            if (@event != null)
            {
                _observable.OnNext(@event);
            }
        }

        /// <summary>
        /// shortcut for subscribing to everything
        /// telemetry purposes etc.
        /// </summary>
        /// <param name="subscriber">observing delegate</param>
        public void SubscribeToAll(Action<object> subscriber)
        {
            Subscribe<object>(subscriber);
        }

        /// <summary>
        /// subscribe to specific types (and indeed subtypes)
        /// </summary>
        /// <typeparam name="T">target type to subscribe</typeparam>
        /// <param name="subscriber">observing delegate</param>
        public void Subscribe<T>(Action<object> subscriber) where T : class
        {
            if (subscriber != null)
            {              
                _observable.OfType<T>().Subscribe(subscriber);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// dispose of container and subscriptions
        /// </summary>
        /// <param name="disposing">true if disposing through GC, false if through the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {                      
            _observable.Dispose(); //this internally disposes of subject AND subscriptions
        }
    }
}
