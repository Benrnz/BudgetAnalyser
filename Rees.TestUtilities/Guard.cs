using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Rees.TestUtilities
{
    /// <summary>
    ///     A Precondition validation helper class
    /// </summary>
    public static class Guard
    {
        /// <summary>
        ///     Guards Against the specified exception. Use as a pre-condition checker to check parameters and conditions before
        ///     getting into the core of a method.
        /// </summary>
        /// <param name="erroneousCondition">The assertion validation.</param>
        /// <param name="message">The message to pass into the exception if one should be raised</param>
        /// <typeparam name="T">
        ///     The type of exception to guard against and throw should <paramref name="erroneousCondition" />
        ///     return true.
        /// </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Best reading syntax")]
        public static void Against<T>(Func<bool> erroneousCondition, string message) where T : Exception, new()
        {
            if (erroneousCondition == null || erroneousCondition())
            {
                AgainstInternal<T>(message);
            }
        }

        /// <summary>
        ///     Guards Against the specified exception. Use as a pre-condition checker to check parameters and conditions before
        ///     getting into the core of a method.
        /// </summary>
        /// <param name="erroneousCondition">The assertion validation. If true the exception will be thrown.</param>
        /// <param name="message">The message to pass into the exception if one should be raised</param>
        /// <typeparam name="T">
        ///     The type of exception to guard against and throw should <paramref name="erroneousCondition" />
        ///     return true.
        /// </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Best reading syntax")]
        public static void Against<T>(bool erroneousCondition, string message) where T : Exception, new()
        {
            if (erroneousCondition)
            {
                AgainstInternal<T>(message);
            }
        }

        /// <summary>
        ///     Againsts the specified erroneous condition and if it exists an action is taken to fix it
        /// </summary>
        /// <param name="erroneousCondition">
        ///     The erroneous condition. If true the <paramref name="rectifyTheProblem" /> Action will
        ///     be performed.
        /// </param>
        /// <param name="rectifyTheProblem">The action to rectify the problem.</param>
        public static void Against(Func<bool> erroneousCondition, Action rectifyTheProblem)
        {
            if (rectifyTheProblem == null)
            {
                throw new ArgumentNullException(nameof(rectifyTheProblem));
            }

            if (erroneousCondition == null || erroneousCondition())
            {
                rectifyTheProblem();
            }
        }

        /// <summary>
        ///     Ensures the <paramref name="instance" /> Implements the specified <typeparamref name="TInterface" /> interface.
        ///     If it does not then an <see cref="InvalidOperationException" /> is thrown.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="InvalidOperationException">Thrown if the type does not implement the interface</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Best reading syntax")]
        public static void Implements<TInterface>(object instance, string message) where TInterface : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            Implements<TInterface>(instance.GetType(), message);
        }

        /// <summary>
        ///     Ensures the <paramref name="type" /> Implements the specified <typeparamref name="TInterface" /> interface.
        ///     If it does not then an <see cref="InvalidOperationException" /> is thrown.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="type">The type in question.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="InvalidOperationException">Thrown if the type does not implement the interface</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Best reading syntax")]
        public static void Implements<TInterface>(Type type, string message) where TInterface : class
        {
            if (!typeof(TInterface).IsAssignableFrom(type))
            {
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        ///     Ensures the <paramref name="instance" /> inherits from <typeparamref name="TBase" />.
        /// </summary>
        /// <typeparam name="TBase">The type of the base.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="message">The message.</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Best reading syntax")]
        public static void InheritsFrom<TBase>(object instance, string message) where TBase : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            InheritsFrom<TBase>(instance.GetType(), message);
        }

        /// <summary>
        ///     Ensures the <paramref name="type" /> inherits from <typeparamref name="TBase" />.
        /// </summary>
        /// <typeparam name="TBase">The type of the base.</typeparam>
        /// <param name="type">The type in question.</param>
        /// <param name="message">The message.</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Best reading syntax")]
        public static void InheritsFrom<TBase>(Type type, string message) where TBase : class
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.BaseType != typeof(TBase))
            {
                throw new InvalidOperationException(message);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Figuring out how and what exception to throw should not pollute the actual exception")]
        private static void AgainstInternal<T>(string message) where T : Exception, new()
        {
            T ex = null;
            foreach (ConstructorInfo constructor in typeof(T).GetConstructors(BindingFlags.Public))
            {
                if (constructor.GetParameters().Length == 1)
                {
                    ParameterInfo param = constructor.GetParameters()[0];
                    if (param.ParameterType == typeof(string) && param.Name.ToUpper(CultureInfo.CurrentCulture) == "MESSAGE")
                    {
                        ex = constructor.Invoke(new object[] { message }) as T;
                        break;
                    }
                }
            }

            if (ex == null)
            {
                try
                {
                    ex = Activator.CreateInstance(typeof(T), message) as T;
                }
                catch
                {
                    ex = new T();
                }

                if (ex == null)
                {
                    ex = new T();
                }
            }

            throw ex;
        }
    }
}