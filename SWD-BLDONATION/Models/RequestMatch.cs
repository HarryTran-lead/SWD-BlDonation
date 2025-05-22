using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD_BLDONATION.Models;

[Table("RequestMatch")]
public partial class RequestMatch
{
    [Key]
    [Column("match_id")]
    public int MatchId { get; set; }

    [Column("blood_request_id")]
    public int? BloodRequestId { get; set; }

    [Column("donation_request_id")]
    public int? DonationRequestId { get; set; }

    [Column("match_status")]
    [StringLength(10)]
    [Unicode(false)]
    public string? MatchStatus { get; set; }

    [Column("scheduled_date")]
    public DateOnly? ScheduledDate { get; set; }

    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }

    [Column("type")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Type { get; set; }

    [ForeignKey("BloodRequestId")]
    [InverseProperty("RequestMatches")]
    public virtual BloodRequest? BloodRequest { get; set; }

    [ForeignKey("DonationRequestId")]
    [InverseProperty("RequestMatches")]
    public virtual DonationRequest? DonationRequest { get; set; }
}
