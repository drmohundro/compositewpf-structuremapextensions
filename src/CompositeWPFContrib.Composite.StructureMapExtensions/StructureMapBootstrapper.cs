using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Practices.Composite;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Presentation.Regions.Behaviors;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Composite.Presentation.Regions;
using Microsoft.Practices.ServiceLocation;
using StructureMap;
using StructureMap.Exceptions;

namespace CompositeWPFContrib.Composite.StructureMapExtensions
{
    /// <summary>
    /// Base class that provides a basic bootstrapper sequene that
    /// registers most of the Composite Application Library assets
    /// in a <see cref="StructureMap.IContainer"/>.
    /// </summary>
    /// <remarks>
    /// This class must be overriden to provide application specific configuration.
    /// </remarks>
    public abstract class StructureMapBootstrapper
    {
        private bool useDefaultConfiguration = true;

        private readonly ILoggerFacade loggerFacade = new TraceLogger();

        /// <summary>
        /// Gets the default <see cref="ILoggerFacade"/> for the application.
        /// </summary>
        /// <value>A <see cref="ILoggerFacade"/> instance.</value>
        public ILoggerFacade LoggerFacade
        {
            get { return loggerFacade; }
        }

        /// <summary>
        /// Gets the default <see cref="IContainer"/> for the application.
        /// </summary>
        /// <value>The default <see cref="IContainer"/> instance.</value>
        [CLSCompliant(false)]
        public IContainer Container { get; private set; }

        /// <summary>
        /// Runs the bootstrapper process.
        /// </summary>
        public void Run()
        {
            Run(true);
        }

        public void Run(bool runWithDefaultConfiguration)
        {
            useDefaultConfiguration = runWithDefaultConfiguration;

            var logger = LoggerFacade;
            if (logger == null)
                throw new InvalidOperationException("NullLoggerException");

            logger.Log("Creating StructureMap container", Category.Debug, Priority.Low);
            Container = CreateContainer();
            if (Container == null)
                throw new InvalidOperationException("NullContainerException");

            logger.Log("Configuring container", Category.Debug, Priority.Low);
            ConfigureContainer();

            logger.Log("Configuring region adapters", Category.Debug, Priority.Low);

            ConfigureRegionAdapterMappings();
            ConfigureDefaultRegionBehaviors();
            RegisterFrameworkExceptionTypes();

            logger.Log("Creating shell", Category.Debug, Priority.Low);
            var shell = CreateShell();
            if (shell != null)
            {
                RegionManager.SetRegionManager(shell, Container.GetInstance<IRegionManager>());
                RegionManager.UpdateRegions();
            }

            logger.Log("Initializing modules", Category.Debug, Priority.Low);
            InitializeModules();

            logger.Log("Bootstrapper sequence completed", Category.Debug, Priority.Low);
        }

        /// <summary>
        /// Configures the <see cref="IRegionBehaviorFactory"/>. This will be the list of default
        /// behaviors that will be added to a region. 
        /// </summary>
        protected virtual IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
        {
            var defaultRegionBehaviorTypesDictionary = Container.TryGetInstance<IRegionBehaviorFactory>();

            if (defaultRegionBehaviorTypesDictionary != null)
            {
                defaultRegionBehaviorTypesDictionary.AddIfMissing(AutoPopulateRegionBehavior.BehaviorKey,
                                                                  typeof(AutoPopulateRegionBehavior));

                defaultRegionBehaviorTypesDictionary.AddIfMissing(BindRegionContextToDependencyObjectBehavior.BehaviorKey,
                                                                  typeof(BindRegionContextToDependencyObjectBehavior));

                defaultRegionBehaviorTypesDictionary.AddIfMissing(RegionActiveAwareBehavior.BehaviorKey,
                                                                  typeof(RegionActiveAwareBehavior));

                defaultRegionBehaviorTypesDictionary.AddIfMissing(SyncRegionContextWithHostBehavior.BehaviorKey,
                                                                  typeof(SyncRegionContextWithHostBehavior));

                defaultRegionBehaviorTypesDictionary.AddIfMissing(RegionManagerRegistrationBehavior.BehaviorKey,
                                                                  typeof(RegionManagerRegistrationBehavior));

            }
            return defaultRegionBehaviorTypesDictionary;

        }

        /// <summary>
        /// Registers in the <see cref="IContainer"/> the <see cref="Type"/> of the Exceptions
        /// that are not considered root exceptions by the <see cref="ExceptionExtensions"/>.
        /// </summary>
        protected virtual void RegisterFrameworkExceptionTypes()
        {
            ExceptionExtensions.RegisterFrameworkExceptionType(
                typeof(ActivationException));

            ExceptionExtensions.RegisterFrameworkExceptionType(
                typeof(MissingPluginFamilyException));
        }

