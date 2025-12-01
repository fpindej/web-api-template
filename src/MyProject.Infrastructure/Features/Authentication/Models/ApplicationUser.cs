using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MyProject.Infrastructure.Features.Authentication.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class ApplicationUser : IdentityUser
{
    [MaxLength(255)]
    public string? FirstName { get; set; }

    [MaxLength(255)]
    public string? LastName { get; set; }
}
