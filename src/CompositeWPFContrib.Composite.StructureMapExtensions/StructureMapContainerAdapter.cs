using System;
using Microsoft.Practices.Composite;
using StructureMap;

namespace CompositeWPFContrib.Composite.StructureMapExtensions
{
    /// <summary>
    /// Defines a <seealso cref="IContainer"/> adapter for
    /// the <see cref="IContainer"/> interface
    /// to be used by the Composite Application Library.
    /// </summary>
    public class StructureMapContainerAdapter : IContainerFacade
    {
        private readonly IContainer _container;

        /// <summary>
        /// Initializes a new instance of <see cref="StructureMapContainerAdapter"/>.
        /// </summary>
        /// <param name="container">The <seealso cref="IContainer"/> that will be used
        /// by the <see cref="Resolve"/> and <see cref="TryResolve"/> methods.</param>
        public StructureMapContainerAdapter(IContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Resolve an instance of the requested type from the container.
        /// </summary>
        /// <param name="type">The type of object to get from the container.</param>
        /// <returns>An instance of <paramref name="type"/>.</returns>
        public object Resolve(Type type)
        {
            return _container.GetInstance(type);
        }

        /// <summary>
        /// Tries to resolve an instance of the requested type from the container.
        /// </summary>
        /// <param name="type">The type of object to get from the container.</param>
        /// <returns>
        /// An instance of <paramref name="type"/>. 
        /// If the type cannot be resolved it will return a <see langword="null"/> value.
        /// </returns>
        public object TryResolve(Type type)
        {
            return _container.TryGetInstance(type);
        }
    }
}