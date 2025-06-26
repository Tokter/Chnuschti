using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public class ResourceDictionary
{
    private Dictionary<Type, Dictionary<Type, Dictionary<string, object>>> _storage = new Dictionary<Type, Dictionary<Type, Dictionary<string, object>>>();

    /// <summary>
    /// Adds a resource to the dictionary, associating it with the specified key and storage types.
    /// </summary>
    /// <typeparam name="TStorage">The type of the storage category to which the resource belongs.</typeparam>
    /// <typeparam name="TData">The type of the resource being added.</typeparam>
    /// <param name="resource">The resource to add. Cannot be <see langword="null"/>.</param>
    /// <param name="key">The key used to identify the resource within the specified storage and data types. Defaults to an empty string
    /// if not provided.</param>
    /// <returns>The current <see cref="ResourceDictionary"/> instance, allowing for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="resource"/> is <see langword="null"/>.</exception>
    public ResourceDictionary Add<TStorage, TData>(TData resource, string key = "")
    {
        if (resource == null) throw new ArgumentNullException(nameof(resource));

        _storage.TryGetValue(typeof(TStorage), out var storageType);
        if (storageType == null)
        {
            storageType = new();
            _storage.Add(typeof(TStorage), storageType);
        }

        storageType.TryGetValue(typeof(TData), out var dataType);
        if (dataType == null)
        {
            dataType = new();
            storageType.Add(typeof(TData), dataType);
        }

        dataType[key] = resource;

        return this;
    }

    /// <summary>
    /// Retrieves a resource from the storage dictionary based on the specified storage type, data type, and key.
    /// </summary>
    /// <typeparam name="TStorage">The type of the storage container to search within.</typeparam>
    /// <typeparam name="TData">The type of the data to retrieve from the storage container.</typeparam>
    /// <param name="key">The key identifying the specific resource to retrieve. If not specified, an empty string is used as the default
    /// key.</param>
    /// <returns>The resource associated with the specified storage type, data type, and key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the specified storage type, data type, or key does not exist in the storage dictionary.</exception>
    public TData Get<TStorage,TData>(string key = "")
    {
        _storage.TryGetValue(typeof (TStorage), out var storageType);
        if (storageType == null) throw new KeyNotFoundException($"Could not find storage type '{typeof(TStorage).Name}' in this ResourceDictionary!");

        storageType.TryGetValue(typeof(TData), out var dataType);
        if (dataType == null) throw new KeyNotFoundException($"Could not find data type '{typeof(TData).Name}' in storage type '{typeof(TStorage).Name}' in this ResourceDictionary!");

        dataType.TryGetValue(key, out var resource);
        if (resource == null) throw new KeyNotFoundException($"Could not find key '{key}' of data type {typeof(TData).Name} in storage type '{typeof(TStorage).Name}' in this ResourceDictionary!");

        return (TData)resource;
    }
}
