using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Rees.TangyFruitMapper
{
    internal class NamespaceFinder
    {
        private readonly Type dtoType;
        private readonly Type modelType;
        private readonly List<Type> visitedTypes = new List<Type>();

        public NamespaceFinder([NotNull] Type dtoType, [NotNull] Type modelType)
        {
            if (dtoType == null) throw new ArgumentNullException(nameof(dtoType));
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));
            this.dtoType = dtoType;
            this.modelType = modelType;
        }

        public IReadOnlyDictionary<string, string> DiscoverNamespaces()
        {
            var namespaces = new ConcurrentDictionary<string, string>();
            namespaces.GetOrAdd("System", key => "System");
            DiscoverAllNamespaces(namespaces, this.dtoType);
            DiscoverAllNamespaces(namespaces, this.modelType);
            return namespaces;
        }

        private void DiscoverAllNamespaces(ConcurrentDictionary<string, string> namespaces, Type type)
        {
            namespaces.GetOrAdd(type.Namespace, key => type.Name);

            if (!this.visitedTypes.Contains(type))
            {
                this.visitedTypes.Add(type);
                foreach (var propertyInfo in type.GetProperties())
                {
                    DiscoverAllNamespaces(namespaces, propertyInfo.PropertyType);
                }
            }
        }
    }
}