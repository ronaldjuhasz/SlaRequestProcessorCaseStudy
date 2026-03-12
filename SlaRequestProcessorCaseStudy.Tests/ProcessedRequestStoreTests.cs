namespace SlaRequestProcessorCaseStudy.Tests;
// Verifies persistence of processed request IDs with file lifecycle handling.
public class ProcessedRequestStoreTests
{
    [Fact]
    // Missing file is created on load and returns count=0.
    public void Load_FileMissing_CreatesFolderAndFile_AndLoadsNothing()
    {
        // Arrange
        string tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string filePath = Path.Combine(tempRoot, "processedRequestIDs", "processed_ids.csv");

        var store = new ProcessedRequestStore(filePath);

        // Act
        int count = store.Load();

        // Assert
        Assert.Equal(0, count);
        Assert.True(Directory.Exists(Path.GetDirectoryName(filePath)!));
        Assert.True(File.Exists(filePath));
        Assert.False(store.IsProcessed("123"));
    }

    [Fact]
    // Existing IDs load into memory, whitespace trimmed, duplicates removed.
    public void Load_FileHasIds_LoadsThemAndTrimsWhitespace()
    {
        // Arrange
        string tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string filePath = Path.Combine(tempRoot, "processedRequestIDs", "processed_ids.csv");

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        File.WriteAllLines(filePath, new[]
        {
            "abc",
            "  def  ",
            "",
            "   ",
            "abc" // duplicate
        });

        var store = new ProcessedRequestStore(filePath);

        // Act
        int count = store.Load();

        // Assert
        Assert.Equal(2, count);
        Assert.True(store.IsProcessed("abc"));
        Assert.True(store.IsProcessed("def"));
        Assert.False(store.IsProcessed("ghi"));
    }

    [Fact]
    // New ID appends to file and IsProcessed returns true (tricky: idempotent add).
    public void MarkProcessed_AppendsToFile_AndIsProcessedBecomesTrue()
    {
        // Arrange
        string tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string filePath = Path.Combine(tempRoot, "processedRequestIDs", "processed_ids.csv");

        var store = new ProcessedRequestStore(filePath);
        store.Load(); // ensures folder + file exist

        // Act
        store.MarkProcessed("xyz");

        // Assert
        Assert.True(store.IsProcessed("xyz"));
        string content = File.ReadAllText(filePath);
        Assert.Contains("xyz", content);
    }
}
