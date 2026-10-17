namespace TheSingularityWorkshop.Physics.Numerics;

/// <summary>
/// Allocation-free structure-of-arrays operations for batches of three-dimensional values.
/// </summary>
/// <remarks>
/// Each component is stored in its own span so callers can map the same logical layout to
/// structure-of-arrays CPU buffers or future GPU buffers. This is a CPU reference implementation,
/// not a GPU backend. Inputs and outputs may alias at matching indices; arbitrary shifted overlap
/// is not promised. All spans participating in an operation must have the same length.
/// </remarks>
public static class Vector3dBatch
{
    /// <summary>Computes component-wise vector addition into caller-owned output spans.</summary>
    public static void Add(
        ReadOnlySpan<double> leftX, ReadOnlySpan<double> leftY, ReadOnlySpan<double> leftZ,
        ReadOnlySpan<double> rightX, ReadOnlySpan<double> rightY, ReadOnlySpan<double> rightZ,
        Span<double> outputX, Span<double> outputY, Span<double> outputZ)
    {
        ValidateLengths(leftX.Length, leftY.Length, leftZ.Length, rightX.Length, rightY.Length,
            rightZ.Length, outputX.Length, outputY.Length, outputZ.Length);

        for (var i = 0; i < outputX.Length; i++)
        {
            outputX[i] = leftX[i] + rightX[i];
            outputY[i] = leftY[i] + rightY[i];
            outputZ[i] = leftZ[i] + rightZ[i];
        }
    }

    /// <summary>Scales each vector by the same finite scalar into caller-owned output spans.</summary>
    public static void Scale(
        ReadOnlySpan<double> inputX, ReadOnlySpan<double> inputY, ReadOnlySpan<double> inputZ,
        double scalar,
        Span<double> outputX, Span<double> outputY, Span<double> outputZ)
    {
        if (!double.IsFinite(scalar))
            throw new ArgumentOutOfRangeException(nameof(scalar), "Scale must be finite.");

        ValidateLengths(inputX.Length, inputY.Length, inputZ.Length, outputX.Length, outputY.Length, outputZ.Length);

        for (var i = 0; i < outputX.Length; i++)
        {
            outputX[i] = inputX[i] * scalar;
            outputY[i] = inputY[i] * scalar;
            outputZ[i] = inputZ[i] * scalar;
        }
    }

    /// <summary>Computes one dot product per vector pair into a caller-owned output span.</summary>
    public static void Dot(
        ReadOnlySpan<double> leftX, ReadOnlySpan<double> leftY, ReadOnlySpan<double> leftZ,
        ReadOnlySpan<double> rightX, ReadOnlySpan<double> rightY, ReadOnlySpan<double> rightZ,
        Span<double> output)
    {
        ValidateLengths(leftX.Length, leftY.Length, leftZ.Length, rightX.Length, rightY.Length, rightZ.Length, output.Length);

        for (var i = 0; i < output.Length; i++)
            output[i] = (leftX[i] * rightX[i]) + (leftY[i] * rightY[i]) + (leftZ[i] * rightZ[i]);
    }

    private static void ValidateLengths(params int[] lengths)
    {
        var expected = lengths[0];
        for (var i = 1; i < lengths.Length; i++)
        {
            if (lengths[i] != expected)
                throw new ArgumentException("All component and output spans must have equal lengths.");
        }
    }
}
