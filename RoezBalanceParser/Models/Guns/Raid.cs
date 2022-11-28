using CsvHelper.Configuration.Attributes;

namespace RoezBalanceParser.Models.Guns;
internal class Raid : IGun
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

    [Index(5)]
    [Default(0)]
    public byte Ammo { get; set; }

    [Index(7)]
    [Default(0)]
    public byte? Firerate { get; set; }

    [Index(8)]
    [Default(0)]
    public float? Range { get; set; }

    public float? PlayerDamage { get; set; }

    public float? VehicleDamage { get; set; }

    public float? ZombieDamage { get; set; }

    [Index(6)]
    [Default(0)]
    public float? StructureDamage { get; set; }

    [Index(6)]
    [Default(0)]
    public float? BarricadeDamage { get; set; }

    public float? AnimalDamage { get; set; }

    public float? ResourceDamage { get; set; }

    public float? ObjectDamage { get; set; }
}
