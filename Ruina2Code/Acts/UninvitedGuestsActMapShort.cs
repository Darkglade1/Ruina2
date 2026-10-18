using MegaCrit.Sts2.Core.Map;

namespace Ruina2.Ruina2Code.Acts;

public sealed class UninvitedGuestsActMapShort : ActMap
{
	private readonly MapPoint?[,] _grid = new MapPoint[7, 8];

	public override MapPoint BossMapPoint { get; }

	public override MapPoint StartingMapPoint { get; }

	protected override MapPoint?[,] Grid => _grid;

	public UninvitedGuestsActMapShort()
	{
		StartingMapPoint = new MapPoint(3, 0)
		{
			PointType = MapPointType.Unknown,
			CanBeModified = false
		};
		MapPoint mapPoint = Create(1, 1, MapPointType.Elite);
		MapPoint mapPoint1 = Create(3, 1, MapPointType.Elite);
		MapPoint mapPoint2 = Create(5, 1, MapPointType.Elite);
		MapPoint mapPoint3 = Create(2, 2, MapPointType.Treasure);
		MapPoint mapPoint4 = Create(4, 2, MapPointType.RestSite);
		MapPoint mapPoint5 = Create(1, 3, MapPointType.Elite);
		MapPoint mapPoint6 = Create(3, 3, MapPointType.Elite);
		MapPoint mapPoint7 = Create(5, 3, MapPointType.Elite);
		MapPoint mapPoint8 = Create(2, 4, MapPointType.Treasure);
		MapPoint mapPoint9 = Create(4, 4, MapPointType.RestSite);
		MapPoint mapPoint10 = Create(1, 5, MapPointType.Elite);
		MapPoint mapPoint11 = Create(3, 5, MapPointType.Elite);
		MapPoint mapPoint12 = Create(5, 5, MapPointType.Elite);
		MapPoint mapPoint14 = Create(3, 6, MapPointType.Shop);
		MapPoint mapPoint15 = Create(3, 7, MapPointType.RestSite);
		BossMapPoint = new MapPoint(3, 8)
		{
			PointType = MapPointType.Boss,
			CanBeModified = false
		};
		StartingMapPoint.AddChildPoint(mapPoint);
		StartingMapPoint.AddChildPoint(mapPoint1);
		StartingMapPoint.AddChildPoint(mapPoint2);
		
		mapPoint.AddChildPoint(mapPoint3);
		mapPoint.AddChildPoint(mapPoint4);
		mapPoint1.AddChildPoint(mapPoint3);
		mapPoint1.AddChildPoint(mapPoint4);
		mapPoint2.AddChildPoint(mapPoint3);
		mapPoint2.AddChildPoint(mapPoint4);
		
		mapPoint3.AddChildPoint(mapPoint5);
		mapPoint3.AddChildPoint(mapPoint6);
		mapPoint3.AddChildPoint(mapPoint7);
		mapPoint4.AddChildPoint(mapPoint5);
		mapPoint4.AddChildPoint(mapPoint6);
		mapPoint4.AddChildPoint(mapPoint7);
		
		mapPoint5.AddChildPoint(mapPoint8);
		mapPoint5.AddChildPoint(mapPoint9);
		mapPoint6.AddChildPoint(mapPoint8);
		mapPoint6.AddChildPoint(mapPoint9);
		mapPoint7.AddChildPoint(mapPoint8);
		mapPoint7.AddChildPoint(mapPoint9);
		
		mapPoint8.AddChildPoint(mapPoint10);
		mapPoint8.AddChildPoint(mapPoint11);
		mapPoint8.AddChildPoint(mapPoint12);
		mapPoint9.AddChildPoint(mapPoint10);
		mapPoint9.AddChildPoint(mapPoint11);
		mapPoint9.AddChildPoint(mapPoint12);
		
		mapPoint10.AddChildPoint(mapPoint14);
		mapPoint11.AddChildPoint(mapPoint14);
		mapPoint12.AddChildPoint(mapPoint14);
		
		mapPoint14.AddChildPoint(mapPoint15);
		mapPoint15.AddChildPoint(BossMapPoint);
		
		startMapPoints.Add(mapPoint);
		startMapPoints.Add(mapPoint1);
		startMapPoints.Add(mapPoint2);
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