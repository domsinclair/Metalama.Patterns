﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Patterns.Caching.TestHelpers;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable MemberCanBeMadeStatic.Local
#pragma warning disable CA1822

namespace Metalama.Patterns.Caching.Tests
{
    public sealed class CacheConfigurationAttributeTests : BaseCachingTests
    {
        private const string _profileNamePrefix = "Caching.Tests.CacheConfigurationAttributeTests_";

        private const string _testCachingAttributeProfileName = _profileNamePrefix + "TestCachingAttribute";

        [CachingConfiguration( ProfileName = _testCachingAttributeProfileName )]
        private class BaseCachingClass
        {
            public sealed class InnerCachingClassInBase
            {
                [Cache]
                public object GetValueInnerBase()
                {
                    return null!;
                }

                [Cache]
                public async Task<object> GetValueInnerBaseAsync()
                {
                    await Task.Yield();

                    return null!;
                }
            }

            [Cache]
            public object GetValueBase()
            {
                return null!;
            }

            [Cache]
            public async Task<object> GetValueBaseAsync()
            {
                await Task.Yield();

                return null!;
            }
        }

        private sealed class ChildCachingClass : BaseCachingClass
        {
            public sealed class InnerCachingClassInChild
            {
                [Cache]
                public object GetValueInnerChild()
                {
                    return null!;
                }

                [Cache]
                public async Task<object> GetValueInnerChildAsync()
                {
                    await Task.Yield();

                    return null!;
                }
            }

            [Cache]
            public object GetValueChild()
            {
                return null!;
            }

            [Cache]
            public async Task<object> GetValueChildAsync()
            {
                await Task.Yield();

                return null!;
            }
        }

        private void DoCachingAttributeTest( Func<object> getValueAction, bool defaultProfile )
        {
            var backend =
                this.InitializeTestWithTestingBackend( _testCachingAttributeProfileName );

            TestProfileConfigurationFactory.CreateProfile( _testCachingAttributeProfileName );

            try
            {
                Assert.Null( backend.LastCachedKey );
                Assert.Null( backend.LastCachedItem );

                getValueAction.Invoke();

                Assert.NotNull( backend.LastCachedKey );
                Assert.NotNull( backend.LastCachedItem );

                Assert.Equal(
                    defaultProfile ? CachingProfile.DefaultName : _testCachingAttributeProfileName,
                    backend.LastCachedItem!.Configuration!.ProfileName );
            }
            finally
            {
                TestProfileConfigurationFactory.DisposeTest();
            }
        }

        private async Task DoCachingAttributeTestAsync( Func<Task<object>> getValueAction, bool defaultProfile )
        {
            var backend =
                this.InitializeTestWithTestingBackend( _testCachingAttributeProfileName );

            TestProfileConfigurationFactory.CreateProfile( _testCachingAttributeProfileName );

            try
            {
                Assert.Null( backend.LastCachedKey );
                Assert.Null( backend.LastCachedItem );

                await getValueAction.Invoke();

                Assert.NotNull( backend.LastCachedKey );
                Assert.NotNull( backend.LastCachedItem );

                Assert.Equal(
                    defaultProfile ? CachingProfile.DefaultName : _testCachingAttributeProfileName,
                    backend.LastCachedItem!.Configuration!.ProfileName );
            }
            finally
            {
                await TestProfileConfigurationFactory.DisposeTestAsync();
            }
        }

        [Fact]
        public void TestCachingAttributeBase()
        {
            var cachingClass = new ChildCachingClass();
            this.DoCachingAttributeTest( cachingClass.GetValueBase, false );
        }

        [Fact]
        public async Task TestCachingAttributeBaseAsync()
        {
            var cachingClass = new ChildCachingClass();
            await this.DoCachingAttributeTestAsync( cachingClass.GetValueBaseAsync, false );
        }

        [Fact]
        public void TestCachingAttributeChild()
        {
            var cachingClass = new ChildCachingClass();
            this.DoCachingAttributeTest( cachingClass.GetValueChild, false );
        }

        [Fact]
        public async Task TestCachingAttributeChildAsync()
        {
            var cachingClass = new ChildCachingClass();
            await this.DoCachingAttributeTestAsync( cachingClass.GetValueChildAsync, false );
        }

        [Fact]
        public void TestCachingAttributeInnerInBase()
        {
            var cachingClass = new BaseCachingClass.InnerCachingClassInBase();
            this.DoCachingAttributeTest( cachingClass.GetValueInnerBase, true );
        }

        [Fact]
        public async Task TestCachingAttributeInnerInBaseAsync()
        {
            var cachingClass = new BaseCachingClass.InnerCachingClassInBase();
            await this.DoCachingAttributeTestAsync( cachingClass.GetValueInnerBaseAsync, true );
        }

        [Fact]
        public void TestCachingAttributeInnerInBaseChild()
        {
            var cachingClass = new ChildCachingClass.InnerCachingClassInChild();
            this.DoCachingAttributeTest( cachingClass.GetValueInnerChild, true );
        }

        [Fact]
        public async Task TestCachingAttributeInnerInBaseChildAsync()
        {
            var cachingClass = new ChildCachingClass.InnerCachingClassInChild();
            await this.DoCachingAttributeTestAsync( cachingClass.GetValueInnerChildAsync, true );
        }

        public CacheConfigurationAttributeTests( ITestOutputHelper testOutputHelper ) : base( testOutputHelper ) { }
    }
}