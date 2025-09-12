namespace SDIA.SharedKernel.Interfaces;

public interface IEntity
{
    Guid Id { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    bool IsDeleted { get; set; }
}