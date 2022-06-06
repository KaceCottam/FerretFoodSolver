using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FerretFoodSolver.Engine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OPTANO.Modeling.GLPK;

namespace FerretSolverApi.Controllers
{
    public class FerretSolverMenuItemParameters
    {
        public string? description { get; set; }
        public double weight { get; set; }
        public double musclePercent { get; set; }
        public double organPercent { get; set; }
        public double heartPercent { get; set; }
        public double bonePercent { get; set; }

        public FerretMenuItem ToFerretMenuItem() => new FerretMenuItem
        {
            Description = description,
            WeightConversion = weight,
            MusclePercent = musclePercent,
            OrganPercent = organPercent,
            HeartPercent = heartPercent,
            BonePercent = bonePercent,
        };
    }
    public class FerretSolverParameters
    {
        public double targetWeight { get; set; }
        public double targetMusclePercent { get; set; }
        public double targetOrganPercent { get; set; }
        public double targetHeartPercent { get; set; }
        public double targetBonePercent { get; set; }
        public double sigma { get; set; }
        public IList<FerretSolverMenuItemParameters> ingredients { get; set; }

        public FerretSolver ToFerretSolver() => new FerretSolver
        {
            TargetWeight = targetWeight,
            TargetMusclePercent = targetMusclePercent,
            TargetOrganPercent = targetOrganPercent,
            TargetHeartPercent = targetHeartPercent,
            TargetBonePercent = targetBonePercent,
            Sigma = sigma,
            Parameters = ingredients.Select(i => i.ToFerretMenuItem()).ToList()
        };
    }

    public static class ApiExtensions
    {
        public static object ToOkResponse(this FerretSolver @this) =>
            new
            {
                actualWeight = @this.ObjectiveValue,
                actualMusclePercent = @this.ActualMusclePercent,
                actualOrganPercent = @this.ActualOrganPercent,
                actualHeartPercent = @this.ActualHeartPercent,
                actualBonePercent = @this.ActualBonePercent,
                ingredients = @this.Parameters.Select(i =>
                    new
                    {
                        description = i.Description,
                        optimalNumber = i.Solution,
                    }
                    ).ToList(),
            };
    }

    [ApiController]
    [Route("/solve")]
    public class FerretSolverController : ControllerBase
    {
        [HttpPost]
        public IActionResult Solve(FerretSolverParameters parameters)
        {
            var solver = parameters.ToFerretSolver();
            try
            {
                solver.Verify();
                solver.Solve(new GLPKSolver());
                return Ok(solver.ToOkResponse());
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    invalidAssumptions = new
                    {
                        item = ex.ParamName,
                        message = ex.Message,
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    unfeasibleModel = new
                    {
                        message = ex.Message,
                    }
                });
            }
        }
    }
}
