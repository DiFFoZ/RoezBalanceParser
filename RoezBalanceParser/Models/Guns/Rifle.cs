using CsvHelper.Configuration.Attributes;

namespace RoezBalanceParser.Models.Guns;
public class Rifle : IGun
{
    [Index(0)]
    [Default(null)]
    public string Name { get; set; }

    [Index(1)]
    [Default(null)]
    public string Tier { get; set; }

    [Index(2)]
    [Default(null)]
    public string Id { get; set; }

    [Index(4)]
    [Default(0)]
    public byte Ammo { get; set; }

    [Index(5)]
    [Default(0)]
    public byte? Firerate { get; set; }

    [Index(6)]
    [Default(0)]
    public float? PlayerDamage { get; set; }

    [Index(7)]
    [Default(0)]
    public float? VehicleDamage { get; set; }

    [Index(8)]
    [Default(0)]
    public float? ZombieDamage { get; set; }

    [Index(9)]
    [Default(0)]
    public float? BarricadeDamage { get; set; }

    [Index(10)]
    [Default(0)]
    public float? AnimalDamage { get; set; }

    [Index(11)]
    [Default(0)]
    public float? ResourceDamage { get; set; }

    public float? Range { get; set; }

    [Index(9)]
    [Default(0)]
    public float? StructureDamage { get; set; }

    [Index(12)]
    [Default(0)]
    public float? ObjectDamage { get; set; }
}
