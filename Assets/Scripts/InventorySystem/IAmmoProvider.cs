public interface IAmmoProvider
{
    void SetAmmo(int clip, int reserve);
    void GetAmmo(out int clip, out int reserve);
}
