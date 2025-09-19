using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lssctc.Share.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Fullname { get; set; }
    public int? Role { get; set; }

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual Instructor? Instructor { get; set; }

    public virtual ProgramManager? ProgramManager { get; set; }

    public virtual SimulationManager? SimulationManager { get; set; }

    public virtual Trainee? Trainee { get; set; }
}
