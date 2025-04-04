namespace LivinParisVF;

public class Lien<T>
{
    public T Destination { get; set; }
    public double Poids { get; set; }

    public Lien(T destination, double poids)
    {
        Destination = destination;
        Poids = poids;
    }
}