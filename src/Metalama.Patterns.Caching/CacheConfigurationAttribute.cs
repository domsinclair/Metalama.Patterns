﻿// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using PostSharp.Patterns.Caching.Implementation;

namespace PostSharp.Patterns.Caching
{
    /// <summary>
    /// Custom attribute that, when applied on a type, configures the <see cref="CacheAttribute"/> aspects applied to the methods of this type
    /// or its derived types. When applied to an assembly, the <see cref="CacheConfigurationAttribute"/> custom attribute configures all methods
    /// of the current assembly.
    /// </summary>
    /// <remarks>
    /// <para>Any <see cref="CacheConfigurationAttribute"/> on the base class has always priority over a <see cref="CacheConfigurationAttribute"/>
    /// on the assembly, even if the base class is in a different assembly.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Assembly )]
    public sealed class CacheConfigurationAttribute : Attribute
    {

        internal CacheItemConfiguration Configuration { get; } = new CacheItemConfiguration();

        /// <summary>
        /// Gets or sets the name of the <see cref="CachingProfile"/>  that contains the configuration of the cached methods.
        /// </summary>
        public string ProfileName
        {
            get { return this.Configuration.ProfileName; }
            set { this.Configuration.ProfileName = value; }
        }

        /// <summary>
        /// Determines whether the method calls are automatically reloaded (by re-evaluating the target method with the same arguments)
        /// when the cache item is removed from the cache.
        /// </summary>
        public bool AutoReload
        {
            get { return this.Configuration.AutoReload.GetValueOrDefault(); }
            set { this.Configuration.AutoReload = value; }
        }

        /// <summary>
        /// Gets or sets the total duration, in minutes, during which the result of the cached method  is stored in cache. The absolute
        /// expiration time is counted from the moment the method is evaluated and cached.
        /// </summary>
        public double AbsoluteExpiration
        {
            get { return this.Configuration.AbsoluteExpiration.GetValueOrDefault(TimeSpan.Zero).TotalMinutes; }
            set { this.Configuration.AbsoluteExpiration = TimeSpan.FromMinutes(value); }
        }

        /// <summary>
        /// Gets or sets the duration, in minutes, during which the result of the cached method is stored in cache after it has been
        /// added to or accessed from the cache. The expiration is extended every time the value is accessed from the cache.
        /// </summary>
        public double SlidingExpiration
        {
            get { return this.Configuration.SlidingExpiration.GetValueOrDefault(TimeSpan.Zero).TotalMinutes; }
            set { this.Configuration.SlidingExpiration = TimeSpan.FromMinutes(value); }
        }

        /// <summary>
        /// Gets or sets the priority of the cached method.
        /// </summary>
        public CacheItemPriority Priority
        {
            get { return this.Configuration.Priority.GetValueOrDefault(CacheItemPriority.Default); }
            set { this.Configuration.Priority = value; }
        }

        /// <summary>
        /// Determines whether the <c>this</c> instance should be a part of the cache key. The default value of this property is <c>false</c>,
        /// which means that by default the <c>this</c> instance is a part of the cache key.
        /// </summary>
        public bool IgnoreThisParameter
        {
            get { return this.Configuration.IgnoreThisParameter.GetValueOrDefault(); }
            set { this.Configuration.IgnoreThisParameter = value; }
        }
  
    }
}