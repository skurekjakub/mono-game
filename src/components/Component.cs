using game_mono.objects;

namespace game_mono.components;

public abstract class Component
{
    public GameObject GameObject { get; internal set; }
}
