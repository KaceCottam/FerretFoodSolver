using System;
using FerretFoodSolver.Engine;
using NUnit.Framework;
using OPTANO.Modeling.GLPK;

namespace Engine.Tests
{
    [TestFixture]
    public class FerretSolverTests
    {
        static Action<FerretSolver, double>[] ModelFunctions =
        {
            (model, value) => model.TargetMusclePercent = value,
            (model, value) => model.TargetOrganPercent = value,
            (model, value) => model.TargetHeartPercent = value,
            (model, value) => model.TargetBonePercent = value,
        };

        FerretSolver FeasibleModel() => new FerretSolver()
        {
            TargetMusclePercent = 0.25,
            TargetOrganPercent = 0.25,
            TargetHeartPercent = 0.25,
            TargetBonePercent = 0.25,
            TargetWeight = 1,
            Sigma = 0.05,
            Parameters = new System.Collections.Generic.List<FerretMenuItem>()
            {
                new FerretMenuItem()
                {
                    MusclePercent = 0.25,
                    OrganPercent = 0.25,
                    HeartPercent = 0.25,
                    BonePercent = 0.25,
                    WeightConversion = 1,
                }
            },
        };
        FerretSolver FeasibleModel2() => new FerretSolver()
        {
            TargetMusclePercent = 0.25,
            TargetOrganPercent = 0.25,
            TargetHeartPercent = 0.25,
            TargetBonePercent = 0.25,
            TargetWeight = 4,
            Sigma = 0.05,
            Parameters = new System.Collections.Generic.List<FerretMenuItem>()
            {
                new FerretMenuItem()
                {
                    MusclePercent = 1.00,
                    OrganPercent = 0.00,
                    HeartPercent = 0.00,
                    BonePercent = 0.00,
                    WeightConversion = 1,
                },
                new FerretMenuItem()
                {
                    MusclePercent = 0.00,
                    OrganPercent = 1.00,
                    HeartPercent = 0.00,
                    BonePercent = 0.00,
                    WeightConversion = 1,
                },
                new FerretMenuItem()
                {
                    MusclePercent = 0.00,
                    OrganPercent = 0.00,
                    HeartPercent = 1.00,
                    BonePercent = 0.00,
                    WeightConversion = 1,
                },
                new FerretMenuItem()
                {
                    MusclePercent = 0.00,
                    OrganPercent = 0.00,
                    HeartPercent = 0.00,
                    BonePercent = 1.00,
                    WeightConversion = 1,
                }
            },
        };

        [Test]
        public void Verify_FeasibleModel_NoErrorsAndCorrect()
        {
            var model = FeasibleModel();
            Assert.That(() => model.Verify(), Throws.Nothing);
            Assert.That(model.ObjectiveValue, Is.Null);
            Assert.That(model.ActualMusclePercent, Is.Null);
            Assert.That(model.ActualOrganPercent, Is.Null);
            Assert.That(model.ActualHeartPercent, Is.Null);
            Assert.That(model.ActualBonePercent, Is.Null);
        }

        [Test]
        public void Solve_FeasibleModel_Correct()
        {
            var model = FeasibleModel();
            model.Sigma = 0;
            Assert.That(() => model.Solve(new GLPKSolver()), Throws.Nothing);
            Assert.That(model.ObjectiveValue, Is.EqualTo(1));
            Assert.That(model.ActualMusclePercent, Is.EqualTo(0.25));
            Assert.That(model.ActualOrganPercent, Is.EqualTo(0.25));
            Assert.That(model.ActualHeartPercent, Is.EqualTo(0.25));
            Assert.That(model.ActualBonePercent, Is.EqualTo(0.25));
            Assert.That(model.Parameters[0].Solution, Is.EqualTo(1));
        }

        [Test]
        public void Solve_FeasibleModel2_Correct()
        {
            var model = FeasibleModel2();
            model.Sigma = 0;
            Assert.That(() => model.Solve(new GLPKSolver()), Throws.Nothing);
            Assert.That(model.ObjectiveValue, Is.EqualTo(4));
            Assert.That(model.ActualMusclePercent, Is.EqualTo(0.25));
            Assert.That(model.ActualOrganPercent, Is.EqualTo(0.25));
            Assert.That(model.ActualHeartPercent, Is.EqualTo(0.25));
            Assert.That(model.ActualBonePercent, Is.EqualTo(0.25));
            Assert.That(model.Parameters[0].Solution, Is.EqualTo(1));
            Assert.That(model.Parameters[1].Solution, Is.EqualTo(1));
            Assert.That(model.Parameters[2].Solution, Is.EqualTo(1));
            Assert.That(model.Parameters[3].Solution, Is.EqualTo(1));
        }

