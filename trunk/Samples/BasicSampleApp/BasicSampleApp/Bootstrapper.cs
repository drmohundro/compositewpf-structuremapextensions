using System.Windows;
using CompositeWPFContrib.Composite.StructureMapExtensions;
using Microsoft.Practices.Composite.Modularity;

namespace BasicSampleApp
{
    public class Bootstrapper : StructureMapBootstrapper
    {
        protected override IModuleCatalog GetModuleCatalog()
        {
            return new ModuleCatalog();
        }

        protected override void ConfigureContainer()
        {
            Container.Configure(x =>
                x.For<IShellView>().Use<Shell>()
            );

            base.ConfigureContainer();
        }

        protected override DependencyObject CreateShell()
        {
            var presenter = Container.GetInstance<ShellPresenter>();
            var view = presenter.View;
            view.ShowView();
            return view as DependencyObject;
        }
    }
}