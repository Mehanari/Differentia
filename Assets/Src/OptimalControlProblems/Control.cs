namespace Src.OptimalControlProblems
{
    public abstract class Control
    {
        public abstract double ControlInput(double time);
        public abstract string ToJson();
    }
}