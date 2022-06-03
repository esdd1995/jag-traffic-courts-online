using System.Text.RegularExpressions;
using TrafficCourts.Citizen.Service.Models.Tickets;
using TrafficCourts.Common.Features.Lookups;

namespace TrafficCourts.Citizen.Service.Validators.Rules;

/// <summary>
/// Validates a Count Section field - content should match that that of the a Statute pulled from the LookupService.
/// </summary>
public class CountSectionRule : ValidationRule
{
    private readonly IStatuteLookupService _lookupService;

    public CountSectionRule(Field field, IStatuteLookupService lookupService) : base(field)
    {
        ArgumentNullException.ThrowIfNull(lookupService);
        _lookupService = lookupService;
    }

    public override async Task RunAsync()
    {
        if (!String.IsNullOrEmpty(Field.Value))
        {
            Field.Value = Regex.Replace(Field.Value, @"\s+", ""); // remove whitespace
            Field.Value = Regex.Replace(Field.Value, @"^\$$", ""); // remove $ if it's the only character.
            if (!String.IsNullOrEmpty(Field.Value))
            {
                var statute = await _lookupService.GetBySectionAsync(Field.Value);
                if (statute is null)
                {
                    AddValidationError(String.Format(ValidationMessages.CountSectionInvalid, Field.Value));
                }
            }
        }
    }
}
