// Core pipeline: loads data, validates requests, decides approvals/rejections.
public class RequestProcessor
{
    private readonly InputFiles files;
	private const string StatusApprove = "Approve";
	private const string StatusReject  = "Reject";
	private const string StatusSkip    = "Skip";

	private readonly DecisionWriter writer;
	private readonly CsvLookupLoader loader;
	private readonly SlaCalculator slaCalculator = new();
	// Constructor allows dependency injection for testability.
	public RequestProcessor(InputFiles files, string? processedIdsFilePath = null, DecisionWriter? writer = null, CsvLookupLoader? loader = null)
	{
		this.files = files;
		this.writer = writer ?? new DecisionWriter();
		this.loader = loader ?? new CsvLookupLoader();
		processedStore = new ProcessedRequestStore(processedIdsFilePath ?? "./processedRequestIDs/processed_ids.csv");
	}	
	private readonly ProcessedRequestStore processedStore;
    public void Run()
	{
		// Load state, then run validation and decision pipeline.
		processedStore.Load();
		var customers = loader.LoadCustomers(files.CustomersPath);
		var tariffs = loader.LoadTariffs(files.TariffsPath);
		writer.Initialize();
		ProcessRequests(customers, tariffs);
	}

	private void ProcessRequests(Dictionary<string, Customer> customers, Dictionary<string, Tariff> tariffs)
	{
		// Stream through requests, apply business rules, write decisions.
		using var reader = new StreamReader(files.RequestsPath);

		string? header = reader.ReadLine();

		if (header == null)
    		return;

		var idx = CsvHeaderMap.BuildHeaderIndex(header);

		string? line;
		while ((line = reader.ReadLine()) != null)
		{
			if (string.IsNullOrWhiteSpace(line))
				continue;

			var cols = line.Split(';');
			string requestId = CsvHeaderMap.Get(cols, idx, "RequestId");

			// Reject if RequestId is missing.
			if (string.IsNullOrWhiteSpace(requestId))
			{
				Console.WriteLine($"{StatusReject}:  (missing RequestId) - Invalid request data.");
				continue;
			}

			// Skip if already processed in a prior run.
			if (processedStore.IsProcessed(requestId))
			{
				Console.WriteLine($"{StatusSkip}:    {requestId} - already processed.");
				continue;
			}

			string customerId = CsvHeaderMap.Get(cols, idx, "CustomerId");
			string tariffId = CsvHeaderMap.Get(cols, idx, "TargetTariffId");
			string requestedAtRaw = CsvHeaderMap.Get(cols, idx, "RequestedAtISO8601");

			// Reject if required fields are missing.
			if (string.IsNullOrWhiteSpace(customerId) || string.IsNullOrWhiteSpace(tariffId) ||	string.IsNullOrWhiteSpace(requestedAtRaw))
			{
				Console.WriteLine($"{StatusReject}:  {requestId} - Invalid request data.");
				writer.AppendDecision(requestId, StatusReject, "Invalid request data", null);
				processedStore.MarkProcessed(requestId);
				continue;
			}		

			// Reject if RequestedAt is unparseable.
			if (!DateTimeOffset.TryParse(requestedAtRaw, out var requestedAt))
			{
				Console.WriteLine($"{StatusReject}:  {requestId} - Invalid request data.");
				writer.AppendDecision(requestId, StatusReject, "Invalid request data", null);
				processedStore.MarkProcessed(requestId);
				continue;
			}

			// Reject if customer not found.
			if (!customers.TryGetValue(customerId, out var customer))
			{
				Console.WriteLine($"{StatusReject}:  {requestId} - Unknown customer.");
				writer.AppendDecision(requestId, StatusReject, "Unknown customer", null);
				processedStore.MarkProcessed(requestId);
				continue;
			}

			// Reject if tariff not found.
			if (!tariffs.TryGetValue(tariffId, out var tariff))
			{
				Console.WriteLine($"{StatusReject}:  {requestId} - Unknown tariff.");
				writer.AppendDecision(requestId, StatusReject, "Unknown tariff", null);
				processedStore.MarkProcessed(requestId);
				continue;
			}

			// Reject if customer has unpaid invoice.
			if (customer.HasUnpaidInvoice)
			{
				Console.WriteLine($"{StatusReject}:  {requestId} - Unpaid invoice.");
				writer.AppendDecision(requestId, StatusReject, "Unpaid invoice", null);
				processedStore.MarkProcessed(requestId);
				continue;
			}

			// Tricky: check if meter upgrade is needed (tariff requires smart but customer doesn't have it).
			bool requiresUpgrade = tariff.RequiresSmartMeter && !customer.MeterType.Equals("Smart", StringComparison.OrdinalIgnoreCase);

			var slaDue = slaCalculator.ComputeSlaDue(requestedAt, customer.SLA, requiresUpgrade);
			Console.WriteLine($"{StatusApprove}: {requestId} - SLA due {SlaCalculator.FormatIso(slaDue) + (requiresUpgrade ? " - Schedule meter upgrade" : "")}");

			// Write approval decision.
			writer.AppendDecision(requestId, StatusApprove, "", slaDue);

			// If upgrade needed, create follow-up task with deadline.
			if (requiresUpgrade)
			{
				var upgradeDeadline = slaCalculator.ComputeUpgradeDeadline(slaDue);
				writer.AppendFollowUp(requestId, "Schedule meter upgrade", upgradeDeadline);
			}

			processedStore.MarkProcessed(requestId);
		}
	}	
}
