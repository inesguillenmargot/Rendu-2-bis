namespace LivinParisVF;

public class Lien<T>
{
    public T Destination { get; set; }
    public int Poids { get; set; }

    public Lien(T destination, int poids)
    {
        Destination = destination;
        Poids = poids;
    }
}