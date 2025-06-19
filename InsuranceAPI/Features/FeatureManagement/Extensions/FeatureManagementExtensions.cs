using System;
using InsuranceAPI.Features.FeatureManagement.DTOs;
using InsuranceAPI.Features.FeatureManagement.Entities;

namespace InsuranceAPI.Features.FeatureManagement.Extensions
{
    public static class FeatureManagementExtensions
    {
        public static FeatureToggle ToFeatureToggle(this FeatureToggleInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            return new FeatureToggle
            {
                Name = input.Name,
                IsEnabled = input.IsEnabled
            };
        }

        public static FeatureToggleInput ToFeatureToggleInput(this FeatureToggle toggle)
        {
            if (toggle == null) throw new ArgumentNullException(nameof(toggle));

            return new FeatureToggleInput(toggle.Name, toggle.Description, toggle.IsEnabled);
        }
        
        public static FeatureToggleOutput ToFeatureToggleOutput(this FeatureToggle toggle)
        {
            if (toggle == null) throw new ArgumentNullException(nameof(toggle));

            return new FeatureToggleOutput(toggle.Id, toggle.Name, toggle.Description, toggle.IsEnabled);
        }

        public static List<string> ToFeatureToggleNames(this FeatureToggleNameInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            return input.Names ?? throw new ArgumentNullException(nameof(input.Names), "Feature toggle names cannot be null.");
        }

        public static FeatureToggleNameOutput ToFeatureToggleNameOutput(this IEnumerable<bool> togglesStatus)
        {
            if (togglesStatus == null) throw new ArgumentNullException(nameof(togglesStatus));
            return new FeatureToggleNameOutput(togglesStatus.ToList());
        }
    }

}