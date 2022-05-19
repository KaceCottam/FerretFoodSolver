using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using FerretFoodSolver.ViewModels;
using ReactiveUI;

namespace FerretFoodSolver.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(ViewModel!.ErrorPopupAsyncInteraction.RegisterHandler(HandleErrorPopupAsync));
            });
        }

        private async Task HandleErrorPopupAsync(InteractionContext<string, Unit> interaction)
        {
            var window = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow;
            var dialog = new Avalonia.Dialogs.AboutAvaloniaDialog() { Content = new TextBlock { Text = "Error!" } };
            await dialog.ShowDialog(window);
            interaction.SetOutput(default);
        }
    }
}