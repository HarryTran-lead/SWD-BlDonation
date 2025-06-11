using System;
using System.Collections.Generic;

namespace SWD_BLDONATION.Models.Generated;

public partial class RequestMatch
{
    public int MatchId { get; set; }

    public int? BloodRequestId { get; set; }

    public int? DonationRequestId { get; set; }

    public string? MatchStatus { get; set; }

    public DateOnly? ScheduledDate { get; set; }

    public string? Notes { get; set; }

    public string? Type { get; set; }

    public virtual BloodRequest? BloodRequest { get; set; }

    public virtual DonationRequest? DonationRequest { get; set; }
}