        [Test]
        public void Solve_InFeasibleModel_ThrowsException()
        {
            var model = FeasibleModel();
            model.TargetOrganPercent = 0.00;
            model.TargetHeartPercent = 0.50;
            Assert.That(() => model.Solve(new GLPKSolver()), Throws.InvalidOperationException);
            Assert.That(model.ObjectiveValue, Is.Null);
            Assert.That(model.ActualMusclePercent, Is.Null);
            Assert.That(model.ActualOrganPercent, Is.Null);
            Assert.That(model.ActualHeartPercent, Is.Null);
            Assert.That(model.ActualBonePercent, Is.Null);
            Assert.That(model.Parameters[0].Solution, Is.Null);
        }

        [Test]
        public void Verify_WeightMinL_ThrowsException()
        {
            var model = FeasibleModel();
            model.TargetWeight = -0.01;
            Assert.That(model.Verify, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Verify_WeightMin_ThrowsException()
        {
            var model = FeasibleModel();
            model.TargetWeight = 0;
            Assert.That(model.Verify, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Verify_WeightMinU_NoThrowsException()
        {
            var model = FeasibleModel();
            model.TargetWeight = 0.01;
            Assert.That(model.Verify, Throws.Nothing);
        }

        [Test]
        public void Verify_NoParameters_ThrowsException()
        {
            var model = FeasibleModel();
            model.Parameters.Clear();
            Assert.That(model.Verify, Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentSumMinL_ThrowsException(Action<FerretSolver, double> fn)
        {
            var model = FeasibleModel();
            fn(model, 0.2399);
            Assert.That(model.Verify, Throws.ArgumentException);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentSumMin_NoThrowsException(Action<FerretSolver, double> fn)
        {
            var model = FeasibleModel();
            fn(model, 0.2400);
            Assert.That(model.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentSumMinU_NoThrowsException(Action<FerretSolver, double> fn)
        {
            var model = FeasibleModel();
            fn(model, 0.2401);
            Assert.That(model.Verify, Throws.Nothing);
        }

        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentSumMaxL_NoThrowsException(Action<FerretSolver, double> fn)
        {
            var model = FeasibleModel();
            fn(model, 0.2599);
            Assert.That(model.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentSumMax_NoThrowsException(Action<FerretSolver, double> fn)
        {
            var model = FeasibleModel();
            fn(model, 0.2600);
            Assert.That(model.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentSumMaxU_ThrowsException(Action<FerretSolver, double> fn)
        {
            var model = FeasibleModel();
            fn(model, 0.2601);
            Assert.That(model.Verify, Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentMinL_ThrowsException(Action<FerretSolver, double> fn)
        {
            var model = FeasibleModel();
            model.TargetMusclePercent = 1;
            model.TargetOrganPercent = 0;
            model.TargetHeartPercent = 0;
            model.TargetBonePercent = 0;
            fn(model, -0.0001);
            if (model.TargetMusclePercent != 1) model.TargetOrganPercent = 1;
            Assert.That(model.Verify, Throws.TypeOf<ArgumentOutOfRangeException>());
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentMin_NoThrowsException(Action<FerretSolver, double> fn)
        {
            var model = FeasibleModel();
            model.TargetMusclePercent = 1;
            model.TargetOrganPercent = 0;
            model.TargetHeartPercent = 0;
            model.TargetBonePercent = 0;
            fn(model, 0.0000);
            if (model.TargetMusclePercent != 1) model.TargetOrganPercent = 1;
            Assert.That(model.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentMinU_NoThrowsException(Action<FerretSolver, double> fn)
        {
            var model = FeasibleModel();
            model.TargetMusclePercent = 1;
            model.TargetOrganPercent = 0;
            model.TargetHeartPercent = 0;
            model.TargetBonePercent = 0;
            fn(model, 0.0001);
            if (model.TargetMusclePercent != 1) model.TargetOrganPercent = 1;
            Assert.That(model.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentMaxL_ThrowsException(Action<FerretSolver, double> fn)
        {
            var model = FeasibleModel();
            model.TargetMusclePercent = 0;
            model.TargetOrganPercent = 0;
            model.TargetHeartPercent = 0;
            model.TargetBonePercent = 0;
            fn(model, 0.9999);
            Assert.That(model.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentMax_NoThrowsException(Action<FerretSolver, double> fn)
        {
            var model = FeasibleModel();
            model.TargetMusclePercent = 0;
            model.TargetOrganPercent = 0;
            model.TargetHeartPercent = 0;
            model.TargetBonePercent = 0;
            fn(model, 1.0000);
            Assert.That(model.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentMaxU_NoThrowsException(Action<FerretSolver, double> fn)
        {
            var model = FeasibleModel();
            model.TargetMusclePercent = 0;
            model.TargetOrganPercent = 0;
            model.TargetHeartPercent = 0;
            model.TargetBonePercent = 0;
            fn(model, 1.0001);
            Assert.That(model.Verify, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Verify_ParameterInvalid_ThrowsException()
        {
            var model = FeasibleModel();
            model.Parameters[0].BonePercent = 0.0000;
            Assert.That(model.Verify, Throws.ArgumentException);
        }
    }
}