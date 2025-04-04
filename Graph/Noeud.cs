namespace LivinParisVF;

public class Noeud<T>
{
    public T Id { get; set; }
    public List<Arc<T>> Voisins { get; set; }
    

    //public List<T> Voisins { get; set; }

    public Noeud(T id)
    {
        Id = id;
        Voisins = new List<Arc<T>>();
    }
}