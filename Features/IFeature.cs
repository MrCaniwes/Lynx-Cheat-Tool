using CounterStrikeSharp.API.Core;

namespace LynxPlugin.Features;

public interface IFeature
{
    string Name { get; }
    void OnLoad(BasePlugin plugin);
    bool IsEnabled(CCSPlayerController player);
}
