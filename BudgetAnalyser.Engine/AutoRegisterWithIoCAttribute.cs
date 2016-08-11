using System;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     An attribute to decorate any class that should be automatically registered with the IoC Container.
    ///     If <see cref="AutoRegisterWithIoCAttribute.RegisterAs" /> is not specified all implemented interfaces will be
    ///     registered as well as the class type itself.
    ///     By default only one type is expected to be registered per interface/abstract class, although no checking is done,
    ///     last in wins. To register multiple types against
    ///     a single interface/abstract class use the <see cref="AutoRegisterWithIoCAttribute.Named" /> property to specify a
    ///     unique name for each one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AutoRegisterWithIoCAttribute : Attribute
    {
        /// <summary>
        ///     Optional, register as a named instance by specifying a name here. If no name is specified then the class is
        ///     registered as a default instance. If a name
        ///     is specified, then multiple registrations can be done for the same type/interface.
        ///     If specified, this name must be used when requesting an instance from the IoC container.
        /// </summary>
        public string Named { get; set; }

        /// <summary>
        ///     Optional, register it as a specific type, useful if the required registration interface is not directly implemented
        ///     by the class ie a parent implements it.
        /// </summary>
        public Type RegisterAs { get; set; }

        /// <summary>
        ///     Optional, register as a single instance by setting this to true. The default is false, transient.
        /// </summary>
        public bool SingleInstance { get; set; }
    }
}