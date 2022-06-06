using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FerretFoodSolver.Engine;
using NUnit.Framework;

namespace Engine.Tests
{
    public class FerretMenuItemTests
    {
        static Action<FerretMenuItem, double>[] ModelFunctions =
        {
            (model, value) => model.MusclePercent = value,
            (model, value) => model.OrganPercent = value,
            (model, value) => model.HeartPercent = value,
            (model, value) => model.BonePercent = value,
        };

        private FerretMenuItem FeasibleItem() => new FerretMenuItem()
        {
            MusclePercent = 0.25,
            OrganPercent = 0.25,
            HeartPercent = 0.25,
            BonePercent = 0.25,
            WeightConversion = 1,
        };

        [Test]
        public void Verify_WeightMinL_ThrowsException()
        {
            var item = FeasibleItem();
            item.WeightConversion = -0.01;
            Assert.That(item.Verify, Throws.TypeOf<ArgumentOutOfRangeException>());
        }
        [Test]
        public void Verify_WeightMin_ThrowsException()
        {
            var item = FeasibleItem();
            item.WeightConversion = 0.00;
            Assert.That(item.Verify, Throws.TypeOf<ArgumentOutOfRangeException>());
        }
        [Test]
        public void Verify_WeightMinU_NoThrowsException()
        {
            var item = FeasibleItem();
            item.WeightConversion = 0.01;
            Assert.That(item.Verify, Throws.Nothing);
        }

        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_PercentSumMinL_ThrowsException(Action<FerretMenuItem, double> fn)
        {
            var item = FeasibleItem();
            fn(item, 0.2399);
            Assert.That(item.Verify, Throws.ArgumentException);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_PercentSumMin_NoThrowsException(Action<FerretMenuItem, double> fn)
        {
            var item = FeasibleItem();
            fn(item, 0.2400);
            Assert.That(item.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_PercentSumMinU_NoThrowsException(Action<FerretMenuItem, double> fn)
        {
            var item = FeasibleItem();
            fn(item, 0.2401);
            Assert.That(item.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentMinL_ThrowsException(Action<FerretMenuItem, double> fn)
        {
            var model = FeasibleItem();
            model.MusclePercent = 1;
            model.OrganPercent = 0;
            model.HeartPercent = 0;
            model.BonePercent = 0;
            fn(model, -0.0001);
            if (model.MusclePercent != 1) model.OrganPercent = 1;
            Assert.That(model.Verify, Throws.TypeOf<ArgumentOutOfRangeException>());
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentMin_NoThrowsException(Action<FerretMenuItem, double> fn)
        {
            var model = FeasibleItem();
            model.MusclePercent = 1;
            model.OrganPercent = 0;
            model.HeartPercent = 0;
            model.BonePercent = 0;
            fn(model, 0.0000);
            if (model.MusclePercent != 1) model.OrganPercent = 1;
            Assert.That(model.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentMinU_NoThrowsException(Action<FerretMenuItem, double> fn)
        {
            var model = FeasibleItem();
            model.MusclePercent = 1;
            model.OrganPercent = 0;
            model.HeartPercent = 0;
            model.BonePercent = 0;
            fn(model, 0.0001);
            if (model.MusclePercent != 1) model.OrganPercent = 1;
            Assert.That(model.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentMaxL_ThrowsException(Action<FerretMenuItem, double> fn)
        {
            var model = FeasibleItem();
            model.MusclePercent = 0;
            model.OrganPercent = 0;
            model.HeartPercent = 0;
            model.BonePercent = 0;
            fn(model, 0.9999);
            Assert.That(model.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentMax_NoThrowsException(Action<FerretMenuItem, double> fn)
        {
            var model = FeasibleItem();
            model.MusclePercent = 0;
            model.OrganPercent = 0;
            model.HeartPercent = 0;
            model.BonePercent = 0;
            fn(model, 1.0000);
            Assert.That(model.Verify, Throws.Nothing);
        }
        [Test]
        [TestCaseSource(nameof(ModelFunctions))]
        public void Verify_TargetPercentMaxU_NoThrowsException(Action<FerretMenuItem, double> fn)
        {
            var model = FeasibleItem();
            model.MusclePercent = 0;
            model.OrganPercent = 0;
            model.HeartPercent = 0;
            model.BonePercent = 0;
            fn(model, 1.0001);
            Assert.That(model.Verify, Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
