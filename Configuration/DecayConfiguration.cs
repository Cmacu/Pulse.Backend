using System;

namespace Pulse.Configuration {
  public class DecayConfiguration {
    public readonly DateTime StartAt = new DateTime(2020, 08, 19);
    public readonly DateTime EndAt = new DateTime(2020, 09, 23);
    public readonly int TotalSteps = 7;
    public readonly int[] StepAmounts = new int[] { 70, 60, 50, 40, 30, 20, 10 };
  }
}