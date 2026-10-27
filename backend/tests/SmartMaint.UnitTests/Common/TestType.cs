namespace SmartMaint.UnitTests.Common;

/// <summary>
/// Report 5 classification. Every test carries exactly one: [Trait(TestType.Key, TestType.Normal)].
/// Every API needs at least one Normal + one Abnormal + one Boundary test.
/// Filter: dotnet test --filter "Type=Boundary"
/// </summary>
public static class TestType
{
    public const string Key = "Type";
    public const string Normal = "Normal";
    public const string Abnormal = "Abnormal";
    public const string Boundary = "Boundary";
}
