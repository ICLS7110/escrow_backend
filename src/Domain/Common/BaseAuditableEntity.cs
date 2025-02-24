namespace Escrow.Api.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }

    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;
    public string? LastModifiedBy { get; set; }

    public RecordState RecordState { get; set; } = RecordState.Active; 

    public DateTimeOffset? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }


    public void MarkAsDeleted(int userId)
    {
        RecordState = RecordState.Deleted;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = userId;
    }


    public void UpdateAuditInfo(string userId)
    {
        LastModified = DateTimeOffset.UtcNow;
        LastModifiedBy = userId;
    }
}
