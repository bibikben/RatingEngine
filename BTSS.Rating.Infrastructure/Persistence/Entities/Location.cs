using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("Locations", Schema="rating")]
public sealed class Location
{
    [Key]
    [Column("LocationId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long LocationId { get; set; }

    [Column("Name")]
    [MaxLength(200)]
    public string? Name { get; set; }

    [Column("Address1")]
    [MaxLength(200)]
    public string? Address1 { get; set; }

    [Column("Address2")]
    [MaxLength(200)]
    public string? Address2 { get; set; }

    [Column("City")]
    [MaxLength(100)]
    public string? City { get; set; }

    [Column("State")]
    [MaxLength(50)]
    public string? State { get; set; }

    [Column("PostalCode")]
    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [Column("CountryCode")]
    [MaxLength(2)]
    public string CountryCode { get; set; }

    [Column("Latitude")]
    [Precision(9, 6)]
    public decimal? Latitude { get; set; }

    [Column("Longitude")]
    [Precision(9, 6)]
    public decimal? Longitude { get; set; }

    [Column("UNLocode")]
    [MaxLength(10)]
    public string? UNLocode { get; set; }

    [Column("AirportCode")]
    [MaxLength(5)]
    public string? AirportCode { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
