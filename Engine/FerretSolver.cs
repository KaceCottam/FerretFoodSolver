
using System.Collections.Generic;
using System.Linq;
using OPTANO.Modeling.Optimization;
using OPTANO.Modeling.Optimization.Solver;
using OPTANO.Modeling.Optimization.Enums;
using Engine;
using System;

namespace FerretFoodSolver.Engine
{
    public class FerretSolver
    {
        public IList<FerretMenuItem> Parameters { get; set; } = new List<FerretMenuItem>();

        public double TargetWeight { get; set; } = 0.0;
        public double TargetMusclePercent { get; set; } = 0.0;
        public double TargetOrganPercent { get; set; } = 0.0;
        public double TargetHeartPercent { get; set; } = 0.0;
        public double TargetBonePercent { get; set; } = 0.0;
        public double Sigma { get; set; } = 0.05;
        public double? ObjectiveValue { get; private set; }
        public double? ActualMusclePercent { get; private set; }
        public double? ActualOrganPercent { get; private set; }
        public double? ActualHeartPercent { get; private set; }
        public double? ActualBonePercent { get; private set; }

        public void Verify()
        {
            if (TargetWeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(TargetWeight), TargetWeight, "Target Weight must be > 0!");
            if (TargetMusclePercent + TargetOrganPercent + TargetHeartPercent + TargetBonePercent is < 0.99 or > 1.01)
                throw new ArgumentException("Target %s must sum to be between 99% and 101%!");
            if (TargetMusclePercent is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(TargetMusclePercent), TargetMusclePercent, "Target % must be between 0.0 and 1.0!");
            if (TargetOrganPercent is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(TargetOrganPercent), TargetOrganPercent, "Target % must be between 0.0 and 1.0!");
            if (TargetHeartPercent is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(TargetHeartPercent), TargetHeartPercent, "Target % must be between 0.0 and 1.0!");
            if (TargetBonePercent is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(TargetBonePercent), TargetBonePercent, "Target % must be between 0.0 and 1.0!");
            if (Parameters.Count < 1)
                throw new ArgumentException("Must have at least 1 parameter!", nameof(Parameters));

            foreach (var param in Parameters) param.Verify();
        }

        public double? Solve(SolverBase solver)
        {
            // model construction
            var model = new Model();

            var X = Enumerable.Range(0, Parameters.Count).ToArray();

            var xi = new VariableCollection<int>(
                model, 
                X,
                name: "x",
                debugNameGenerator: i => $"x_{i}",
                variableTypeGenerator: x => VariableType.Continuous
                );

            var H1 = Expression.Sum((X, Parameters).ZipWith((x, y) => xi[x] * y.WeightConversion));

            model.AddObjective(new Objective(H1, name: "Maximize", sense: ObjectiveSense.Maximize));

            model.AddConstraint(H1 >= TargetWeight - Sigma, "WeightLB");
            model.AddConstraint(H1 <= TargetWeight, "WeightUB");
            
            var H2 = Expression.Sum((X, Parameters).ZipWith((x, y) => xi[x] * y.MusclePercent));
            var H3 = Expression.Sum((X, Parameters).ZipWith((x, y) => xi[x] * y.OrganPercent));
            var H4 = Expression.Sum((X, Parameters).ZipWith((x, y) => xi[x] * y.HeartPercent));
            var H5 = Expression.Sum((X, Parameters).ZipWith((x, y) => xi[x] * y.BonePercent));

            var sumXi = Expression.Sum(X.Select(x => xi[x]));

            model.AddConstraint(sumXi * (TargetMusclePercent - Sigma) <= H2, "Muscle%UB");
            model.AddConstraint(sumXi * (TargetOrganPercent - Sigma) <= H3, "Organ%UB");
            model.AddConstraint(sumXi * (TargetHeartPercent - Sigma) <= H4, "Heart%UB");
            model.AddConstraint(sumXi * (TargetBonePercent - Sigma) <= H5, "Bone%UB");

            model.AddConstraint(H2 <= sumXi * (TargetMusclePercent + Sigma), "Muscle%LB");
            model.AddConstraint(H3 <= sumXi * (TargetOrganPercent + Sigma), "Organ%LB");
            model.AddConstraint(H4 <= sumXi * (TargetHeartPercent + Sigma), "Heart%LB");
            model.AddConstraint(H5 <= sumXi * (TargetBonePercent + Sigma), "Bone%LB");

            // solving
            using var s = solver;
            var solution = s.Solve(model);

            // solution reporting
            if (solution.ModelStatus is not ModelStatus.Feasible)
            {
                foreach (var param in Parameters) param.Solution = null;
                ObjectiveValue = ActualMusclePercent = ActualOrganPercent = ActualHeartPercent = ActualBonePercent = null;
                throw new InvalidOperationException("Model is not feasible!");
            }

            ObjectiveValue = solution.GetObjectiveValue("Maximize");
            ActualMusclePercent = ActualOrganPercent = ActualHeartPercent = ActualBonePercent = 0;
            
            foreach (var (i, y) in X.Zip(Parameters))
            {
                y.Solution = xi[i].Value;
                ActualMusclePercent += xi[i].Value * y.MusclePercent;
                ActualOrganPercent += xi[i].Value * y.OrganPercent;
                ActualHeartPercent += xi[i].Value * y.HeartPercent;
                ActualBonePercent += xi[i].Value * y.BonePercent;
            }

            ActualMusclePercent /= ObjectiveValue;
            ActualOrganPercent /= ObjectiveValue;
            ActualHeartPercent /= ObjectiveValue;
            ActualBonePercent /= ObjectiveValue;

            return ObjectiveValue;
        }
    }
}
