﻿using System;

namespace FeiEventStore.IntegrationTests
{
    /// <summary>
    /// When applied on ITest class, only those test cases that are marked with this attribute will be executed.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class OnlyAttribute : Attribute
    {
        public OnlyAttribute()
        {
            
        }
    }
}
