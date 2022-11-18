
namespace UtilitiesLib.NumMeth.LinConv
{
    public class ConvergenceIteration
    {
        public ExtrapolatedLinearFunction ApproximationFunction { get; }
        public double TargetOutput { get; }
        public Func<double, double> ActualFunction { get; }

        public ConvergenceIteration(ExtrapolatedLinearFunction approximationFunction, double targetOutput, Func<double, double> actualFunction)
        {
            this.ApproximationFunction = approximationFunction;
            this.TargetOutput = targetOutput;
            this.ActualFunction = actualFunction;
        }
        private double? _resultInput;
        public double ResultInput
        {
            get
            {
                if (this._resultInput == null)
                    this._resultInput = this.ApproximationFunction.Inverse(this.TargetOutput);
                return this._resultInput.Value;
            }
        }
        private double? _resultOutput;
        private double ResultOutput
        {
            get
            {
                if (this._resultOutput == null)
                    this._resultOutput = this.ActualFunction(this.ResultInput);
                return this._resultOutput.Value;
            }
        }
        private double[]? _newInputs;
        public double[] NewInputs
        {
            get
            {
                if(this._newInputs == null)
                {
                    this._newInputs = new double[]
                    {
                        this.ResultInput,
                        this.ApproximationFunction.Inverse(this.ResultOutput)
                    };          
                }
                return this._newInputs;
            }
        }
        private double? _absoluteDifference;
        public double AbsoluteDifference
        {
            get
            {
                if(this._absoluteDifference == null)
                    this._absoluteDifference = System.Math.Abs(this.TargetOutput - this.ResultOutput);
                return this._absoluteDifference.Value;
            }
        }
    }
}
