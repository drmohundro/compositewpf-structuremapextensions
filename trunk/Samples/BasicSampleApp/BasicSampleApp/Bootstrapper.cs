using System.Windows;
using CompositeWPFContrib.Composite.StructureMapExtensions;
using Microsoft.Practices.Composite.Modularity;

namespace BasicSampleApp
{
    public class Bootstrapper : StructureMapBootstrapper
    {
        protected override IModuleEnumerator GetModuleEnumerator()
        {
            return new StaticModuleEnumerator();
        }

        protected override void ConfigureContainer()
        {
            Container.Configure(x =>
                x.BuildInstancesOf<IShellView>().TheDefaultIsConcreteType<Shell>()
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