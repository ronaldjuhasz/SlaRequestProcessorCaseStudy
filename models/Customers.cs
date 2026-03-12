// Immutable record: customer with billing and meter info.
public sealed record Customer(
    string CustomerId,
    string Name,
    bool HasUnpaidInvoice,
    string SLA,
    string MeterType
);
