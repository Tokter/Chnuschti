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
    public TData? Get<TStorage,TData>(string key = "")
    {
        return Get<TData>(typeof(TStorage), key);
    }

    /// <summary>
    /// Retrieves a resource of the specified data type from the given storage type using an optional key.
    /// </summary>
    /// <typeparam name="TData">The type of the data to retrieve.</typeparam>
    /// <param name="tStorage">The type of the storage from which to retrieve the data.</param>
    /// <param name="key">An optional key to identify the specific resource. Defaults to an empty string.</param>
    /// <returns>The resource of type <typeparamref name="TData"/> associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the specified storage type, data type, or key is not found in the resource dictionary.</exception>
    public TData? Get<TData>(Type tStorage, string key = "")
    {
        _storage.TryGetValue(tStorage, out var storageType);
        if (storageType == null) return default(TData);

        storageType.TryGetValue(typeof(TData), out var dataType);
        if (dataType == null) return default(TData);

        dataType.TryGetValue(key, out var resource);
        if (resource == null)
        {
            //If a key was provided and no resource was found, retun the default
            if (!string.IsNullOrEmpty(key))
            {
                dataType.TryGetValue(string.Empty, out resource);
            }
        }

        return (TData?)resource;
    }

    internal void RecreateStyles(Theme theme, Type[] types)
    {
        foreach(var storage in _storage.Values)
        {
            foreach (var data in storage.Values)
            {
                foreach (var type in types.Where(t=> typeof(Style).IsAssignableFrom(t))) //Look for style types
                {
                    foreach(var key in data.Keys.ToList())
                    {
                        if (data[key].GetType() == type)
                        {
                            data[key] = Activator.CreateInstance(type, theme)!;
                        }
                    }
                }
            }
        }
    }
}
