using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Rees.TangyFruitMapper
{
    internal class NamespaceFinder
    {
        private readonly Type dtoType;
        private readonly Type modelType;

        public NamespaceFinder([NotNull] Type dtoType, [NotNull] Type modelType)
        {
            if (dtoType == null) throw new ArgumentNullException(nameof(dtoType));
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));
            this.dtoType = dtoType;
            this.modelType = modelType;
        }

        public IEnumerable<string> DiscoverNamespaces()
        {
            var list = new List<string>();
            list.Add(this.dtoType.Namespace);

            if (!list.Contains(this.modelType.Namespace))
            {
                list.Add(this.modelType.Namespace);
            }

            return list;
        }
    }
}
