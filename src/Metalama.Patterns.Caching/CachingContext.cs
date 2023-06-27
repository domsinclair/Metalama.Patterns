// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Patterns.Caching.Dependencies;
using Metalama.Patterns.Contracts;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Metalama.Patterns.Caching;

// Needs to be [Serializable] and derived from MarshalByRefObject because we are using CallContext in .NET 4.5.
// No serialization actually occurs, but CallContext requires this.
[Serializable]
internal sealed class CachingContext : MarshalByRefObject, IDisposable, ICachingContext
{
    private static readonly AsyncLocal<ICachingContext> _currentContext = new();

    public static ICachingContext Current
    {
        get { return _currentContext.Value ?? (_currentContext.Value = new NullCachingContext()); }
        internal set { _currentContext.Value = value; }
    }

    private readonly string _key;
    private bool _disposed;
    private readonly object _dependenciesSync = new();
    private HashSet<string> _dependencies;

    [SuppressMessage( "Microsoft.Usage", "CA2235:MarkAllNonSerializableFields", Justification = "Not really serialized." )]
    private ImmutableHashSet<string> _immutableDependencies;

    private CachingContext() { }

    private CachingContext( string key, CachingContextKind options, ICachingContext parent )
    {
        this._key = key;
        this.Kind = options;
        this.Parent = parent;
    }

    public CachingContextKind Kind { get; }

    internal ImmutableHashSet<string> Dependencies
    {
        get
        {
            if ( this._immutableDependencies == null )
            {
                lock ( this._dependenciesSync )
                {
                    if ( this._dependencies == null )
                    {
                        this._immutableDependencies = ImmutableHashSet<string>.Empty;
                    }
                    else
                    {
                        this._immutableDependencies = this._dependencies.ToImmutableHashSet();
                    }
                }
            }

            return this._immutableDependencies;
        }
    }

    public ICachingContext Parent { get; }

    public static CachingContext OpenRecacheContext( string key )
    {
        var context = new CachingContext( key, CachingContextKind.Recache, Current );
        Current = context;

        return context;
    }

    internal static CachingContext OpenCacheContext( string key )
    {
        var context = new CachingContext( key, CachingContextKind.Cache, Current );
        Current = context;

        return context;
    }

    internal static SuspendedCachingContext OpenSuspendedCacheContext()
    {
        var context = new SuspendedCachingContext( Current );
        Current = context;

        return context;
    }

    public void AddDependency( [Required] ICacheDependency dependency )
    {
        if ( string.IsNullOrEmpty( this._key ) )
        {
            throw new InvalidOperationException( "This method can be invoked only in the context of a cached method." );
        }

        if ( this._disposed )
        {
            this.Parent?.AddDependency( dependency );

            return;
        }

        lock ( this._dependenciesSync )
        {
            this.PrepareAddDependency();
            this._dependencies.Add( dependency.GetCacheKey() );
        }
    }

    private void PrepareAddDependency()
    {
        if ( this._dependencies == null )
        {
            this._dependencies = new HashSet<string>();
        }

        this._immutableDependencies = null;
    }

    public void AddDependency( [Required] object dependency )
    {
        switch ( dependency )
        {
            case ICacheDependency cacheDependency:
                this.AddDependency( cacheDependency );

                return;

            case string str:
                this.AddDependency( str );

                return;

            default:
                this.AddDependency( new ObjectDependency( dependency ) );

                return;
        }
    }

    public void AddDependency( [Required] string dependency )
    {
        this.AddDependency( new StringDependency( dependency ) );

        if ( this._disposed )
        {
            this.Parent?.AddDependency( dependency );

            return;
        }
    }

    public void AddDependencies( IEnumerable<ICacheDependency> dependencies )
    {
        if ( string.IsNullOrEmpty( this._key ) )
        {
            throw new InvalidOperationException( "This method can be invoked only in the context of a cached method." );
        }

        if ( this._disposed )
        {
            this.Parent?.AddDependencies( dependencies );

            return;
        }

        if ( dependencies != null )
        {
            lock ( this._dependenciesSync )
            {
                this.PrepareAddDependency();

                foreach ( var dependency in dependencies )
                {
                    this._dependencies.Add( dependency.GetCacheKey() );
                }
            }
        }
    }

    public void AddDependencies( IEnumerable<string> dependencies )
    {
        if ( this._disposed )
        {
            this.Parent?.AddDependencies( dependencies );

            return;
        }

        if ( dependencies != null )
        {
            lock ( this._dependenciesSync )
            {
                this.PrepareAddDependency();

                foreach ( var dependency in dependencies )
                {
                    this._dependencies.Add( dependency );
                }
            }
        }
    }

    internal void AddDependenciesToParent( MethodInfo method )
    {
        if ( this.Parent != null )
        {
            this.Parent.AddDependencies( this.Dependencies );

            if ( CachingServices.DefaultBackend.SupportedFeatures.Dependencies )
            {
                this.Parent.AddDependency( this._key );
            }
            else
            {
                CachingServices.Invalidation.AddedNestedCachedMethod( method );
            }
        }
    }

    public void Dispose()
    {
        if ( Current != this )
        {
            throw new InvalidOperationException( "Only the current context can be disposed." );
        }

        this._disposed = true;

        Current = this.Parent;
    }
}