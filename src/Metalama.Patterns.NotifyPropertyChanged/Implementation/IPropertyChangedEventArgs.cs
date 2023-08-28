﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Patterns.NotifyPropertyChanged.Implementation;

/// <summary>
/// Allows covariant use of <see cref="PropertyChangedEventArgs{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPropertyChangedEventArgs<out T>
{
    T? OldValue { get; }

    T? NewValue { get; }
}