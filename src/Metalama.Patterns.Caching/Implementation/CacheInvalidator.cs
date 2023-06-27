// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using PostSharp.Patterns.Contracts;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Utilities;

namespace PostSharp.Patterns.Caching.Implementation
{
    /// <summary>
    /// Base class for a kind of <see cref="CachingBackendEnhancer"/> that allows several instances of the same application to use
    /// a local cache, and synchronize themselves by sending invalidation messages over a publish/subscribe channel.
    /// </summary>
    public abstract class CacheInvalidator : CachingBackendEnhancer
    {
        private readonly BackgroundTaskScheduler backgroundTaskScheduler = new BackgroundTaskScheduler();

        /// <summary>
        /// Gets the options of the current <see cref="CacheInvalidator"/>.
        /// </summary>
        public CacheInvalidatorOptions Options { get; }

        /// <summary>
        /// Initializes a new <see cref="CachingBackend"/>/
        /// </summary>
        /// <param name="underlyingBackend">The underlying <see cref="CachingBackend"/> (typically an in-memory cache).</param>
        /// <param name="options">Options of the new <see cref="CacheInvalidator"/>.</param>
        protected CacheInvalidator([Required] CachingBackend underlyingBackend, [Required] CacheInvalidatorOptions options) : base(underlyingBackend)
        {
            this.Options = options;
        }

        /// <inheritdoc />
        protected override async Task RemoveItemAsyncCore(string key, CancellationToken cancellationToken)
        {
            await base.RemoveItemAsyncCore(key, cancellationToken);

             this.backgroundTaskScheduler.EnqueueBackgroundTask(() => this.PublishInvalidationAsync(key, CacheKeyKind.Item, CancellationToken.None));
        }

        /// <inheritdoc />
        protected override void RemoveItemCore(string key)
        {
            base.RemoveItemCore(key);

            this.backgroundTaskScheduler.EnqueueBackgroundTask(() => this.PublishInvalidationAsync(key, CacheKeyKind.Item, CancellationToken.None));
        }

        /// <inheritdoc />
        protected override async Task InvalidateDependencyAsyncCore(string key, CancellationToken cancellationToken)
        {
            await base.InvalidateDependencyAsyncCore(key, cancellationToken);

            this.backgroundTaskScheduler.EnqueueBackgroundTask(() => this.PublishInvalidationAsync(key, CacheKeyKind.Dependency, CancellationToken.None));

        }

        /// <inheritdoc />
        protected override void InvalidateDependencyCore(string key)
        {
            base.InvalidateDependencyCore(key);

            this.backgroundTaskScheduler.EnqueueBackgroundTask(() => this.PublishInvalidationAsync(key, CacheKeyKind.Dependency, CancellationToken.None));
        }

        /// <summary>
        /// Implementations of <see cref="CacheInvalidator"/> must call this method when an invalidation message is received.
        /// </summary>
        /// <param name="message">The serialized invalidation message.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        protected void OnMessageReceived([Required] string message)
        {
            StringTokenizer tokenizer = new StringTokenizer(message);
            string prefix = tokenizer.GetNext();

            if (prefix != this.Options.Prefix)
            {
                return;
            }

            LogActivity activity = this.Logger.OpenActivity("{This} is processing the message {Message}.", this, message);
            try
            {


                string kind = tokenizer.GetNext();
                string backendIdStr = tokenizer.GetNext();
                string key = tokenizer.GetRest();

                Guid sourceId;
                if (!Guid.TryParse(backendIdStr, out sourceId))
                {
                    activity.SetFailure("Cannot parse the SourceId '{SourceId}' into a Guid. Skipping the event.", backendIdStr);
                    return;
                }


                // We use synchronous APIs because most the typical consumer of InvalidationBroker is synchronous.

                if (sourceId == this.UnderlyingBackend.Id)
                {
                    activity.SetSuccess("Skipped the message {Message} because it has sent it itself.", message);
                    return;
                }


                switch (kind)
                {
                    case "dependency":
                        this.UnderlyingBackend.InvalidateDependency(key);
                        activity.SetSuccess("Invalidated the dependency {Key}.", key);
                        break;

                    case "item":
                        this.UnderlyingBackend.RemoveItem(key);
                        activity.SetSuccess("Removed the item {Key}.", key);
                        break;

                    default:
                        activity.SetFailure("Invalid kind key: {Kind}.", kind);
                        break;
                }

            }
            catch (Exception e)
            {
                activity.SetException(e);
            }

        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private string GetMessage(string kind, string key)
        {
            return this.Options.Prefix + ":" + kind.ToLowerInvariant() + ":" + this.UnderlyingBackend.Id + ":" + key;
        }



        private Task PublishInvalidationAsync(string key, CacheKeyKind cacheKeyKind, CancellationToken cancellationToken)
        {
            string message = this.GetMessage(cacheKeyKind, key);

            this.Logger.Write( LogLevel.Debug, "{This} is sending the message {Message}.", this, message );

            return this.SendMessageAsync(message, cancellationToken);
        }


        private string GetMessage(CacheKeyKind cacheKeyKind, string key)
        {
            switch (cacheKeyKind)
            {
                case CacheKeyKind.Item:
                    return this.GetMessage("item", key);

                case CacheKeyKind.Dependency:
                    return this.GetMessage("dependency", key);

                default:
                    throw new ArgumentOutOfRangeException(nameof(cacheKeyKind), cacheKeyKind, null);
            }
        }

        /// <summary>
        /// Sends an invalidation message over the message bus of the implementation.
        /// </summary>
        /// <param name="message">A serialized, opaque serialization message.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        protected abstract Task SendMessageAsync(string message, CancellationToken cancellationToken);

        private enum CacheKeyKind
        {
            Item,
            Dependency
        }


    }


}
