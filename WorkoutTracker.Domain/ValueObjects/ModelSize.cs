using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Domain.ValueObjects;

public struct ModelSize : IComparable<ModelSize>
{
    public double Size { get; set; }
    public SizeType SizeType { get; set; }


    const double oneCentimeterInInches = 0.393701;
    public double GetSizeInInches()
    {
        if (SizeType == SizeType.Inch)
            return Size;

        return Size * oneCentimeterInInches;
    }

    public static ModelSize GetModelSizeInInches(ModelSize modelSize)
    {
        if (modelSize.SizeType == SizeType.Inch)
            return modelSize;

        double sizeInInches = modelSize.GetSizeInInches();
        return new ModelSize() { SizeType = SizeType.Inch, Size = sizeInInches };
    }


    const double oneInchInCentimeters = 2.54;
    public double GetSizeInCentimeters()
    {
        if (SizeType == SizeType.Centimeter)
            return Size;

        return Size * oneInchInCentimeters;
    }

    public static ModelSize GetModelSizeInCentimeters(ModelSize modelSize)
    {
        if (modelSize.SizeType == SizeType.Centimeter)
            return modelSize;

        double sizeInCentimeters = modelSize.GetSizeInCentimeters();
        return new ModelSize() { SizeType = SizeType.Centimeter, Size = sizeInCentimeters };
    }

    public int CompareTo(ModelSize other)
    {
        double sizeInCentimeters = GetSizeInCentimeters(), otherSizeInCentimeters = other.GetSizeInCentimeters();
        return sizeInCentimeters.CompareTo(otherSizeInCentimeters);
    }
}


