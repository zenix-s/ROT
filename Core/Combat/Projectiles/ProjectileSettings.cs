namespace RotOfTime.Core.Combat.Projectiles;

public class ProjectileSettings
{
    public ProjectileSettings(int initialSpeed, int targetSpeed, double acceleration, int lifetime)
    {
        InitialSpeed = initialSpeed;
        TargetSpeed = targetSpeed;
        Acceleration = acceleration;
        Lifetime = lifetime;
    }

    public ProjectileSettings()
        : this(
            initialSpeed: 200,
            targetSpeed: 200,
            acceleration: 0,
            lifetime: 5
        )
    {
    }

    public int InitialSpeed { get; set; }
    public int TargetSpeed { get; set; }
    public double Acceleration { get; set; }
    public int Lifetime { get; set; }
}