        /// <summary>
        /// Configures the <see cref="IContainer"/>. May be overwritten in a derived class to add specific
        /// type mappings required by the application.
        /// </summary>
        protected virtual void ConfigureContainer()
        {
            Container.Configure(reg =>
                {
                    reg.For<ILoggerFacade>().Use(LoggerFacade);
                    reg.For<IContainer>().Use(Container);

                    var catalog = GetModuleCatalog();
                    if (catalog != null)
                        reg.For<IModuleCatalog>().Use(catalog);

                    if (useDefaultConfiguration)
                    {
                        RegisterTypeIfMissing<IServiceLocator, StructureMapServiceLocator>(true);
                        RegisterTypeIfMissing<IModuleInitializer, ModuleInitializer>(true);
                        RegisterTypeIfMissing<IModuleManager, ModuleManager>(true);
                        RegisterTypeIfMissing<RegionAdapterMappings, RegionAdapterMappings>(true);
                        RegisterTypeIfMissing<IRegionManager, RegionManager>(true);
                        RegisterTypeIfMissing<IEventAggregator, EventAggregator>(true);
                        RegisterTypeIfMissing<IRegionViewRegistry, RegionViewRegistry>(true);
                        RegisterTypeIfMissing<IRegionBehaviorFactory, RegionBehaviorFactory>(true);

                        ServiceLocator.SetLocatorProvider(() => Container.GetInstance<IServiceLocator>());
                    }
                });
        }

        /// <summary>
        /// Configures the default region adapter mappings to use in the application, in order
        /// to adapt UI controls defined in XAML to use a region and register it automatically.
        /// May be overwritten in a derived class to add specific mappings required by the application.
        /// </summary>
        /// <returns>The <see cref="RegionAdapterMappings"/> instance containing all the mappings.</returns>
        protected virtual RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            var regionAdapterMappings = Container.GetInstance<RegionAdapterMappings>();
            if (regionAdapterMappings != null)
            {
                regionAdapterMappings.RegisterMapping(typeof (Selector), Container.GetInstance<SelectorRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof (ItemsControl), Container.GetInstance<ItemsControlRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof (ContentControl), Container.GetInstance<ContentControlRegionAdapter>());
            }

            return regionAdapterMappings;
        }

        /// <summary>
        /// Initializes the modules. May be overwritten in a derived class to use custom
        /// module loading and avoid using an <seealso cref="IModuleManager"/> and
        /// <seealso cref="IModuleManager"/>.
        /// </summary>
        protected virtual void InitializeModules()
        {
            IModuleManager manager;

            try
            {
                manager = Container.GetInstance<IModuleManager>();
            }
            catch (StructureMapException ex)
            {
                if (ex.Message.Contains("IModuleCatalog"))
                    throw new InvalidOperationException("Module not found.");

                throw;
            }

            manager.Run();
        }

        /// <summary>
        /// Creates the <see cref="IContainer"/> that will be used as the default container.
        /// </summary>
        /// <returns>A new instance of <see cref="IContainer"/>.</returns>
        [CLSCompliant(false)]
        protected virtual IContainer CreateContainer()
        {
            return new Container();
        }

        /// <summary>
        /// Returns the module enumerator that will be used to initialize the modules.
        /// </summary>
        /// <remarks>
        /// When using the default initialization behavior, this method must be overwritten by a derived class.
        /// </remarks>
        /// <returns>An instance of <see cref="IModuleCatalog"/> that will be used to initialize the modules.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual IModuleCatalog GetModuleCatalog()
        {
            return null;
        }

        protected void RegisterTypeIfMissing<TFrom, TTo>(bool registerAsSingleton)
            where TFrom : class
            where TTo : class, TFrom
        {
            var logger = LoggerFacade;

            if (Container.TryGetInstance<TFrom>() != null)
            {
                logger.Log(string.Format("{0} is already registered", typeof(TFrom).Name),
                           Category.Debug, Priority.Low);
            }
            else
            {
                Container.Configure(x =>
                    {
                        if (registerAsSingleton)
                            x.For<TFrom>().Singleton().Use<TTo>();
                        else
                            x.For<TFrom>().Use<TTo>();
                    });
            }
        }

        /// <summary>
        /// Creates the shell or main window of the application.
        /// </summary>
        /// <returns>The shell of the application.</returns>
        /// <remarks>
        /// If the returned instance is a <see cref="DependencyObject"/>, the
        /// <see cref="StructureMapBootstrapper"/> will attach the default <seealso cref="IRegionManager"/> of
        /// the application in its <see cref="RegionManager.RegionManagerProperty"/> attached property
        /// in order to be able to add regions by using the <seealso cref="RegionManager.RegionNameProperty"/>
        /// attached property from XAML.
        /// </remarks>
        protected abstract DependencyObject CreateShell();
    }
}