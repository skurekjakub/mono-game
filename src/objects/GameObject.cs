using Microsoft.Xna.Framework;
using game_mono.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace game_mono;

public class GameObject
{
    public Transform Transform { get; }
    public bool IsDestroyed { get; private set; }
    private readonly List<Component> _components = new List<Component>();
    private readonly List<IUpdatable> _updatableComponents = new List<IUpdatable>();
    private readonly List<interfaces.IDrawable> _drawableComponents = new List<interfaces.IDrawable>();

    public GameObject()
    {
        Transform = new Transform();
        AddComponent(Transform);
    }

    public T AddComponent<T>(T component) where T : Component
    {
        if (component == null)
            throw new ArgumentNullException(nameof(component));

        _components.Add(component);
        component.GameObject = this;

        if (component is IUpdatable updatable)
            _updatableComponents.Add(updatable);
        
        if (component is interfaces.IDrawable drawable)
            _drawableComponents.Add(drawable);

        return component;
    }

    public T GetComponent<T>() where T : Component
    {
        return _components.OfType<T>().FirstOrDefault();
    }

    public void Destroy()
    {
        IsDestroyed = true;
    }

    public void Update(GameTime gameTime)
    {
        foreach (var component in _updatableComponents)
        {
            component.Update(gameTime);
        }
    }

    public void Draw(PrimitiveRenderer renderer)
    {
        foreach (var component in _drawableComponents)
        {
            component.Draw(renderer);
        }
    }
}
