// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;

namespace osu.Framework.Allocation
{
    /// <summary>
    /// An attribute that is attached to fields of a <see cref="Drawable"/> component to indicate
    /// that the value of the field should be retrieved from a dependency cache.
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Property)]
    public class DependencyAttribute : Attribute
    {
        private const BindingFlags activator_flags = BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Whether a null value can be accepted if the value does not exist in the cache.
        /// </summary>
        public bool CanBeNull;

        internal static InjectDependencyDelegate CreateActivator(Type type)
        {
            var activators = new List<Action<object, DependencyContainer>>();

            var properties = type.GetProperties(activator_flags).Where(f => f.GetCustomAttribute<DependencyAttribute>() != null);
            foreach (var property in properties)
            {
                if (!property.CanWrite)
                    throw new PropertyNotWritableException(type, property.Name);

                var attribute = property.GetCustomAttribute<DependencyAttribute>();
                var fieldGetter = getDependency(property.PropertyType, type, attribute.CanBeNull);

                activators.Add((target, dc) => property.SetValue(target, fieldGetter(dc)));
            }

            return (target, dc) => activators.ForEach(a => a(target, dc));
        }

        private static Func<DependencyContainer, object> getDependency(Type type, Type requestingType, bool permitNulls) => dc =>
        {
            var val = dc.Get(type);
            if (val == null && !permitNulls)
                throw new DependencyNotRegisteredException(requestingType, type);
            return val;
        };
    }

    public class PropertyNotWritableException : Exception
    {
        public PropertyNotWritableException(Type type, string propertyName)
            : base($"Attempting to inject dependencies into non-write-able property {propertyName} of type {type.ReadableName()}.")
        {
        }
    }
}
