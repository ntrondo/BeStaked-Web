namespace UtilitiesLib.NumMeth.LinConv
{
    public interface IFunction
    {
        double Execute(double input);
    }
    public interface IInversibleFunction:IFunction
    {
        double Inverse(double output);
    }
    public abstract class LinearFunction : IInversibleFunction
    {
        public abstract double Inclination { get; }
        public abstract double Constant { get; }
        public double Execute(double input)
        {
            double output = Inclination * input + Constant;
            return output;
        }
        public double Inverse(double output)
        {
            double input = (output - Constant) / Inclination;
            return input;
        }
    }
    public class ExtrapolatedLinearFunction : LinearFunction
    {
        public LinearizationData Data { get; }
        public ExtrapolatedLinearFunction(LinearizationData data)
        {
            this.Data = data;
        }
        private double? _inclination;
        public override double Inclination
        {
            get
            {
                if (this._inclination == null)
                {
                    this._inclination = (Data.Outputs[1] - Data.Outputs[0]) / (Data.Inputs[1] - Data.Inputs[0]);
                }
                return this._inclination.Value;
            }
        }

        private double? _constant;
        public override double Constant
        {
            get
            {
                if (this._constant == null)
                {
                    this._constant = this.Data.Outputs[0] - this.Inclination * this.Data.Inputs[0];
                }
                return this._constant.Value;
            }
        }
    }
    public class LinearizationData
    {
        public double[] Inputs { get; set; }
        public double[] Outputs { get; set; }

        public LinearizationData(double[] inputs, double[] outputs)
        {
            Inputs = inputs;
            Outputs = outputs;
        }
    }
}
