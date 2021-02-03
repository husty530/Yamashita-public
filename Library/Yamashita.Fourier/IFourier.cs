namespace Yamashita.Fourier
{
    public interface IFourier
    {

        public void Dft(out float[] result);

        public void Idft(out float[] result);

    }
}
