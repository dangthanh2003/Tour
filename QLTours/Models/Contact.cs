using System;
using System.Collections.Generic;

namespace QLTours.Models;

public partial class Contact
{
    public int ContactId { get; set; }

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public string? Status { get; set; }
}
