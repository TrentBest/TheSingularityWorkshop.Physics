namespace TheSingularityWorkshop.Physics.Combustion;

/// <summary>
/// Renderer-independent state for one surface cell in a proposed combustion grid.
/// Inventories are cell quantities, not normalized texture colors.
/// </summary>
/// <param name="CellId">Stable identity for this cell across replay and updates.</param>
/// <param name="MaterialId">Resolved material identity; property evidence lives in the material catalog.</param>
/// <param name="AreaSquareMeters">Surface area represented by this cell, in square metres.</param>
/// <param name="FuelMassKilograms">Remaining fuel inventory represented by this cell, in kilograms.</param>
/// <param name="OxygenMassKilograms">Available oxygen inventory in the declared cell control volume, in kilograms.</param>
/// <param name="ThermalEnergyJoules">Thermal energy relative to the model's declared reference state, in joules.</param>
/// <param name="NormalX">X component of the unit surface normal in the declared coordinate frame.</param>
/// <param name="NormalY">Y component of the unit surface normal in the declared coordinate frame.</param>
/// <param name="NormalZ">Z component of the unit surface normal in the declared coordinate frame.</param>
/// <remarks>
/// This is a state contract only. It does not calculate reaction rates, temperature, ignition,
/// transport, or a GPU texture layout. Oxygen control-volume depth and thermal reference/state
/// model must be declared by the consuming model.
/// </remarks>
public readonly record struct SurfaceCombustionCellState(
    string CellId,
    string MaterialId,
    double AreaSquareMeters,
    double FuelMassKilograms,
    double OxygenMassKilograms,
    double ThermalEnergyJoules,
    double NormalX,
    double NormalY,
    double NormalZ)
{
    private const double NormalLengthSquaredTolerance = 1e-6d;

    /// <summary>Whether identities, inventories, geometry, and normal are valid and finite.</summary>
    public bool IsValid
    {
        get
        {
            var normalLengthSquared =
                NormalX * NormalX + NormalY * NormalY + NormalZ * NormalZ;

            return !string.IsNullOrWhiteSpace(CellId) &&
                !string.IsNullOrWhiteSpace(MaterialId) &&
                double.IsFinite(AreaSquareMeters) && AreaSquareMeters > 0d &&
                double.IsFinite(FuelMassKilograms) && FuelMassKilograms >= 0d &&
                double.IsFinite(OxygenMassKilograms) && OxygenMassKilograms >= 0d &&
                double.IsFinite(ThermalEnergyJoules) && ThermalEnergyJoules >= 0d &&
                double.IsFinite(NormalX) && double.IsFinite(NormalY) && double.IsFinite(NormalZ) &&
                double.IsFinite(normalLengthSquared) &&
                Math.Abs(normalLengthSquared - 1d) <= NormalLengthSquaredTolerance;
        }
    }

    /// <summary>Throws when the surface cell contains invalid physical state or geometry.</summary>
    public void EnsureValid()
    {
        if (!IsValid)
            throw new ArgumentException(
                "Surface cell identities, non-negative inventories, area, energy, and unit normal must be valid.",
                nameof(SurfaceCombustionCellState));
    }
}
