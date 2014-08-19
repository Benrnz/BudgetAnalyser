﻿using System;
using System.Globalization;
using System.Reflection;

namespace Rees.TestUtilities {
    /// <summary>
    /// A utility class to allow test only access to private members for Dependency injection etc.
    /// </summary>
    public static class PrivateAccessor {
        /// <summary>
        /// Accesses a Private / Internal / Protected constructor and creates an object.
        /// </summary>
        /// <typeparam name="T">The type on which to access the constructor</typeparam>
        /// <param name="argumentTypes">The argument types.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A newly constructed object</returns>
        public static T PrivateConstructor<T>(Type[] argumentTypes, object[] parameters) where T : class {
            Type type = typeof(T);
            var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, argumentTypes, null);
            return constructor.Invoke(parameters) as T;
        }

        /// <summary>
        /// Accesses a Private / Internal / Protected default constructor and creates an object.
        /// </summary>
        /// <typeparam name="T">The type on which to access the constructor</typeparam>
        /// <returns>A newly constructed object</returns>
        public static T PrivateConstructor<T>() where T : class {
            return PrivateConstructor<T>(new Type[] { }, new object[] { });
        }

        /// <summary>
        /// Gets a constant value from a type.
        /// </summary>
        /// <param name="type">The type on which to look for the constant.</param>
        /// <param name="constName">Name of the constant.</param>
        /// <returns>The value of the constant.</returns>
        public static object GetConstant(Type type, string constName) {
            return GetStaticField(type, constName);
        }

        /// <summary>
        /// Gets a static private field.
        /// </summary>
        /// <param name="type">The type on which to look for the field.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the private field.</returns>
        public static object GetStaticField(Type type, string fieldName) {
            Guard.Against<ArgumentNullException>(type == null, "type cannot be null");
            Guard.Against<ArgumentNullException>(fieldName == null, "fieldName cannot be null");
            return GetStaticFieldInfo(type, fieldName).GetValue(null);
        }

