﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Patterns.NotifyPropertyChanged.AspectTests.Options.SafeForDependencyAnalysis.UsingFabricOnExternalClass;

public static class ExternalClass
{
    public static int Foo() => 42;
}