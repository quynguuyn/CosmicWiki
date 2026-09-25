[Flags]
public enum MetallicityFlags
{
    None = 0,

    /// Result comes from a low signal-to-noise spectrum.
    LowSignalToNoise = 1,

    /// Result is an average of several values.
    Averaged = 2,

    /// Solar [Fe/H] reference was calculated using solar value 7.50.
    SolarAbundanceReferenceCalculated = 4
}