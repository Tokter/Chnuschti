using Chnuschti.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public interface IElement { } // marker for your base element type

public interface IHasChildren<TChild> : IElement
    where TChild : VisualElement
{
    IReadOnlyList<VisualElement> Children { get; }   // e.g. StackPanel, Grid, etc.
    TChild AddChild(TChild child);
}


public interface IHasContent : IElement
{
    Control? Content { get; set; }   // e.g. ContentControl-like
}

public static class FluentUI
{
    /// <summary>
    /// Configures the specified element using the provided action and returns the element.
    /// </summary>
    /// <typeparam name="T">The type of the element, which must implement <see cref="IElement"/>.</typeparam>
    /// <param name="element">The element to be configured. Cannot be <see langword="null"/>.</param>
    /// <param name="config">An action that defines the configuration to apply to the element. Can be <see langword="null"/>.</param>
    /// <returns>The configured element.</returns>
    public static T With<T>(this T element, Action<T> config)
        where T : IElement
    {
        config?.Invoke(element);
        return element;
    }

    /// <summary>
    /// Adds the specified child elements to the parent element and returns the parent.
    /// </summary>
    /// <remarks>Only elements of type <typeparamref name="TChild"/> from the <paramref name="children"/>
    /// array will be added to the parent. If <paramref name="children"/> is <see langword="null"/> or contains no
    /// elements, the parent remains unchanged.</remarks>
    /// <typeparam name="TParent">The type of the parent element, which must implement <see cref="IHasChildren{TChild}"/>.</typeparam>
    /// <typeparam name="TChild">The type of the child elements, which must derive from <see cref="VisualElement"/>.</typeparam>
    /// <param name="parent">The parent element to which the child elements will be added. Cannot be <see langword="null"/>.</param>
    /// <param name="children">An array of child elements to add to the parent. Can be <see langword="null"/> or empty.</param>
    /// <returns>The parent element, after the child elements have been added.</returns>
    public static TParent Children<TParent, TChild>(this TParent parent, object v, params VisualElement[] children)
            where TParent : IHasChildren<TChild>
            where TChild : VisualElement
    {
        if (children != null)
            foreach (var c in children)
                if (c is TChild child)
                    parent.AddChild(child);
        return parent;
    }

    public static TParent Children<TParent>(this TParent parent, params VisualElement[] children)
        where TParent : IHasChildren<VisualElement>
    {
        if (children != null)
            foreach (var c in children)
                    parent.AddChild(c);
        return parent;
    }

    /// <summary>
    /// Adds a child element to the parent and returns the added child.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent element, which must implement <see cref="IHasChildren{TChild}"/>.</typeparam>
    /// <typeparam name="TChild">The type of the child element, which must derive from <see cref="VisualElement"/>.</typeparam>
    /// <param name="parent">The parent element to which the child will be added. Cannot be <see langword="null"/>.</param>
    /// <param name="child">The child element to add to the parent. Cannot be <see langword="null"/>.</param>
    /// <returns>The child element that was added to the parent.</returns>
    public static VisualElement AddGet<TParent, TChild>(this TParent parent, TChild child)
        where TParent : IHasChildren<TChild>
        where TChild : VisualElement
    {
        parent.AddChild(child);
        return child;
    }

    /// <summary>
    /// Adds a child element to the parent element, optionally configuring the child before adding it.
    /// </summary>
    /// <remarks>This method facilitates the creation and addition of a child element to a parent element, 
    /// while providing an optional configuration step for the child. It is commonly used in scenarios  where elements
    /// are dynamically constructed and added to a hierarchical structure.</remarks>
    /// <typeparam name="TParent">The type of the parent element, which must implement <see cref="IHasChildren{TChild}"/>.</typeparam>
    /// <typeparam name="TChild">The type of the child element, which must implement <see cref="IElement"/>.</typeparam>
    /// <param name="parent">The parent element to which the child will be added.</param>
    /// <param name="factory">A function that creates the child element based on the parent.</param>
    /// <param name="config">An optional action to configure the child element before it is added. Can be <see langword="null"/>.</param>
    /// <returns>The parent element, allowing for method chaining.</returns>
    public static TParent Add<TParent, TChild>(this TParent parent, Func<TParent, TChild> factory, Action<TChild>? config = null)
        where TParent : IHasChildren<TChild>
        where TChild : VisualElement
    {
        var child = factory(parent);
        config?.Invoke(child);
        parent.AddChild(child);
        return parent;
    }

    /// <summary>
    /// Sets the content of the specified parent element and returns the parent element.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent element, which must implement <see cref="IHasContent{TContent}"/>.</typeparam>
    /// <typeparam name="TContent">The type of the content element, which must implement <see cref="IElement"/>.</typeparam>
    /// <param name="parent">The parent element whose content is to be set.</param>
    /// <param name="content">The content to set for the parent element. Can be <see langword="null"/>.</param>
    /// <returns>The parent element with the updated content.</returns>
    public static TParent Content<TParent>(this TParent parent, Control? content)
        where TParent : IHasContent
    {
        parent.Content = content;
        return parent;
    }

    /// <summary>
    /// Adds the specified child element to the parent's content and returns the child element.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent element, which must implement <see cref="IHasContent{TChild}"/>.</typeparam>
    /// <typeparam name="TChild">The type of the child element, which must implement <see cref="IElement"/>.</typeparam>
    /// <param name="child">The child element to be added to the parent's content.</param>
    /// <param name="parent">The parent element to which the child element will be added.</param>
    /// <returns>The child element after it has been added to the parent's content.</returns>
    public static TChild Into<TParent, TChild>(this TChild child, TParent parent)
        where TParent : IHasContent
        where TChild : Control
    {
        parent.Content = child;
        return child;
    }

    /// <summary>
    /// Adds the specified child element to the parent element and returns the child.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent element, which must implement <see cref="IHasChildren{TChild}"/>.</typeparam>
    /// <typeparam name="TChild">The type of the child element, which must inherit from <see cref="VisualElement"/>.</typeparam>
    /// <param name="child">The child element to add to the parent.</param>
    /// <param name="parent">The parent element to which the child will be added.</param>
    /// <returns>The child element after it has been added to the parent.</returns>
    public static TChild AddTo<TParent, TChild>(this TChild child, TParent parent)
        where TParent : IHasChildren<TChild>
        where TChild : VisualElement
    {
        parent.AddChild(child);
        return child;
    }

    /// <summary>
    /// Assigns the specified value to an output variable and returns the same value.
    /// </summary>
    /// <typeparam name="T">The type of the value to assign and return.</typeparam>
    /// <param name="value">The value to assign to the output variable and return.</param>
    /// <param name="variable">When this method returns, contains the value of <paramref name="value"/>.</param>
    /// <returns>The value of <paramref name="value"/>.</returns>
    public static T Out<T>(this T value, out T? variable)
    {
        variable = value;
        return value;
    }

    //public static T SetText<T>(this T element, string text)
    //    where T : IHasText
    //{
    //    element.Text = text;
    //    return element;
    //}

    //public static T OnClick<T>(this T button, Action<T> handler)
    //    where T : IHasClick
    //{
    //    button.Click += (_, __) => handler(button);
    //    return button;
    //}

    //// Example “shape” helpers for common props/events (optional)
    //public interface IHasText { string? Text { get; set; } }
    //public interface IHasClick
    //{
    //    event EventHandler Click;
    //}
}
