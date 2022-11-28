namespace RoezBalanceParser.Models.Guns;
internal interface IGun
{
    public string Name { get; set; }

    public string Tier { get; set; }

    public string Id { get; set; }

    public byte Ammo { get; set; }

    public byte? Firerate { get; set; }

    public float? Range { get; set; }

    public float? PlayerDamage { get; set; }

    public float? VehicleDamage { get; set; }

    public float? ZombieDamage { get; set; }

    public float? StructureDamage { get; set; }

    public float? BarricadeDamage { get; set; }

    public float? AnimalDamage { get; set; }

    public float? ResourceDamage { get; set; }

    public float? ObjectDamage { get; set; }
}
