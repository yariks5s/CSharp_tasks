using MvcTasksApp.Models.Integrals;

namespace MvcTasksApp.Services
{
    public interface IIntegrationService
    {
        TrapezoidalIntegrationResult ComputeIntegration(double a, double b, int n);
    }
}
