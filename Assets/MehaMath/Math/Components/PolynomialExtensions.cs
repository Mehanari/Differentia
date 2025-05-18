namespace MehaMath.Math.Components
{
	public static class PolynomialExtensions
	{
		/// <summary>
		/// Returns a tangent line function in a form of polynomial for a given x.
		/// </summary>
		/// <param name="polynomial"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		public static Polynomial TangentLine(this Polynomial polynomial, double x)
		{
			var derivative = polynomial.Derivative();
			var derivX = derivative.Compute(x);
			var fX = polynomial.Compute(x);
			return new Polynomial(fX - x * derivX, derivX);
		}
	}
}