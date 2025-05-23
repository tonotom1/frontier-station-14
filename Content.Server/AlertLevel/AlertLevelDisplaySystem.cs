using Content.Server.Power.Components;
using Content.Server.Station.Systems;
using Content.Shared.AlertLevel;
using Content.Shared.Power;
using Content.Server._NF.SectorServices; // Frontier

namespace Content.Server.AlertLevel;

public sealed class AlertLevelDisplaySystem : EntitySystem
{
    // [Dependency] private readonly StationSystem _stationSystem = default!; // Frontier
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SectorServiceSystem _sectorService = default!; // Frontier

    public override void Initialize()
    {
        SubscribeLocalEvent<AlertLevelChangedEvent>(OnAlertChanged);
        SubscribeLocalEvent<AlertLevelDisplayComponent, ComponentInit>(OnDisplayInit);
        SubscribeLocalEvent<AlertLevelDisplayComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnAlertChanged(AlertLevelChangedEvent args)
    {
        var query = EntityQueryEnumerator<AlertLevelDisplayComponent, AppearanceComponent>();
        while (query.MoveNext(out var uid, out _, out var appearance))
        {
            _appearance.SetData(uid, AlertLevelDisplay.CurrentLevel, args.AlertLevel, appearance);
        }
    }

    private void OnDisplayInit(EntityUid uid, AlertLevelDisplayComponent alertLevelDisplay, ComponentInit args)
    {
        if (TryComp(uid, out AppearanceComponent? appearance))
        {
            //var stationUid = _stationSystem.GetOwningStation(uid); // Frontier: sector-wide alerts
            var stationUid = _sectorService.GetServiceEntity(); // Frontier: sector-wide alerts
            if (stationUid.Valid && TryComp(stationUid, out AlertLevelComponent? alert)) // Frontier: uid != null < uid.Valid
            {
                _appearance.SetData(uid, AlertLevelDisplay.CurrentLevel, alert.CurrentLevel, appearance);
            }
        }
    }
    private void OnPowerChanged(EntityUid uid, AlertLevelDisplayComponent alertLevelDisplay, ref PowerChangedEvent args)
    {
        if (!TryComp(uid, out AppearanceComponent? appearance))
            return;

        _appearance.SetData(uid, AlertLevelDisplay.Powered, args.Powered, appearance);
    }
}
