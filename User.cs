using System;
using System.Collections.Generic;

namespace TodoApi;

public partial class User
{
    public int IdUsers { get; set; }

    public string Name { get; set; } = null!;

    public string Password { get; set; } = null!;
}
