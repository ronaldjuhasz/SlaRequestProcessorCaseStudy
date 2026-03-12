// Entry point: validates input CSVs and processes tariff-switch requests.
const int ExitSuccess = 0;
const int ExitFailure = 1;

// Reject multiple arguments.
if (args.Length > 1)
{
    Console.Error.WriteLine("Too many arguments. Usage: dotnet run -- \"[input folder]\"");
    return ExitFailure;
}

string inputPath = InputPathResolver.DecidePathToFiles(args);

// Validate folder, required files, and CSV schema in one pass.
string? fileValidationMessage = InputPathValidator.ValidateFolderAndFiles(inputPath);
if (fileValidationMessage != null)
{
    Console.Error.WriteLine(fileValidationMessage);
    return ExitFailure;
}

InputFiles files = InputPathValidator.BuildInputFiles(inputPath);

// Run main request processing pipeline.
var processor = new RequestProcessor(files);
processor.Run();

return ExitSuccess;