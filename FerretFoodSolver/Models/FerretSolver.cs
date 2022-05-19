using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gurobi;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace FerretFoodSolver.Models
{
    public class FerretMenuItem : ReactiveObject
    {
        /// Mx
        [Reactive] public double MusclePercent { get; set; }
        /// Ox
        [Reactive] public double OrganPercent { get; set; }
        /// Hx
        [Reactive] public double HeartPercent { get; set; }
        /// Bx
        [Reactive] public double BonePercent { get; set; }
        /// Wx
        [Reactive] public double WeightConversion { get; set; }
        /// Final Solution
        [Reactive] public double? Solution { get; set; }

        public FerretMenuItem()
        {
            MusclePercent = 0;
            OrganPercent = 0;
            HeartPercent = 0;
            BonePercent = 0;
            WeightConversion = 1;
        }
    }

    /// <summary>
    /// Minimize 
    /// H = sum for i in xi Wi*i
    /// subject to
    /// sum for i in xi Wi*i <= W'
    /// M' - sigma <= (sum i in xi Mi*i)/(sum xi) <= M' + sigma
    /// O' - sigma <= (sum i in xi Oi*i)/(sum xi) <= O' + sigma
    /// H' - sigma <= (sum i in xi Hi*i)/(sum xi) <= H' + sigma
    /// B' - sigma <= (sum i in xi Bi*i)/(sum xi) <= B' + sigma
    /// i >= 0, forall i in xi
    /// </summary>
    public class FerretSolver : ReactiveObject
    {
        public BindingList<FerretMenuItem> Parameters { get; set; } = new();

        /// <summary>
        /// W'
        /// </summary>
        [Reactive] public double TargetWeight { get; set; } = 0.0;
        /// <summary>
        /// M'
        /// </summary>
        [Reactive] public double TargetMusclePercent { get; set; } = 0.0;
        /// <summary>
        /// O'
        /// </summary>
        [Reactive] public double TargetOrganPercent { get; set; } = 0.0;
        /// <summary>
        /// H'
        /// </summary>
        [Reactive] public double TargetHeartPercent { get; set; } = 0.0;
        /// <summary>
        /// B'
        /// </summary>
        [Reactive] public double TargetBonePercent { get; set; } = 0.0;
        /// <summary>
        /// Allowable flexibility
        /// </summary>
        [Reactive] public double Sigma { get; set; } = 0.05;

        /// <summary>
        /// The result of the most recent <see cref="Solve"/>.
        /// </summary>
        [Reactive] public double? ObjectiveValue { get; private set; }

        [Reactive] public double? ActualMusclePercent { get; private set; }
        [Reactive] public double? ActualOrganPercent { get; private set; }
        [Reactive] public double? ActualHeartPercent { get; private set; }
        [Reactive] public double? ActualBonePercent { get; private set; }

        /// <summary>
        /// Runs a integer programming optimizer on the given inputs.
        /// </summary>
        /// <returns>null if not optimizable, else objective value</returns>
        public double? Solve()
        {
            using var env = new GRBEnv(true);
            env.Start();

            using var model = new GRBModel(env);

            // Maximize 
            // H = sum for i in xi Wi*i
            // subject to
            // sum for i in xi Wi*i <= W'
            // M' - sigma <= (sum i in xi Mi*i)/(sum xi) <= M' + sigma
            // O' - sigma <= (sum i in xi Oi*i)/(sum xi) <= O' + sigma
            // H' - sigma <= (sum i in xi Hi*i)/(sum xi) <= H' + sigma
            // B' - sigma <= (sum i in xi Bi*i)/(sum xi) <= B' + sigma
            // i >= 0, forall i in xi

            var xi = model.AddVars(Parameters.Count, GRB.CONTINUOUS);

            var H1 = new GRBLinExpr();

            foreach (var (i, y) in xi.Zip(Parameters)) H1 += i * y.WeightConversion;

            model.SetObjective(H1, GRB.MAXIMIZE);

            model.AddConstr(H1 >= TargetWeight - Sigma, "H1");
            model.AddConstr(H1 <= TargetWeight, "H1p");

            var sumXi = new GRBLinExpr();
            foreach (var i in xi) sumXi += i;

            var H2 = new GRBLinExpr();
            var H3 = new GRBLinExpr();
            var H4 = new GRBLinExpr();
            var H5 = new GRBLinExpr();
            foreach (var (i, y) in xi.Zip(Parameters))
            {
                H2 += i * y.MusclePercent;
                H3 += i * y.OrganPercent;
                H4 += i * y.HeartPercent;
                H5 += i * y.BonePercent;
            }

            model.AddConstr(sumXi * (TargetMusclePercent - Sigma) <= H2, "H2");
            model.AddConstr(sumXi * (TargetOrganPercent - Sigma) <= H3, "H4");
            model.AddConstr(sumXi * (TargetHeartPercent - Sigma) <= H4, "H6");
            model.AddConstr(sumXi * (TargetBonePercent - Sigma) <= H5, "H8");

            model.AddConstr(H2 <= sumXi * (TargetMusclePercent + Sigma), "H3");
            model.AddConstr(H3 <= sumXi * (TargetOrganPercent + Sigma), "H5");
            model.AddConstr(H4 <= sumXi * (TargetHeartPercent + Sigma), "H7");
            model.AddConstr(H5 <= sumXi * (TargetBonePercent + Sigma), "H9");

            model.Optimize();

            if (model.Status is GRB.Status.INFEASIBLE or GRB.Status.INF_OR_UNBD or GRB.Status.UNBOUNDED)
            {
                foreach (var (i, y) in xi.Zip(Parameters)) y.Solution = null;
                return ObjectiveValue = ActualMusclePercent = ActualOrganPercent = ActualHeartPercent = ActualBonePercent = null;
            }
            else
            {
                ObjectiveValue = model.ObjVal;
                ActualMusclePercent = ActualOrganPercent = ActualHeartPercent = ActualBonePercent = 0;
                foreach (var (i, y) in xi.Zip(Parameters))
                {
                    y.Solution = i.X;
                    ActualMusclePercent += i.X * y.MusclePercent;
                    ActualOrganPercent += i.X * y.OrganPercent;
                    ActualHeartPercent += i.X * y.HeartPercent;
                    ActualBonePercent += i.X * y.BonePercent;
                }

                ActualMusclePercent /= ObjectiveValue;
                ActualOrganPercent /= ObjectiveValue;
                ActualHeartPercent /= ObjectiveValue;
                ActualBonePercent /= ObjectiveValue;

                return ObjectiveValue;
            }
        }
    }
}
