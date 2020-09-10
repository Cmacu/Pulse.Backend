namespace Pulse.Ranking.Rating {

  public class RatingConfig {
    public readonly int Offset = 800;
    public readonly int Multiplier = 40;
    public readonly double BetaDivisor = 6.0;
    public readonly double DrawProbability = 0.0;
    public readonly double DynamicsFactorDivisor = 30.0;
    public readonly double InitialMean = 25.0;
    public readonly double InitialDeviation = 8.0;
    public readonly double MinDeviation = 2.5;
    public readonly double MaxDelta = 2.25;
    public readonly double MinDelta = 0.25;
    public readonly int ConservativeMultiplier = 3;
  }
}