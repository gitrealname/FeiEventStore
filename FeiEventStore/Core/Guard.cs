using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace FeiEventStore.Core
{

    /// <summary>
    ///     A set of common methods used through the NEventStore.
    /// </summary>
    public static class Guard
    {
        public static string FormatWith(this string format, params object[] values)
        {
            return string.Format(CultureInfo.InvariantCulture, format ?? string.Empty, values);
        }

        public static void NotFalse(bool condition, Func<Exception> createException)
        {
            if(!condition)
            {
                throw createException();
            }
        }

        public static void NotNullOrWhiteSpace(Expression<Func<string>> reference, string value)
        {
            if(string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(GetParameterName(reference));
            }
        }


        public static void NotNull<T>(Expression<Func<T>> reference, T value)
        {
            if(ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(GetParameterName(reference));
            }
        }

        public static void EqualTo<T>(Expression<Func<T>> reference, T value, T compareTo)
            where T : IComparable
        {
            NotNull(reference, value);
            if(value.CompareTo(compareTo) != 0)
            {
                throw new ArgumentOutOfRangeException("{0} has value {1} which is not equal to {2}".FormatWith(GetParameterName(reference), value, compareTo));
            }
        }

        public static void NotLessThanOrEqualTo<T>(Expression<Func<T>> reference, T value, T compareTo)
            where T : IComparable
        {
            NotNull(reference, value);
            if(value.CompareTo(compareTo) <= 0)
            {
                throw new ArgumentOutOfRangeException("{0} has value {1} which is less than or equal to {2}".FormatWith(GetParameterName(reference), value, compareTo));
            }
        }

        public static void NotLessThan<T>(Expression<Func<T>> reference, T value, T compareTo)
            where T : IComparable
        {
            NotNull(reference, value);
            if(value.CompareTo(compareTo) < 0)
            {
                throw new ArgumentOutOfRangeException("{0} has value {1} which is less than {2}".FormatWith(GetParameterName(reference), value, compareTo));
            }
        }

        public static void NotDefault<T>(Expression<Func<T>> reference, T value)
            where T : IComparable
        {
            NotNull(reference, value);
            if(value.CompareTo(default(T)) == 0)
            {
                throw new ArgumentException("{0} has value {1} which cannot be equal to it's default value {2}".FormatWith(GetParameterName(reference), value, default(T)));
            }
        }

        public static void NotEmpty<T>(Expression<Func<IEnumerable<T>>> reference, IEnumerable<T> value)
        {
            NotNull(reference, value);
            if(!value.Any())
            {
                throw new ArgumentException("{0} cannot be empty".FormatWith(GetParameterName(reference), value, default(T)));
            }
        }

        private static string GetParameterName(LambdaExpression reference)
        {
            return ((MemberExpression)reference.Body).Member.Name;
        }
    }
}