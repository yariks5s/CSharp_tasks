using MvcTasksApp.Services;
using MvcTasksApp.Models.Integrals;

namespace MvcTasksApp.Helpers
{
    public static class IntegrationHelper
    {
        public static TrapezoidalIntegrationResult ComputeIntegration(IIntegrationService integrationService, double a, double b, int n)
        {
            return integrationService.ComputeIntegration(a, b, n);
        }
    }
}
