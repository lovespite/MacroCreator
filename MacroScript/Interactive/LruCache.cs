namespace MacroScript.Interactive;

public class LruCache<TKey, TValue>(int capacity = 100) where TKey : notnull
{
    private readonly int _capacity = capacity;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _cacheMap = [];
    private readonly LinkedList<(TKey Key, TValue Value)> _lruList = new();

    public bool TryGet(TKey key, out TValue value)
    {
        if (_cacheMap.TryGetValue(key, out var node))
        {
            value = node.Value.Value;
            _lruList.Remove(node);
            _lruList.AddFirst(node);
            return true;
        }
        value = default!;
        return false;
    }

    public void Set(TKey key, TValue value)
    {
        if (_cacheMap.TryGetValue(key, out LinkedListNode<(TKey Key, TValue Value)>? node))
        {
            _lruList.Remove(node);
            _cacheMap.Remove(key);
        }
        else if (_cacheMap.Count >= _capacity)
        {
            var lruNode = _lruList.Last!;
            _lruList.RemoveLast();
            _cacheMap.Remove(lruNode.Value.Key);
        }
        var newNode = new LinkedListNode<(TKey Key, TValue Value)>((key, value));
        _lruList.AddFirst(newNode);
        _cacheMap[key] = newNode;
    }
}