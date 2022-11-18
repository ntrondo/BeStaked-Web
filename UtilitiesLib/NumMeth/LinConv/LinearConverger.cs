namespace UtilitiesLib.NumMeth.LinConv
{
    public class LinearConverger
    {
        public double[] FirstGuess { get; }
        public LinearConverger(double target, Func<double,double> function, double accuracy, double firstGuess, double secondGuess)
        {
            this.Target = target;
            this.Function = function;
            this.Accuracy = accuracy;
            this.FirstGuess = new double[]
            {
                firstGuess,
                secondGuess
            };
        }
       
        public bool Converge()
        {
            ConvergenceIteration? iteration = null;
            do
            {
                var input = iteration == null ? this.FirstGuess : iteration.NewInputs;                
                var output = input.Select(i => this.Function(i)).ToArray();
                var function = new ExtrapolatedLinearFunction(new LinearizationData(input, output));
                iteration = new ConvergenceIteration(function, Target, Function);
                this.Iterations.Add(iteration);
                //iteration.Log(logger);
            }
            while ((this.IsStarting || this.IsConverging) && !this.IsCompleted);
            return this.IsCompleted;
        }
        public readonly List<ConvergenceIteration> Iterations = new();

        public double Target { get; }
        public Func<double, double> Function { get; }
        public double Accuracy { get; }
        public double Result
        {
            get
            {
                var last = this.Iterations.LastOrDefault();
                if (last == null)
                    return 0;
                return last.ResultInput;
            }
        }
        private bool IsStarting
        {
            get
            {
                return this.Iterations.Count < 2;
            }
        }
        private bool IsConverging
        {
            get
            {
                var iterations = this.Iterations.TakeLast(2).ToArray();
                if (iterations.Length < 2)
                    return true;
                return iterations[0].AbsoluteDifference > iterations[1].AbsoluteDifference;
            }
        }

        public bool IsCompleted
        {
            get
            {
                var iteration = this.Iterations.LastOrDefault();
                if (iteration == null)
                    return false;
                return iteration.AbsoluteDifference <= this.Accuracy;
            }
        }
    }
}
