using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD_BLDONATION.Models;

[Table("BlogPost")]
public partial class BlogPost
{
    [Key]
    [Column("post_id")]
    public int PostId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("title")]
    [StringLength(255)]
    [Unicode(false)]
    public string? Title { get; set; }

    [Column("content", TypeName = "text")]
    public string? Content { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column("category")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Category { get; set; }

    [Column("img")]
    [StringLength(255)]
    [Unicode(false)]
    public string? Img { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("BlogPosts")]
    public virtual User? User { get; set; }
}
