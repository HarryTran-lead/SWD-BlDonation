using System;
using System.Collections.Generic;

namespace SWD_BLDONATION.Models.Generated;

public partial class BlogPost
{
    public int PostId { get; set; }

    public int? UserId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Category { get; set; }

    public string? ImgPath { get; set; }

    public virtual User? User { get; set; }
}
