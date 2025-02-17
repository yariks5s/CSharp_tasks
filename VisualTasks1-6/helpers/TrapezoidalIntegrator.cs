using System;
using System.Linq;

namespace MyProject.Domain
{
    public class TrapezoidalIntegrator
    {
        public double A { get; }
        public double B { get; }
        public int N { get; }

        public double[] XValues { get; private set; }
        public double[] YValues { get; private set; }
        public double Integral { get; private set; }

        public TrapezoidalIntegrator(double a, double b, int n)
        {
            A = a;
            B = b;
            N = n;
        }

        public void Compute(Func<double, double> f)
        {
            double h = (B - A) / N;
            XValues = new double[N + 1];
            YValues = new double[N + 1];

            for (int i = 0; i <= N; i++)
            {
                double x = A + i * h;
                XValues[i] = x;
                YValues[i] = f(x);
            }

            // Обчислення інтегралу методом трапецій
            Integral = YValues[0] / 2.0 + YValues[N] / 2.0;
            for (int i = 1; i < N; i++)
            {
                Integral += YValues[i];
            }
            Integral *= h;
        }
    }
}
