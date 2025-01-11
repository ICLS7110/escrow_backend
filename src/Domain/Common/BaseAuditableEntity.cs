namespace Escrow.Api.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset LastModifiedDate { get; set; }

    public string? LastModifiedBy { get; set; }
}
