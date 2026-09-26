using BaseLib.Utils;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Encounters.UninvitedGuests;

namespace Ruina2.Ruina2Code.Acts;

public sealed class UninvitedGuestsActMap : ActMap
{
	private readonly MapPoint?[,] _grid = new MapPoint[7, 17];

	public override MapPoint BossMapPoint { get; }

	public override MapPoint StartingMapPoint { get; }

	protected override MapPoint?[,] Grid => _grid;
	public static readonly SpireField<MapPoint, EncounterModel> MapPointSpecificEncounter = new(() => null);
	public static MapCoord CurrentMapCoord;

	public UninvitedGuestsActMap()
	{
		StartingMapPoint = new MapPoint(3, 0)
		{
			PointType = MapPointType.Unknown,
			CanBeModified = false
		};
		MapPoint mapPoint = Create(3, 1, MapPointType.Elite);
		//MapPointSpecificEncounter.Set(mapPoint, ModelDb.Encounter<PhilipEncounter>());
		MapPoint mapPoint1 = Create(3, 2, MapPointType.Elite);
		//MapPointSpecificEncounter.Set(mapPoint1, ModelDb.Encounter<EileenEncounter>());
		MapPoint mapPoint2 = Create(3, 3, MapPointType.Elite);
		//MapPointSpecificEncounter.Set(mapPoint2, ModelDb.Encounter<GretaEncounter>());
		MapPoint mapPoint3 = Create(3, 4, MapPointType.Treasure);
		MapPoint mapPoint4 = Create(3, 5, MapPointType.RestSite);
		MapPoint mapPoint5 = Create(3, 6, MapPointType.Elite);
		//MapPointSpecificEncounter.Set(mapPoint5, ModelDb.Encounter<BremenEncounter>());
		MapPoint mapPoint6 = Create(3, 7, MapPointType.Elite);
		//MapPointSpecificEncounter.Set(mapPoint6, ModelDb.Encounter<OswaldEncounter>());
		MapPoint mapPoint7 = Create(3, 8, MapPointType.Elite);
		//MapPointSpecificEncounter.Set(mapPoint7, ModelDb.Encounter<TanyaEncounter>());
		MapPoint mapPoint8 = Create(3, 9, MapPointType.Treasure);
		MapPoint mapPoint9 = Create(3, 10, MapPointType.RestSite);
		MapPoint mapPoint10 = Create(3, 11, MapPointType.Elite);
		//MapPointSpecificEncounter.Set(mapPoint10, ModelDb.Encounter<PuppeteerEncounter>());
		MapPoint mapPoint11 = Create(3, 12, MapPointType.Elite);
		//MapPointSpecificEncounter.Set(mapPoint11, ModelDb.Encounter<ElenaEncounter>());
		MapPoint mapPoint12 = Create(3, 13, MapPointType.Elite);
		//MapPointSpecificEncounter.Set(mapPoint12, ModelDb.Encounter<PlutoEncounter>());
		MapPoint mapPoint13 = Create(3, 14, MapPointType.Treasure);
		MapPoint mapPoint14 = Create(3, 15, MapPointType.Shop);
		MapPoint mapPoint15 = Create(3, 16, MapPointType.RestSite);
		BossMapPoint = new MapPoint(3, 17)
		{
			PointType = MapPointType.Boss,
			CanBeModified = false
		};
		StartingMapPoint.AddChildPoint(mapPoint);
		mapPoint.AddChildPoint(mapPoint1);
		mapPoint1.AddChildPoint(mapPoint2);
		mapPoint2.AddChildPoint(mapPoint3);
		mapPoint3.AddChildPoint(mapPoint4);
		mapPoint4.AddChildPoint(mapPoint5);
		mapPoint5.AddChildPoint(mapPoint6);
		mapPoint6.AddChildPoint(mapPoint7);
		mapPoint7.AddChildPoint(mapPoint8);
		mapPoint8.AddChildPoint(mapPoint9);
		mapPoint9.AddChildPoint(mapPoint10);
		mapPoint10.AddChildPoint(mapPoint11);
		mapPoint11.AddChildPoint(mapPoint12);
		mapPoint12.AddChildPoint(mapPoint13);
		mapPoint13.AddChildPoint(mapPoint14);
		mapPoint14.AddChildPoint(mapPoint15);
		mapPoint15.AddChildPoint(BossMapPoint);
		startMapPoints.Add(mapPoint);
	}

	private MapPoint Create(int column, int row, MapPointType type)
	{
		MapPoint mapPoint = new MapPoint(column, row)
		{
			PointType = type,
			CanBeModified = false
		};
		_grid[column, row] = mapPoint;
		return mapPoint;
	}
}