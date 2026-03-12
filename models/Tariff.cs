// Immutable record: tariff with smart meter requirement and pricing.
public sealed record Tariff(
    string TariffId,
    string Name,
    bool RequiresSmartMeter,
    decimal BaseMonthlyGross
);
