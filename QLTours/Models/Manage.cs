using System;
using System.Collections.Generic;

namespace QLTours.Models;

public partial class Manage
{
    public int IdMng { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool Role { get; set; }

    public string Status { get; set; } = "Unlocked";

}
