using InsuranceAPI.Features.Insurance.Entities;

namespace InsuranceAPI.Features.FeatureManagement.Entities
{
    public class FeatureToggle : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
    }
}