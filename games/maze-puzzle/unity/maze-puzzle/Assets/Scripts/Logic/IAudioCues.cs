namespace OrbitalSalvage.Logic
{
    public interface IAudioCues
    {
        void PlaySpinRatchet(double pitchMultiplier);
        void PlayDebrisCollision(double intensity, string debrisType);
        void PlayDebrisSwallowed();
        void PlayGatePass(string gateType, double value);
        void PlayRcsThrust();
        void PlayBumperBounce();
        void PlayHazardExplosion();
    }
}
