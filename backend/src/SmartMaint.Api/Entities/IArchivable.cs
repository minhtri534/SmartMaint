namespace SmartMaint.Api.Entities;

/// <summary>A catalog item that is hidden from the pick lists but stays on the records that already use it.</summary>
public interface IArchivable
{
    DateTimeOffset? ArchivedAt { get; set; }
}