        /// <summary>
        /// Sets the static private field.
        /// </summary>
        /// <param name="type">The type on which to look for the field.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetStaticField(Type type, string fieldName, object value) {
            Guard.Against<ArgumentNullException>(type == null, "type cannot be null");
            Guard.Against<ArgumentNullException>(fieldName == null, "fieldName cannot be null");
            GetStaticFieldInfo(type, fieldName).SetValue(null, value);
        }

        /// <summary>
        /// Gets a private field.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the private field.</returns>
        public static object GetField(object instance, string fieldName) {
            if (instance == null) {
                throw new ArgumentNullException("instance");
            }

            if (string.IsNullOrEmpty(fieldName)) {
                throw new ArgumentNullException("fieldName");
            }

            return GetFieldInfo(instance.GetType(), fieldName).GetValue(instance);
        }

        /// <summary>
        /// Sets the private field.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetField(object instance, string fieldName, object value) {
            if (instance == null) {
                throw new ArgumentNullException("instance");
            }

            if (string.IsNullOrEmpty(fieldName)) {
                throw new ArgumentNullException("fieldName");
            }

            GetFieldInfo(instance.GetType(), fieldName).SetValue(instance, value);
        }

        /// <summary>
        /// Gets a private property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The value of the private property.</returns>
        public static object GetProperty(object instance, string propertyName) {
            if (instance == null) {
                throw new ArgumentNullException("instance");
            }

            if (string.IsNullOrEmpty(propertyName)) {
                throw new ArgumentNullException("propertyName");
            }

            return GetPropertyInfo(instance.GetType(), propertyName).GetValue(instance, new object[] { });
        }

        /// <summary>
        /// Gets a private property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <typeparam name="T">The type of the instance object. Use this to specify a different type other than its concrete type</typeparam>
        /// <returns>The value of the private property.</returns>
        public static object GetProperty<T>(object instance, string propertyName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            return GetPropertyInfo(typeof(T), propertyName).GetValue(instance, new object[] { });
        }

        /// <summary>
        /// Sets the private property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetProperty(object instance, string propertyName, object value) {
            if (instance == null) {
                throw new ArgumentNullException("instance");
            }

            if (string.IsNullOrEmpty(propertyName)) {
                throw new ArgumentNullException("propertyName");
            }

            GetPropertyInfo(instance.GetType(), propertyName).SetValue(instance, value, new object[] { });
        }

        /// <summary>
        /// Invokes a non-public function.
        /// </summary>
        /// <typeparam name="T">The return type of the member to invoke</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">Name of the function.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The return value of the method</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Typed function saves time and code")]
        public static T InvokeFunction<T>(object instance, string name, params object[] arguments) {
            Guard.Against<ArgumentNullException>(instance == null, "instance cannot be null");
            Guard.Against<ArgumentNullException>(name == null, "name cannot be null");

            Type returnType = typeof(T);
            object result = GetMethod(instance.GetType(), name, arguments, instance);
            try {
                return (T)result;
            } catch (InvalidCastException ex) {
                throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "The return type was not the expected type. Expected {0}. But was {1}", returnType.Name, result.GetType().Name), ex);
            }
        }

        /// <summary>
        /// Invokes a non-public method.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">Name of the function.</param>
        /// <param name="arguments">The arguments.</param>
        public static void InvokeMethod(object instance, string name, params object[] arguments) {
            Guard.Against<ArgumentNullException>(instance == null, "instance cannot be null");
            Guard.Against<ArgumentNullException>(name == null, "name cannot be null");

            try {
                GetMethod(instance.GetType(), name, arguments, instance);
            } catch (TargetInvocationException ex) {
                var inner = ex.InnerException;
                if (inner == null) {
                    throw;
                }

                throw inner;
            }
        }

        /// <summary>
        /// Invokes a non-public static method.
        /// </summary>
        /// <param name="type">The Type on which to look for the static method.</param>
        /// <param name="name">Name of the method.</param>
        /// <param name="arguments">The arguments.</param>
        public static void InvokeStaticMethod(Type type, string name, params object[] arguments) {
            Guard.Against<ArgumentNullException>(type == null, "type cannot be null");
            Guard.Against<ArgumentNullException>(name == null, "name cannot be null");

            GetMethod(type, name, arguments, null);
        }

        /// <summary>
        /// Invokes a non-public static function.
        /// </summary>
        /// <typeparam name="T">The return type of the member to invoke</typeparam>
        /// <param name="type">The Type on which to look for the static method.</param>
        /// <param name="name">Name of the method.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The typed return value of the method</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Typed function saves time and code")]
        public static T InvokeStaticFunction<T>(Type type, string name, params object[] arguments) {
            Guard.Against<ArgumentNullException>(type == null, "type cannot be null");
            Guard.Against<ArgumentNullException>(name == null, "name cannot be null");

            Type returnType = typeof(T);
            object result = GetMethod(type, name, arguments, null);
            try {
                return (T)result;
            } catch (InvalidCastException ex) {
                throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "The return type was not the expected type. Expected {0}. But was {1}", returnType.Name, result.GetType().Name), ex);
            }
        }

        private static object GetMethod(Type type, string name, object[] arguments, object instance) {
            bool isStatic = instance == null;
            var flags = isStatic ? BindingFlags.NonPublic | BindingFlags.Static : BindingFlags.Instance | BindingFlags.NonPublic;
            var method = type.GetMethod(name, flags);
            if (method == null) {
                throw new NotSupportedException("Type does not support this member name. " + name);
            }

            return method.Invoke(instance, arguments);
        }

        private static FieldInfo GetFieldInfo(Type type, string fieldName) {
            var info = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (info == null) {
                throw new ArgumentException("Field does not exist (is the field static?)", "fieldName");
            }

            return info;
        }

        private static FieldInfo GetStaticFieldInfo(Type type, string fieldName) {
            var info = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
            if (info == null) {
                throw new ArgumentException("Static Field does not exist (is the field instance?)", "fieldName");
            }

            return info;
        }

        private static PropertyInfo GetPropertyInfo(Type type, string propertyName) {
            var info = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (info == null) {
                throw new ArgumentException("Field does not exist", "propertyName");
            }

            return info;
        }
    }
}
