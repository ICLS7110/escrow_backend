namespace Escrow.Api.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public RecordState RecordState { get; set; } = RecordState.Active; // Default to Active
    public DateTimeOffset? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

}
