using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pr8_trpo.Models;

public partial class Task
{
    [Key]
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string OwnerLogin { get; set; } = null!;

    public bool IsCompleted { get; set; }
}
