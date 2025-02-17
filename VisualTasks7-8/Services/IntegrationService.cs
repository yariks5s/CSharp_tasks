using System;
using MvcTasksApp.Models.Integrals;

namespace MvcTasksApp.Services
{
    public class IntegrationService : IIntegrationService
    {
        public TrapezoidalIntegrationResult ComputeIntegration(double a, double b, int n)
        {
            var integrator = new TrapezoidalIntegrator(a, b, n);
            integrator.Compute(Math.Sin);
            return new TrapezoidalIntegrationResult
            {
                Integral = integrator.Integral,
                ChartBase64 = integrator.GenerateChartBase64()
            };
        }
    }
}
