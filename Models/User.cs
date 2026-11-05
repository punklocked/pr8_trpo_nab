using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pr8_trpo.Models;

public partial class User
{
    [Key]
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;
}
