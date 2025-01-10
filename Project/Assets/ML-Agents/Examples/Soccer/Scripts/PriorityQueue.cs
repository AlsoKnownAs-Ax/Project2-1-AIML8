using System.Collections.Generic;
using System.Linq;

public class PriorityQueue<T>
{
    private List<(T item, float priority)> elements = new List<(T, float)>();
    
    public int Count => elements.Count;
    
    public void Enqueue(T item, float priority)
    {
        elements.Add((item, priority));
    }
    
    public T Dequeue()
    {
        var first = elements.First();
        elements.RemoveAt(0);
        return first.item;
    }
    
    public IEnumerable<T> OrderByImportance()
    {
        return elements.OrderByDescending(x => x.priority).Select(x => x.item);
    }
}
