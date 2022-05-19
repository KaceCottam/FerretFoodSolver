using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Metadata;
using FerretFoodSolver.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace FerretFoodSolver.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [NotNull] public FerretSolver solver = new();
        [Reactive] public double TargetMusclePercent { get; set; }
        [Reactive] public double TargetOrganPercent { get; set; }
        [Reactive] public double TargetHeartPercent { get; set; }
        [Reactive] public double TargetBonePercent { get; set; }
        [Reactive] public double Sigma { get; set; }
        [Reactive] public double TargetWeight { get; set; }
        [Reactive] public double? ObjectiveValue { get; set; }
        [Reactive] public double? ActualMusclePercent { get; set; }
        [Reactive] public double? ActualOrganPercent { get; set; }
        [Reactive] public double? ActualHeartPercent { get; set; }
        [Reactive] public double? ActualBonePercent { get; set; }

        [Reactive]
        public BindingList<FerretMenuItem> DataList { get; set; } = new()
        {
            RaiseListChangedEvents = true,
            AllowNew = true,
            AllowRemove = true,
            AllowEdit = true
        };

        public MainWindowViewModel()
        {
            ErrorPopupAsync = ReactiveCommand.CreateFromTask<string, Unit>(async (s) => await ErrorPopupAsyncInteraction.Handle(s));

            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(e => ErrorPopupAsync.Execute(e.ToString()));

            this.WhenAnyValue(x => x.TargetMusclePercent)
                .BindTo(solver, x => x.TargetMusclePercent);
            this.WhenAnyValue(x => x.TargetOrganPercent)
                .BindTo(solver, x => x.TargetOrganPercent);
            this.WhenAnyValue(x => x.TargetHeartPercent)
                .BindTo(solver, x => x.TargetHeartPercent);
            this.WhenAnyValue(x => x.TargetBonePercent)
                .BindTo(solver, x => x.TargetBonePercent);
            this.WhenAnyValue(x => x.Sigma)
                .BindTo(solver, x => x.Sigma);
            this.WhenAnyValue(x => x.TargetWeight)
                .BindTo(solver, x => x.TargetWeight);
            this.WhenAnyValue(x => x.DataList)
                .WhereNotNull()
                .Select(x => new BindingList<FerretMenuItem>(x))
                .BindTo(solver, x => x.Parameters);

            DataList.ListChanged += delegate
            {
                this.RaisePropertyChanged(nameof(DataList));
            };

            Reset();
        }

        public async Task Solve()
        {
            if (await Task.Run(() => solver.Solve()) is null)
            {
                await ErrorPopupAsync.Execute("Unfeasible to find a solution.");
            }

            foreach (var (i, y) in solver.Parameters.Zip(DataList)) y.Solution = i.Solution;
            ActualMusclePercent = solver.ActualMusclePercent;
            ActualOrganPercent = solver.ActualOrganPercent;
            ActualHeartPercent = solver.ActualHeartPercent;
            ActualBonePercent = solver.ActualBonePercent;

            ObjectiveValue = solver.ObjectiveValue;
        }

        [DependsOn(nameof(TargetMusclePercent))]
        [DependsOn(nameof(TargetBonePercent))]
        [DependsOn(nameof(TargetOrganPercent))]
        [DependsOn(nameof(TargetHeartPercent))]
        [DependsOn(nameof(TargetWeight))]
        [DependsOn(nameof(DataList))]
        public bool CanSolve(object _)
        {
            bool Equals(double a, double b, double epsilon)
            {
                return Math.Abs(a - b) <= epsilon;
            }

            if (TargetWeight == 0) return false;
            if (TargetBonePercent == 0) return false;
            if (TargetOrganPercent == 0) return false;
            if (TargetMusclePercent == 0) return false;
            if (TargetHeartPercent == 0) return false;
            if (!Equals(TargetBonePercent + TargetMusclePercent + TargetHeartPercent + TargetOrganPercent, 1, 0.01)) return false;
            if (DataList.Count == 0) return false;
            foreach (var i in DataList) if (i.WeightConversion == 0) return false;
            foreach (var i in DataList) if (!Equals(i.BonePercent + i.MusclePercent + i.HeartPercent + i.OrganPercent, 1, 0.01)) return false;
            return true;
        }

        public void Delete(object parameter)
        {
            if (parameter is not FerretMenuItem x)
                throw new ArgumentException("needs to be FerretMenuItem!", nameof(parameter));

            DataList.Remove(x);
            DataList = new BindingList<FerretMenuItem>(DataList.ToList());
        }

        public bool CanDelete(object p) => p is not null && DataList.Count > 1;

        public void Reset()
        {
            TargetMusclePercent = 0.75;
            TargetOrganPercent = 0.10;
            TargetHeartPercent = 0.05;
            TargetBonePercent = 0.10;
            TargetWeight = 4;
            Sigma = 0.05;
            ObjectiveValue = null;
            DataList.Clear();
            DataList.Add(new FerretMenuItem()
            {
                BonePercent = 0.10,
                HeartPercent = 0.05,
                OrganPercent = 0.10,
                MusclePercent = 0.75,
            });
        }

        public void AddRow()
        {
            DataList.Add(new FerretMenuItem() { MusclePercent = 1.0 });
            DataList = new BindingList<FerretMenuItem>(DataList.ToList());
        }

        public Interaction<string, Unit> ErrorPopupAsyncInteraction { get; } = new();
        public ReactiveCommand<string, Unit> ErrorPopupAsync { get; }
    }
}
